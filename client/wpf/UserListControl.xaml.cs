using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using Taskpiea.Core.Accounts;

namespace Taskpiea.WPFClient
{
    /// <summary>
    /// Interaction logic for UserListControl.xaml
    /// </summary>
    public partial class UserListControl : UserControl
    {
        private readonly IUserRepository _userRepository;

        public UserListControl()
        {
            InitializeComponent();

            // We bind a command for delete because there is no event. 
            // We are not binding a command for Create/Update because it was already programmed to use the events.
            UsersDataGrid.CommandBindings.Add(new CommandBinding(DataGrid.DeleteCommand, OnDeleteExecutedAsync));
            //UsersDataGrid.CommandBindings.Add(new CommandBinding(DataGrid.CommitEditCommand, OnCommitEditExecutedAsync));
        }

        private void DataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            //e.NewItem = new User
            //{
            //    Name = ""
            //};
        }

        private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (e.NewItem is not User user)
                return;

            user.Name = "";
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
            if (DataContext is not UserListViewModel viewModel || e.Row.Item is not User user)
                return;

            var result = (user.Id == 0) ? await _userRepository.CreateAsync(AppDataCache.ProjectName, user) 
                : await _userRepository.UpdateAsync(AppDataCache.ProjectName, user);

            if (result.ResultCode == Core.Results.ResultCode.Success)
            {

            }
            else
            {
                foreach (var validationError in result.ValidationErrors)
                {
                    var error = new ValidationError(new DataErrorValidationRule(), e.Row.BindingGroup)
                    {
                        ErrorContent = result.ErrorMessage
                    };
                    //Validation.MarkInvalid(e.Row.BindingGroup, error);
                }
                
                e.Cancel = true; // Prevent commit if validation fails
            }


            //if (user.Id == 0)
            //{
            //    await viewModel.CreateAsync(user); // TODO: GET RESULT AND SHOW ERROR HERE IF FAILED?
            //}
            //else
            //{
            //    await viewModel.UpdateAsync(user);
            //}

            
        }

        private async void OnDeleteExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            if (DataContext == null || DataContext is not UserListViewModel viewModel)
                return;

            // Note: For some reason e.Parameter is null here.
            // Either way, the param is usually a struct that says "Cell or Row".

            // cast to list of objects so it makes a copy, and handles users selecting to delete the "empty row"
            var selectedItems = UsersDataGrid.SelectedItems.Cast<object>().ToList();
            foreach (var item in selectedItems)
            {
                if (item is null || item is not User user)
                    continue;

                await viewModel.DeleteAsync(user);
            }
        }

        private async void OnCommitEditExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            throw new NotImplementedException();

            // The commands dont pass the values in e.Parameter, you need to use CurrentItem or SelectedObjects.
            // Either way, I dont find this API to be very well done, so I am just sticking with whatever method works. 

            if (e.Parameter is not DataGridEditingUnit editUnit)
                return;

            if (editUnit == DataGridEditingUnit.Cell)
            {
                Debug.WriteLine("Editing Cell");
                var cell = UsersDataGrid.CurrentCell;
                var user = UsersDataGrid.CurrentItem;
            }
            else if (editUnit == DataGridEditingUnit.Row)
            {
                Debug.WriteLine("Editing Row");
                var user = UsersDataGrid.CurrentItem;
            }
        }
    }
}
