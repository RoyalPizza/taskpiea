using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Taskpiea.Core.Connections;

namespace Taskpiea.WPFClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new HomeScreenControl();
        }

        // These functions are so we can support our own "window handle"
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void NewProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // create a new project with defaults
            var newProjectName = AppDataCache.shared.ProjectProber.GetNewDefaultProjectName();
            var newProjectDirectory = AppDataCache.shared.ProjectProber.GetDefaultProjectDirectory();
            SqliteConnectionData connectionData = new SqliteConnectionData(newProjectName, newProjectDirectory);
            AppDataCache.shared.OpenProject(connectionData);
            MainContentControl.Content = new TaskListControl();
        }

        private void OpenProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (AppDataCache.shared.Project != null)
                AppDataCache.shared.CloseProject();

            // TODO: Right now this is hardcoded to be local projects only, but we need to support either local or client/server

            string extensionName = AppDataCache.shared.ProjectProber.GetProjectFileExtension();

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = $"TaskPia Projects (*{extensionName})|*{extensionName}",
                InitialDirectory = AppDataCache.shared.ProjectProber.GetDefaultProjectDirectory(),
                Title = "Open Project"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string projectName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                string projectDirectory = Path.GetDirectoryName(openFileDialog.FileName);
                SqliteConnectionData connectionData = new SqliteConnectionData(projectName, projectDirectory);
                AppDataCache.shared.OpenProject(connectionData);
                MainContentControl.Content = new TaskListControl();
            }
        }

        private void CloseProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (AppDataCache.shared.Project == null)
                return;

            AppDataCache.shared.CloseProject();
            MainContentControl.Content = new HomeScreenControl();
        }
    }
}