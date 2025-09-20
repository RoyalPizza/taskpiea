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
                // TODO: just revert the changes instead of doing an entire refresh
                AppDataCache.shared.Refresh();
            }
        }

        private async void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;
            if (DataContext == null || e.Row.Item == null)
                return;
            if (DataContext is not TaskListViewModel viewModel || e.Row.Item is not TaskItem taskItem)
                return;
            
            if (taskItem.Id == 0)
                await viewModel.CreateAsync(taskItem);
            else
                await viewModel.UpdateAsync(taskItem);
        }
    }
}
