using System.Windows;
using System.Windows.Input;

namespace Taskiea.WPFClient
{
    /// <summary>
    /// Interaction logic for CreateProjectWindow.xaml
    /// </summary>
    public partial class CreateProjectWindow : Window
    {
        // TODO: Bind to UI
        private string _projectName { get; set; } = "";

        public CreateProjectWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            //string storageConnectionString = StorageDataLayerUtil.BuildSqliteConnectionString(_projectName);
            //Project _project = new Project(_projectName, storageConnectionString);
        }
    }
}
