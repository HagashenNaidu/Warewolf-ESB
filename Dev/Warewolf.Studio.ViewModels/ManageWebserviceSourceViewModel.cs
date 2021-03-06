﻿
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable VirtualMemberCallInContructor
// ReSharper disable ValueParameterNotUsed
// ReSharper disable UnusedMember.Global

namespace Warewolf.Studio.ViewModels
{
    public class ManageWebserviceSourceViewModel : SourceBaseImpl<IWebServiceSource>, IManageWebserviceSourceViewModel
    {
        public IAsyncWorker AsyncWorker { get; set; }
        public IExternalProcessExecutor Executor { get; set; }
        private AuthenticationType _authenticationType;
        private string _hostName;
        private string _userName;
        private string _password;
        private string _defaultQuery;
        private string _testMessage;
        private string _testDefault;
        readonly IManageWebServiceSourceModel _updateManager;
        IWebServiceSource _webServiceSource;
        bool _testPassed;
        bool _testFailed;
        bool _testing;
        bool _isHyperLinkEnabled;
        string _resourceName;
        CancellationTokenSource _token;
        readonly string _warewolfserverName;
        string _headerText;
        private bool _isDisposed;
        Task<IRequestServiceNameViewModel> _requestServiceNameViewModel;
        public ManageWebserviceSourceViewModel(IManageWebServiceSourceModel updateManager, IEventAggregator aggregator,IAsyncWorker asyncWorker,IExternalProcessExecutor executor)
            : base(ResourceType.WebSource)
        {
            VerifyArgument.IsNotNull("executor", executor);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("updateManager", updateManager);
            VerifyArgument.IsNotNull("aggregator", aggregator);
            AsyncWorker = asyncWorker;
            Executor = executor;
            _updateManager = updateManager;
            _warewolfserverName = updateManager.ServerName;
            _authenticationType = AuthenticationType.Anonymous;
            _hostName = String.Empty;
            _defaultQuery = String.Empty;
            _userName = String.Empty;
            _password = String.Empty;
            HeaderText = Resources.Languages.Core.WebserviceNewHeaderLabel;
            Header = Resources.Languages.Core.WebserviceNewHeaderLabel;
            TestCommand = new DelegateCommand(TestConnection, CanTest);
            OkCommand = new DelegateCommand(SaveConnection, CanSave);
            CancelTestCommand = new DelegateCommand(CancelTest, CanCancelTest);
            ViewInBrowserCommand = new DelegateCommand(ViewInBrowser, CanViewInBrowser);

        }

        static bool CanViewInBrowser()
        {
            return true;
        }

        void ViewInBrowser()
        {
            try
            {
                Executor.OpenInBrowser(new Uri(TestDefault));
            }
            catch(Exception exception)
            {
                TestFailed = true;
                TestPassed = false;
                Testing = false;
                TestMessage = exception.Message;
            }
        }

        public ManageWebserviceSourceViewModel(IManageWebServiceSourceModel updateManager, Task<IRequestServiceNameViewModel> requestServiceNameViewModel, IEventAggregator aggregator, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : this(updateManager, aggregator, asyncWorker,executor)
        {
            VerifyArgument.IsNotNull("requestServiceNameViewModel", requestServiceNameViewModel);
            _requestServiceNameViewModel = requestServiceNameViewModel;

        }
        public ManageWebserviceSourceViewModel(IManageWebServiceSourceModel updateManager, IEventAggregator aggregator, IWebServiceSource webServiceSource, IAsyncWorker asyncWorker, IExternalProcessExecutor executor)
            : this(updateManager, aggregator, asyncWorker, executor)
        {
            VerifyArgument.IsNotNull("webServiceSource", webServiceSource);
            _webServiceSource = webServiceSource;
            _warewolfserverName = updateManager.ServerName;
            SetupHeaderTextFromExisting();
            FromModel(webServiceSource);
        }

        public ManageWebserviceSourceViewModel() : base(ResourceType.WebSource)
        {
          
        }

        void SetupHeaderTextFromExisting()
        {
            var serverName = _warewolfserverName;
            if(serverName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                HeaderText = (_webServiceSource == null ? ResourceName : _webServiceSource.Name).Trim();
                Header = (_webServiceSource == null ? ResourceName : _webServiceSource.Name).Trim();
            }
            else
            {
                HeaderText = (_webServiceSource == null ? ResourceName : _webServiceSource.Name).Trim();
                Header = (_webServiceSource == null ? ResourceName : _webServiceSource.Name).Trim();
            }
        }

        public override bool CanSave()
        {
            return TestPassed || CanTest();
        }

        bool CanCancelTest()
        {
            return Testing;
        }

        void CancelTest()
        {
            if (_token != null)
            {
                if (!_token.IsCancellationRequested && _token.Token.CanBeCanceled)
                {
                    _token.Cancel();
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        Testing = false;
                        TestFailed = true;
                        TestPassed = false;
                        TestMessage = "Test Cancelled";
                    });
                }
            }
        }

