﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using System.Windows.Threading;
using System.IO;
using System.Text;

namespace LiveReload
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow window;
        private NodeRPC nodeFoo;
        private ObjectRPC.RootEntity rpcRoot;
        private string baseDir, logDir, resourcesDir, appDataDir;
        private string localAppDataDir;
        private string extractedResourcesDir;
        private TextWriter logWriter;
        private TrayIconController trayIcon;
        private string logFile;
        private CommandLineOptions options;

        public void SendCommand(string command, object arg)
        {
            nodeFoo.Send(command, arg);
        }

        public static string Version
        {
            get
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(asm.Location);
                //return String.Format("{0}.{1}", fvi.ProductMajorPart, fvi.ProductMinorPart);
                return fvi.FileVersion;
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Environment.SetEnvironmentVariable("DEBUG", "*");

            // root dirs
            baseDir         = System.AppDomain.CurrentDomain.BaseDirectory;
            localAppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiveReload");
            appDataDir      = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"LiveReload\");

            // derived dirs
            resourcesDir          = Path.Combine(baseDir, @"res");
            logDir                = Path.Combine(localAppDataDir, @"Logs");
            extractedResourcesDir = Path.Combine(localAppDataDir, @"Bundled");

            Directory.CreateDirectory(appDataDir);
            Directory.CreateDirectory(logDir);

            logFile = Path.Combine(logDir, "LiveReload_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmss") + ".txt");
            logWriter = TextWriter.Synchronized(new StreamWriter(logFile));
            logWriter.WriteLine("LiveReload v" + Version + " says hi.");
            logWriter.WriteLine("OS version: " + Environment.OSVersion);
            logWriter.WriteLine("Paths:");
            logWriter.WriteLine("  resourcesDir  = \"" + resourcesDir + "\"");
            logWriter.WriteLine("  appDataDir    = \"" + appDataDir + "\"");
            logWriter.WriteLine("  logDir        = \"" + logDir + "\"");
            logWriter.Flush();

            try
            {
                options = CommandLineOptions.Parse(e.Args);
            }
            catch (CommandLineArgException err)
            {
                DisplayCommandLineError(err.Message);
                Shutdown();
                return;
            }

            StartUI();
        }

        private void StartUI()
        {
            window = new MainWindow();
            window.MainWindowHideEvent         += HandleMainWindowHideEvent;
            window.NodeMessageEvent            += HandleNodeMessageEvent;
            window.buttonVersion.Content = "v" + Version;
            window.gridProgress.Visibility = Visibility.Visible;
            window.Show();

            trayIcon = new TrayIconController();
            trayIcon.MainWindowHideEvent += HandleMainWindowHideEvent;
            trayIcon.MainWindowShowEvent += HandleMainWindowShowEvent;
            trayIcon.MainWindowToggleEvent  += HandleMainWindowToggleEvent;

            // has to be done before launching Node
            BeginExtractBundledResources(Application_ContinueStartupAfterExtraction);
        }

        private void Application_ContinueStartupAfterExtraction()
        {
            if (options.LRBackendOverride != null)
            {
                bundledBackendDir = options.LRBackendOverride;
                logWriter.WriteLine("LRBackendOverride = \"" + options.LRBackendOverride + "\"");
            }
            if (options.LRBundledPluginsOverride != null) {
                Environment.SetEnvironmentVariable("LRBundledPluginsOverride", options.LRBundledPluginsOverride);
                logWriter.WriteLine("LRBundledPluginsOverride = \"" + options.LRBundledPluginsOverride + "\"");
            }

            window.gridProgress.Visibility = Visibility.Hidden;

            nodeFoo = new NodeRPC(bundledNodeDir, bundledBackendDir, logWriter);
            nodeFoo.NodeMessageEvent += HandleNodeMessageEvent;
            nodeFoo.NodeStartedEvent += HandleNodeStartedEvent;
            nodeFoo.NodeCrash += HandleNodeCrash;
            nodeFoo.Start();

            rpcRoot = new ObjectRPC.RootEntity();
            ObjectRPC.WPF.UIFacets.Register(rpcRoot);
            rpcRoot.OutgoingUpdate += (payload => nodeFoo.Send("rpc", payload));

            rpcRoot.Expose("app", this);
            rpcRoot.Expose("mainwnd", window);
        }

        private void HandleNodeMessageEvent(string nodeLine)
        {
            var b = (object[])fastJSON.JSON.Instance.ToObject(nodeLine);
            string messageType = (string) b[0];
            if (messageType == "app.displayCriticalError")
            {
                var arg = (Dictionary<string, object>) b[1];

                var title  = (string)arg["title"];
                var text   = (string)arg["text"];
                var url    = (string)arg["url"];
                var button = (string)arg["button"];

                MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
            }
            else if (messageType == "rpc")
            {
                var arg = (Dictionary<string, object>)b[1];

                ObjectRPC.PayloadDelegate reply = null;
                if (b.Length > 2)
                {
                    string callback = (string)b[2];
                    reply = (payload => SendCommand(callback, payload));
                }

                rpcRoot.ProcessIncomingUpdate(arg, reply);
            }
        }

        private void HandleNodeStartedEvent()
        {
            string version = Version;
            string build = "beta";
            string platform = "windows";

            var foo = new object[] { "app.init",
                                     new Dictionary<string, object> {
                                        {"resourcesDir", resourcesDir},
                                        {"appDataDir",   appDataDir},
                                        {"rubies",       new object[] {
                                            new Dictionary<string, object> {
                                                {"version", "1.9.3"},
                                                {"path",    bundledRubyDir}
                                            }
                                        }},
                                        {"logDir",       logDir},
                                        {"version",      version},
                                        {"build",        build},
                                        {"platform",     platform}
            }};

            string response = fastJSON.JSON.Instance.ToJSON(foo);
            nodeFoo.NodeMessageSend(response);
        }

        public void OpenExplorerWithLog()
        {
            System.Diagnostics.Process explorerWindowProcess = new System.Diagnostics.Process();
            explorerWindowProcess.StartInfo.FileName = "explorer.exe";
            explorerWindowProcess.StartInfo.Arguments = "/select,\"" + logFile + "\"";
            explorerWindowProcess.Start();
        }

        private void HandleNodeCrash()
        {
            logWriter.WriteLine("Node.js appears to have crashed.");
            logWriter.Flush();

            if (CanRestartBackend)
            {
                MessageBox.Show("Node.js backend has crashed. Press OK to restart.", "LiveReload crash", MessageBoxButton.OK, MessageBoxImage.Error);
                RestartBackend();
                return;
            }

            EmergencyShutdown("LiveReload backend process (LiveReloadNodejs) appears to have crashed.");
       }

        private void EmergencyShutdown(string reason)
        {
            string message = reason + "\n\nPress OK to report the crash, reveal the log file and quit the app.";
            MessageBoxResult result = MessageBox.Show(message, "LiveReload crash", MessageBoxButton.OK, MessageBoxImage.Error);
            if (result == MessageBoxResult.OK)
            {
                string crashUrl = @"http://go.livereload.com/crashed/windows/";
                System.Diagnostics.Process.Start(crashUrl);
                OpenExplorerWithLog();
            }

            App.Current.Shutdown(1);
        }

        public bool CanRestartBackend
        {
            get
            {
                return options.LRBackendOverride != null;
            }
        }

        public void RestartBackend()
        {
            window.Hide();
            window = null;
            rpcRoot = null;
            nodeFoo.Dispose();
            nodeFoo = null;
            trayIcon.Dispose();
            trayIcon = null;
            StartUI();
        }

        private void HandleMainWindowHideEvent()
        {
            window.Hide();
        }
        public void HandleMainWindowShowEvent()
        {
            window.Show();
            window.Activate();
        }
        private void HandleMainWindowToggleEvent()
        {
            if (window.IsVisible)
            {
                HandleMainWindowHideEvent();
            }
            else
            {
                HandleMainWindowShowEvent();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (trayIcon != null)
            {
                trayIcon.Dispose();
                trayIcon = null;
            }
            logWriter.WriteLine("LiveReload says bye.");
            logWriter.Flush();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Why do we handle both CurrentDomain_UnhandledException and Application_DispatcherUnhandledException?
            //
            // We could rely on CurrentDomain_UnhandledException to process all unhandled exceptions, but:
            //
            // 1) looks like Application_DispatcherUnhandledException can continue running the app, which
            //    we will probably want to do in the future; after all, we have no in-process state to corrupt --
            //    all valuable state is only stored on Node.js side, so it's 100% safe to continue
            //    (well actually I have no idea whether CurrentDomain_UnhandledException is recoverable or not)
            //
            // 2) the log file can say "in UI thread" vs "in non-UI thread" (there is probably a better
            //    way to do that, which I don't want to think about right now)

            ReportUnhandledException(e.Exception, "UI thread");

            // Mark as handled to prevent CurrentDomain_UnhandledException from occurring.
            // (Would also cause the app to continue running, if ReportUnhandledException didn't call Shutdown.)
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // No idea why the fuck e.ExceptionObject is declared as 'object'; MSDN docs seem to imply the cast is safe.
            Exception exception = (Exception) e.ExceptionObject;
            App.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                ReportUnhandledException(exception, "non-UI thread");
            }));
        }

        private void ReportUnhandledException(Exception exception, string threadName)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine("************************************************************************");
            builder.AppendLine("Unhandled Exception in " + threadName + ": " + CreateStackTrace(exception));
            builder.AppendLine("************************************************************************");
            builder.AppendLine();

            logWriter.Write(builder.ToString());
            logWriter.Flush();

            EmergencyShutdown("LiveReload has experienced an unknown error.");
        }

        private String CreateStackTrace(Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(exception.GetType().ToString());
            builder.Append(": ");
            builder.Append(string.IsNullOrEmpty(exception.Message) ? "No reason" : exception.Message);
            builder.AppendLine();
            builder.Append(string.IsNullOrEmpty(exception.StackTrace) ? "  at unknown location" : exception.StackTrace);

            Exception inner = exception.InnerException;
            if ((inner != null) && (!string.IsNullOrEmpty(inner.StackTrace)))
            {
                builder.AppendLine();
                builder.AppendLine("Inner Exception");
                builder.Append(inner.StackTrace);
            }

            return builder.ToString().Trim();
        }
    }

    public class ProjectData
    {
        public string id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public bool compilationEnabled { get; set; }
        public string url { get; set; }
        public string snippet { get; set; }

        public ProjectData(Dictionary<string,object> dic)
        {
            id      = (string) dic["id"];
            name    = (string) dic["name"];
            path    = (string) dic["path"];
            compilationEnabled = (bool) dic["compilationEnabled"];
            url     = (string) dic["url"];
            snippet = (string) dic["snippet"];
        }
    }
}
