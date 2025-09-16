using System.ComponentModel;

namespace Taskpiea.WPFClient;

internal class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _currentProjectName = "";
    public string CurrentProjectName
    {
        get => _currentProjectName;
        set
        {
            _currentProjectName = value;
            OnPropertyChanged(nameof(CurrentProjectName));
        }
    }

    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public MainWindowViewModel()
    {
        CurrentProjectName = AppDataCache.shared.Project?.Name ?? "";
        AppDataCache.shared.PropertyChanged += Shared_PropertyChanged;
    }

    private void Shared_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppDataCache.Project))
            CurrentProjectName = AppDataCache.shared.Project?.Name ?? "";
    }
}
