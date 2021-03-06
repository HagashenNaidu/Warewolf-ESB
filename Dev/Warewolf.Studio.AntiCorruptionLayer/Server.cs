using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Controller;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public class Server : Resource,IServer,INotifyPropertyChanged
    {
        readonly Guid _serverId;
        readonly StudioServerProxy _proxyLayer;
        IList<IToolDescriptor> _tools;
        readonly IEnvironmentModel _environmentModel;
        bool _hasloaded;
        string _version;
        string _informationalVersion;
        private string _minversion;
        private List<IWindowsGroupPermission> _permissions;

        public Server(IEnvironmentModel environmentModel)
        {
            EnvironmentConnection = environmentModel.Connection;
            EnvironmentID = environmentModel.ID;
            _serverId = EnvironmentConnection.ServerID;
            var communicationControllerFactory = new CommunicationControllerFactory();
            _proxyLayer = new StudioServerProxy(communicationControllerFactory, EnvironmentConnection);
            UpdateRepository = new StudioResourceUpdateManager(communicationControllerFactory, EnvironmentConnection);
            EnvironmentConnection.PermissionsModified += RaisePermissionsModifiedEvent;
            ResourceName = EnvironmentConnection.DisplayName;
            EnvironmentConnection.NetworkStateChanged+=RaiseNetworkStateChangeEvent;
            EnvironmentConnection.ItemAddedMessageAction+=ItemAdded;
            environmentModel.WorkflowSaved += (sender, args) => UpdateRepository.FireItemSaved();
            _environmentModel = environmentModel;            
        }

        public bool CanDeployTo
        {
            get
            {
                return _environmentModel.IsAuthorizedDeployTo;
            }
        }

        public bool CanDeployFrom
        {
            get
            {
                return _environmentModel.IsAuthorizedDeployFrom;
            }
        }

        public Server()
        {
        }

        public Guid EnvironmentID { get; set; }
        public Guid? ServerID
        {
            get
            {
                return EnvironmentConnection.ServerID;
        }
        }

        public bool HasLoaded
        {
            get
            {
                return _hasloaded;
            }
            private set
            {

                _hasloaded = value;
                OnPropertyChanged("IsConnected");
                OnPropertyChanged("DisplayName");
            }
        }

        void ItemAdded(IExplorerItem obj)
        {
            if (ItemAddedEvent != null)
            {
                ItemAddedEvent(obj);
            }
        }

        public string GetServerVersion()
        {
            if (_version == null)
            {
                if (!EnvironmentConnection.IsConnected)
                {
                    EnvironmentConnection.Connect(Guid.Empty);
                }
                _version = ProxyLayer.AdminManagerProxy.GetServerVersion();
            }

            return _version;
        }

        public string GetServerInformationalVersion()
        {
            if (_informationalVersion == null)
            {
                if (!EnvironmentConnection.IsConnected)
                {
                    EnvironmentConnection.Connect(Guid.Empty);
                }
                _informationalVersion = ProxyLayer.AdminManagerProxy.GetServerInformationalVersion();
            }

            return _informationalVersion;
        }

        public string GetMinSupportedVersion()
        {
            if (_minversion == null)
            {
                if (!EnvironmentConnection.IsConnected)
                {
                    EnvironmentConnection.Connect(Guid.Empty);
                }
                _minversion = ProxyLayer.AdminManagerProxy.GetMinSupportedServerVersion();
            }

            return _minversion;

        }

        void RaiseNetworkStateChangeEvent(object sender, System.Network.NetworkStateEventArgs e)
        {
            if(NetworkStateChanged!= null)
            {
                NetworkStateChanged(new NetworkStateChangedEventArgs(e),this);
            }
        }

        void RaisePermissionsModifiedEvent(object sender, List<WindowsGroupPermission> windowsGroupPermissions)
        {
            if (PermissionsChanged != null)
            {
                PermissionsChanged(new PermissionsChangedArgs(windowsGroupPermissions.Cast<IWindowsGroupPermission>().ToList()));
            }
            Permissions = windowsGroupPermissions.Select(permission => permission as IWindowsGroupPermission).ToList();
        }

        #region Implementation of IServer

        public void Connect()
        {
            if(!EnvironmentConnection.IsConnected)
            {
                EnvironmentConnection.Connect(_serverId);
                OnPropertyChanged("IsConnected");
                OnPropertyChanged("DisplayName");
            }
        }


        public async Task<bool> ConnectAsync()
        {
            
            var connected = await EnvironmentConnection.ConnectAsync(_serverId);
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("DisplayName");
            return connected;
        }

        public string DisplayName
        {
            get
            {
                var displayName = Resources.Languages.Core.NewServerLabel;
                if (EnvironmentConnection != null)
                {
                    displayName = EnvironmentConnection.DisplayName;
                    if (IsConnected && (HasLoaded || EnvironmentConnection.IsLocalHost))
                    {
                        if (!displayName.Contains(Resources.Languages.Core.ConnectedLabel))
                        {
                            displayName += Resources.Languages.Core.ConnectedLabel;
                        }
                    }
                }
                
                return displayName;
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                EnvironmentConnection.DisplayName = DisplayName;
                OnPropertyChanged();
            }
        }


        public IServer Clone()
        {
            return new Server(_environmentModel)
            {
                Permissions = Permissions
            };
        }

        public List<IResource> Load()
        {
            return null;
        }

        public async Task<IExplorerItem> LoadExplorer()
        {
            var result = await ProxyLayer.LoadExplorer();
            HasLoaded = true;
            return result;
        }

        public IList<IServer> GetServerConnections()
        {
            var environmentModels = EnvironmentRepository.Instance.ReloadServers();
            return environmentModels.Select(environmentModel => new Server(environmentModel)).Cast<IServer>().ToList();
        }

        public IList<IToolDescriptor> LoadTools()
        {
            return _tools ?? (_tools = ProxyLayer.QueryManagerProxy.FetchTools());
        }

        public IExplorerRepository ExplorerRepository
        {
            get
            {
                return ProxyLayer;
            }
        }

        public IQueryManager QueryProxy
        {
            get
            {
                return _proxyLayer.QueryManagerProxy;
            }
        }
        
        public bool IsConnected
        {
            get
            {
                return EnvironmentConnection != null && EnvironmentConnection.IsConnected;
            }
        }

        public bool AllowEdit
        {
            get { return EnvironmentConnection!=null&& !EnvironmentConnection.IsLocalHost; }
        }

        public void ReloadTools()
        {
        }

        public void Disconnect()
        {
            EnvironmentConnection.Disconnect();
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("DisplayName");
        }

        public void Edit()
        {
        }

        public List<IWindowsGroupPermission> Permissions
        {
            get
            {
                if(_permissions == null)
                {
                    try
                    {
                        if (IsConnected)
                        {
                    _permissions = ProxyLayer.QueryManagerProxy.FetchPermissions();
                    if (PermissionsChanged != null)
                    {
                        PermissionsChanged(new PermissionsChangedArgs(_permissions.ToList()));
                    }
                        }
                }
                    catch(Exception)
                    {
                        
//ignore
                    }

                }
                return _permissions;
            }
            set
            {
                _permissions = value;
            }
        }

        public event PermissionsChanged PermissionsChanged;
        public event NetworkStateChanged NetworkStateChanged;
        public event ItemAddedEvent ItemAddedEvent;

        public IStudioUpdateManager UpdateRepository { get; private set; }
        
        public StudioServerProxy ProxyLayer
        {
            get
            {
                return _proxyLayer;
            }
        }
        public IEnvironmentConnection EnvironmentConnection { get; set; }

        #endregion

        #region Overrides of Resource

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
           return DisplayName;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}