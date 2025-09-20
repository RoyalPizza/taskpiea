using System.Diagnostics;
using System.Windows.Controls;
using Taskpiea.Core.Tasks;

namespace Taskpiea.WPFClient
{
    /// <summary>
    /// Interaction logic for TaskList.xaml
    /// </summary>
    public partial class TaskListControl : UserControl
    {
        public TaskListControl()
        {
            InitializeComponent();
        }

        private void DataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            e.NewItem = new TaskItem
            {
                Name = "",
                Description = "",
                Status = Status.Open,
                Assignee = 0
            };
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // do nothing because we only want to commit when the row is finished
            }
            else if (e.EditAction == DataGridEditAction.Cancel)
            {
                AppDataCache.shared.Refresh();
            }
        }

        private async void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var viewModel = DataContext as TaskListViewModel;
                var task = e.Row.Item as TaskItem;
                if (viewModel != null && task != null)
                {
                    if (task.Id == 0)
                        await viewModel.CreateNewTaskCommand.ExecuteAsync(task);
                    else
                        await viewModel.UpdateTaskCommand.ExecuteAsync(task);
                }
            }
            else
            {
                Debug.WriteLine("Row: " + e.EditAction.ToString());
            }
        }

        
    }
}
