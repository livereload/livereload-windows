﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace LiveReload
{
    class NodeRPC
    {
        private Process process = new Process();
        private StreamWriter writer;
        private StreamReader reader;
        private StreamReader stderrReader;
        private Dispatcher dispatcher = App.Current.Dispatcher;
        private string nodeDir;
        private string backendDir;
        private StreamWriter logWriter;

        public event Action         NodeStartedEvent;
        public event Action         NodeCrash;
        public event Action<string> NodeMessageEvent;

        public NodeRPC(string nodeDir_, string backendDir_, StreamWriter logWriter_)
        {
            nodeDir = nodeDir_;
            backendDir = backendDir_;
            logWriter = logWriter_;
        }

        public void Start()
        {
            Thread nodeThread = new Thread(new ThreadStart(NodeRun));
            nodeThread.IsBackground = true; // need for thread to close at application exit
            nodeThread.Start();
        }

        private void NodeStart()
        {
            process.StartInfo.FileName = Path.Combine(nodeDir, @"LiveReloadNodejs.exe");
            process.StartInfo.Arguments = "\"" + (Path.Combine(backendDir, "bin/livereload.js") + "\" " + "rpc server");
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true; // node doesn't work without this line

            process.Start();

            writer = process.StandardInput;
            reader = process.StandardOutput;
            stderrReader = process.StandardError;

            dispatcher.Invoke(DispatcherPriority.Normal,
                (Action)(() => { NodeStartedEvent(); })
            );

            Thread stderrThread = new Thread(new ThreadStart(CopyNodeStderrToLog));
            stderrThread.IsBackground = true;
            stderrThread.Start();
        }

        private void NodeRun()
        {
            NodeStart();
            while (!reader.EndOfStream)
            {
                string nodeLine = reader.ReadLine();
                logWriter.WriteLine("INCOMING: " + nodeLine);
                logWriter.Flush();
                if (nodeLine[0] == '[')
                {
                    dispatcher.Invoke(DispatcherPriority.Normal,
                        (Action)(() => { NodeMessageEvent(nodeLine); })
                    );
                }
            }
            dispatcher.Invoke(DispatcherPriority.Normal,
                (Action)(() => { NodeCrash(); })
            );
        }

        private void CopyNodeStderrToLog()
        {
            while (!stderrReader.EndOfStream)
            {
                string nodeLine = stderrReader.ReadLine();
                logWriter.WriteLine("STDERR: " + nodeLine);
                logWriter.Flush();
            }
        }

        public void NodeMessageSend(string message)
        {
            logWriter.WriteLine("OUTGOING: " + message);
            logWriter.Flush();

            Console.WriteLine("OUTGOING: " + message);

            writer.WriteLine(message);
            writer.Flush();
        }
    }
}
