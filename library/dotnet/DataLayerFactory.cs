using Taskiea.Core.Accounts;

namespace Taskiea.Core;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// This is a semi factory. It doubles as a datalayer creator 
/// and a datalayer manager.
/// </remarks>
public class DataLayerFactory
{
    private List<IDataLayer> _dataLayers = new List<IDataLayer>();
    
    public void SetupStorageDataLayers(ProjectConnectionData projectConnectionData)
    {
        // This is not a good solution for servers. Because now servers need to reload factories everytime a project changes. This wont work.

        _dataLayers.Add(new UserStorageDataLayer());
        _dataLayers.Add(new TaskStorageDataLayer());

        foreach (var layer in _dataLayers)
            layer.Initialize(projectConnectionData);
    }
}