        public bool CanTest()
        {
            if (Testing)
                return false;
            if (String.IsNullOrEmpty(HostName))
            {
                return false;
            }
            if (AuthenticationType == AuthenticationType.User)
            {
                return !String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(Password);
            }
            return true;
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        public override void FromModel(IWebServiceSource webServiceSource)
        {
            ResourceName = webServiceSource.Name;
            AuthenticationType = webServiceSource.AuthenticationType;
            UserName = webServiceSource.UserName;
            DefaultQuery = webServiceSource.DefaultQuery;
            HostName = webServiceSource.HostName;
            Password = webServiceSource.Password;
        }

        public override string Name
        {
            get
            {
                return ResourceName;
            }
            set
            {
                ResourceName = ResourceName;
            }
        }

        public string ResourceName
        {
            get
            {
                return _resourceName;
            }
            set
            {
                _resourceName = value;
                OnPropertyChanged(_resourceName);
            }
        }

        public bool UserAuthenticationSelected
        {
            get { return AuthenticationType == AuthenticationType.User; }
        }

        void SaveConnection()
        {
            if (_webServiceSource == null)
            {
                var res = RequestServiceNameViewModel.ShowSaveDialog();

                if (res == MessageBoxResult.OK)
                {
                    ResourceName = RequestServiceNameViewModel.ResourceName.Name;
                    var src = ToSource();
                    src.Path = RequestServiceNameViewModel.ResourceName.Path ?? RequestServiceNameViewModel.ResourceName.Name;
                    Save(src);
                    Item = src;
                    _webServiceSource = src;
                    SetupHeaderTextFromExisting();
                }
            }
            else
            {
                var src = ToSource();
                Save(src);
                Item = src;
                _webServiceSource = src;
                SetupHeaderTextFromExisting();
            }
        }

        void Save(IWebServiceSource source)
        {
            _updateManager.Save(source);

        }
        public override void Save()
        {
            SaveConnection();

        }

        void TestConnection()
        {
            _token = new CancellationTokenSource();
            AsyncWorker.Start(SetupProgressSpinner, () =>
            {
                TestMessage = "Passed";
                TestFailed = false;
                TestPassed = true;
                Testing = false;
            },
            _token, exception =>
            {
                TestFailed = true;
                TestPassed = false;
                Testing = false;
                TestMessage = exception != null ? exception.Message : "Failed";
            });


        }

        void SetupProgressSpinner()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Testing = true;
                TestFailed = false;
                TestPassed = false;
            });
            _updateManager.TestConnection(ToNewSource());
        }

        IWebServiceSource ToNewSource()
        {

            return new WebServiceSourceDefinition
            {
                AuthenticationType = AuthenticationType,
                HostName = HostName,
                Password = Password,
                UserName = UserName,
                Name = ResourceName,
                DefaultQuery = DefaultQuery,
                Id = _webServiceSource == null ? Guid.NewGuid() : _webServiceSource.Id
            };
        }

        IWebServiceSource ToSource()
        {
            if (_webServiceSource == null)
                return new WebServiceSourceDefinition
                {
                    AuthenticationType = AuthenticationType,
                    HostName = HostName,
                    Password = Password,
                    UserName = UserName,
                    DefaultQuery = DefaultQuery,
                    Name = ResourceName,
                    Id = _webServiceSource == null ?  SelectedGuid : _webServiceSource.Id
                }
            ;
            // ReSharper disable once RedundantIfElseBlock
            else
            {
                _webServiceSource.AuthenticationType = AuthenticationType;
                _webServiceSource.DefaultQuery = DefaultQuery;
                _webServiceSource.Password = Password;
                _webServiceSource.HostName = HostName;
                _webServiceSource.UserName = UserName;
                return _webServiceSource;

            }
        }

        public override IWebServiceSource ToModel()
        {
            if (Item == null)
            {
                Item = ToSource();
                return Item;
            }

            return new WebServiceSourceDefinition
            {
                Name = Item.Name,
                HostName = HostName,
                AuthenticationType = AuthenticationType,
                DefaultQuery = DefaultQuery,
                Password = Password,
                UserName = UserName,
                Id = Item.Id,
                Path = Item.Path
            };

        }

        public IRequestServiceNameViewModel RequestServiceNameViewModel
        {
            get
            {
                if(_requestServiceNameViewModel != null)
                {
                    _requestServiceNameViewModel.Wait();
                    if (_requestServiceNameViewModel.Exception == null)
                    {
                        return _requestServiceNameViewModel.Result;
                    }
                    // ReSharper disable once RedundantIfElseBlock
                    else
                    {
                        throw _requestServiceNameViewModel.Exception;
                    }
                }
                return null;
            }
            set { _requestServiceNameViewModel = new Task<IRequestServiceNameViewModel>(() => value); _requestServiceNameViewModel.Start(); }
        }

        public AuthenticationType AuthenticationType
        {
            get { return _authenticationType; }
            set
            {
                if (_authenticationType != value)
                {
                    _authenticationType = value;
                    OnPropertyChanged(() => AuthenticationType);
                    OnPropertyChanged(() => Header);
                    OnPropertyChanged(() => UserAuthenticationSelected);
                    ResetTestValue();
                }
            }
        }

        public string HostName
        {
            get { return _hostName; }
            set
            {
                if (value != _hostName)
                {
                    _hostName = value;
                    TestDefault = _hostName + "" + DefaultQuery;
                    OnPropertyChanged(() => HostName);
                    ResetTestValue();
                }
            }
        }

        public string DefaultQuery
        {
            get { return _defaultQuery; }
            set
            {
                if (value != _defaultQuery)
                {
                    _defaultQuery = value;
                    TestDefault = _hostName + "" + DefaultQuery;
                    OnPropertyChanged(() => DefaultQuery);
                    ResetTestValue();
                }
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    OnPropertyChanged(() => UserName);
                    ResetTestValue();
                }
            }
        }

        void ResetTestValue()
        {
            TestMessage = "";
            TestFailed = false;
            Testing = false;
            OnPropertyChanged(() => Header);
            ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value != _userName)
                {
                    _password = value;
                    OnPropertyChanged(() => Password);
                    ResetTestValue();
                }
            }
        }

        public ICommand CancelTestCommand { get; set; }

        public ICommand TestCommand { get; set; }

        public ICommand ViewInBrowserCommand { get; set; }

        public string TestDefault
        {
            get { return _testDefault; }
            set
            {
                _testDefault = value;
                OnPropertyChanged(() => TestDefault);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
            }
        }

        public string TestMessage
        {
            get { return _testMessage; }
            // ReSharper disable UnusedMember.Local
            private set
            // ReSharper restore UnusedMember.Local
            {
                _testMessage = value;
                OnPropertyChanged(() => TestMessage);
                OnPropertyChanged(() => TestPassed);
            }
        }

        public bool TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                if (_testPassed)
                {
                    IsHyperLinkEnabled = true;
                }
                OnPropertyChanged(() => TestPassed);
                ViewModelUtils.RaiseCanExecuteChanged(OkCommand);
                ViewModelUtils.RaiseCanExecuteChanged(ViewInBrowserCommand);
            }
        }

        public bool IsHyperLinkEnabled
        {
            get { return _isHyperLinkEnabled; }
            set
            {
                _isHyperLinkEnabled = value;
                OnPropertyChanged(() => IsHyperLinkEnabled);
            }
        }

        public bool TestFailed
        {
            get
            {
                return _testFailed;
            }
            set
            {
                _testFailed = value;
                OnPropertyChanged(() => TestFailed);
            }
        }

        public bool Testing
        {
            get
            {
                return _testing;
            }
            private set
            {
                _testing = value;

                OnPropertyChanged(() => Testing);
                ViewModelUtils.RaiseCanExecuteChanged(ViewInBrowserCommand);
                ViewModelUtils.RaiseCanExecuteChanged(TestCommand);
                ViewModelUtils.RaiseCanExecuteChanged(CancelTestCommand);
            }
        }

        public ICommand OkCommand { get; set; }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                OnPropertyChanged(() => HeaderText);
                OnPropertyChanged(() => Header);
            }
        }

        public bool IsEmpty { get { return String.IsNullOrEmpty(HostName) && AuthenticationType == AuthenticationType.Anonymous && String.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password); } }

        protected override void OnDispose()
        {
            if (RequestServiceNameViewModel != null)
            {
                if (RequestServiceNameViewModel != null) RequestServiceNameViewModel.Dispose();
   
            }
            Dispose(true);
        }
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_token != null) _token.Dispose();
                }

                // Dispose unmanaged resources.
                _isDisposed = true;
            }
        }
    }
}
