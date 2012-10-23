using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;

namespace LiveReload
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        public event Action MainWindowHideEvent;

        private List<ProjectData> projectsList;

        private string selectedID = null;
        public bool isUrlFieldChangedByUser          = false;
        public bool isUrlFieldChangedProgramatically = false;
        private bool isTreeViewUpdateInProgress      = false;

        public event Action<string> ProjectAddEvent;
        public event Action<string> ProjectRemoveEvent;
        public event Action<string, string, object> ProjectPropertyChangedEvent;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Current.Shutdown();
        }
        public void updateTreeView(List<ProjectData> projectsList_)
        {
            projectsList = projectsList_;

            isTreeViewUpdateInProgress = true;
            treeViewProjects.Items.Clear(); // will react on it later if necessary
            TreeViewItem oldSelection = null;
            foreach (ProjectData t in projectsList)
            {
                TreeViewItem newChild = new TreeViewItem();
                newChild.Header = t.name;
                //newChild.Name   = t.id; // crashes whem there are some symbols in id, like '!'
                treeViewProjects.Items.Add(newChild);
                if (t.id == selectedID)
                {
                    oldSelection = newChild;
                }
            }
            isTreeViewUpdateInProgress = false;
            if (oldSelection != null)
            {
                SelectItem(oldSelection);
            }
            else
            {
                treeViewProjects_SelectedItemChanged(null, null); // need to reset view
            }
        }

        private void treeViewProjects_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (isTreeViewUpdateInProgress)
                return;

            ProjectData project = SelectedProject();
            if (project != null)
            {
                textBlockProjectName.Text = project.name;
                textBlockProjectPath.Text = project.path;
                textBoxSnippet.Text = project.snippet;
                textBoxSnippet.IsEnabled = true;
                textBoxUrl.IsReadOnly = false;
                textBoxUrl.IsEnabled = true;
                if (!isUrlFieldChangedByUser)
                {
                    isUrlFieldChangedProgramatically = true;
                    textBoxUrl.Text = project.url;
                    isUrlFieldChangedProgramatically = false;
                }
                checkBoxCompile.IsEnabled = true;
                checkBoxRunCustom.IsEnabled = true;
                checkBoxCompile.IsChecked = project.compilationEnabled;
                selectedID = project.id;
            }
            else
            {
                textBlockProjectName.Text = "-";
                textBlockProjectPath.Text = "-";
                textBoxSnippet.Text      = null;
                textBoxSnippet.IsEnabled = false;
                textBoxUrl.IsReadOnly = true;
                textBoxUrl.IsEnabled = false;
                if (!isUrlFieldChangedByUser)
                {
                    isUrlFieldChangedProgramatically = true;
                    textBoxUrl.Text = null;
                    isUrlFieldChangedProgramatically = false;
                }
                checkBoxCompile.IsEnabled = false;
                checkBoxRunCustom.IsEnabled = false;
                checkBoxCompile.IsChecked = false;
                checkBoxRunCustom.IsChecked = false;
            }
        }

        private void buttonProjectAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ProjectAddEvent(dialog.SelectedPath);
            }
        }

        private ProjectData SelectedProject()
        {
            var item = treeViewProjects.SelectedItem;
            return (item == null) ? null : projectsList[treeViewProjects.Items.IndexOf(item)];
        }

        private void buttonProjectRemove_Click(object sender, RoutedEventArgs e)
        {
            ProjectData project = SelectedProject();
            if (project != null)
            {
                ProjectRemoveEvent(project.id);
            }
        }

        private void checkBoxCompile_Click(object sender, RoutedEventArgs e)
        {
            ProjectData project = SelectedProject();
            if (checkBoxCompile.IsChecked == true)
            {
                ProjectPropertyChangedEvent(project.id,"compilationEnabled",true);
            }
            else // ThreeWayState is disabled for this checkbox!
            {
                ProjectPropertyChangedEvent(project.id,"compilationEnabled",false);
            }
        }

        private void SelectItemHelper(TreeViewItem node) // unneeded ATM, retest when we will have tree depth > 1
        {
            if (node == null)
                return;
            SelectItemHelper((TreeViewItem)node.Parent);
            if (!node.IsExpanded)
            {
                node.IsExpanded = true;
                node.UpdateLayout();
            }
        }
        private void SelectItem(TreeViewItem node) // QND solution
        {
            SelectItemHelper(node.Parent as TreeViewItem);
            node.IsSelected = true;
        }

        private void buttonVersion_Click(object sender, RoutedEventArgs e)
        {
            ((App)App.Current).OpenExplorerWithLog();
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Normal;
            MainWindowHideEvent();
        }

        private void textBoxUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUrlFieldChangedProgramatically)
                return;

            isUrlFieldChangedByUser = true;
            ProjectPropertyChangedEvent(selectedID, "url", textBoxUrl.Text);
        }
        private void textBoxUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            isUrlFieldChangedByUser = false;
            ProjectPropertyChangedEvent(selectedID, "url", textBoxUrl.Text);
        }
        private void buttonSupport_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"http://feedback.livereload.com/");
        }
    }
}
