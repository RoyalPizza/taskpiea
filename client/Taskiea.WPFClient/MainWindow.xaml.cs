using System.Windows;
using System.Windows.Input;

namespace Taskiea.WPFClient
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

        // These functions are so we can support our own "window handle"
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void CreateProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CreateProjectWindow createProjectWindow = new CreateProjectWindow();
            bool? result = createProjectWindow.ShowDialog();

            if (result == true)
            {
                // TODO
            }
        }

        private void OpenProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open file dialogue for *.taskp files
        }

        private void CloseProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}