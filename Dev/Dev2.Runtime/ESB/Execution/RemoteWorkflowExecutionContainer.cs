
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using ServiceStack.Common.Extensions;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Execute a remote workflow ;)
    /// </summary>
    public class RemoteWorkflowExecutionContainer : EsbExecutionContainer
    {
        readonly IResourceCatalog _resourceCatalog;

        /// <summary>
        /// Need to add loc property to AbstractActivity ;)
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="dataObj"></param>
        /// <param name="workspace"></param>
        /// <param name="esbChannel"></param>
        public RemoteWorkflowExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace workspace, IEsbChannel esbChannel)
            : this(sa, dataObj, workspace, esbChannel, ResourceCatalog.Instance)
        {
        }

        public RemoteWorkflowExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace workspace, IEsbChannel esbChannel, IResourceCatalog resourceCatalog)
            : base(sa, dataObj, workspace, esbChannel)
        {
            if (resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }
            _resourceCatalog = resourceCatalog;
        }

        public void PerformLogExecution(string logUri, int update)
        {
            
            var expressionsEntry = DataObject.Environment.Eval(logUri, update);
            var itr = new WarewolfIterator(expressionsEntry);
            while (itr.HasMoreData())
            {
                var val = itr.GetNextValue();
                {
                    var buildGetWebRequest = BuildSimpleGetWebRequest(val);
                    if (buildGetWebRequest == null)
                    {
                        throw new Exception("Invalid Url to execute for logging");
                    }
                    buildGetWebRequest.UseDefaultCredentials = true;
                    ExecuteWebRequestAsync(buildGetWebRequest);
                }
            }
        }

        protected virtual void ExecuteWebRequestAsync(WebRequest buildGetWebRequest)
        {
            if (buildGetWebRequest == null)
            {
                return;
            }
            buildGetWebRequest.GetResponseAsync();
        }

        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            Dev2Logger.Info(String.Format("Starting Remote Execution. Service Name:{0} Resource Id:{1} Mode:{2}", DataObject.ServiceName, DataObject.ResourceID, DataObject.IsDebug ? "Debug" : "Execute"));

            var serviceName = DataObject.ServiceName;

            errors = new ErrorResultTO();

            // get data in a format we can send ;)
            Dev2Logger.Debug("Creating DataList fragment for remote execute");
            var dataListFragment = ExecutionEnvironmentUtils.GetXmlInputFromEnvironment(DataObject, DataObject.RemoteInvokeResultShape.ToString(), update);

            string result = string.Empty;

            var connection = GetConnection(DataObject.EnvironmentID);
            if (connection == null)
            {
                errors.AddError("Server source not found.");
                return DataObject.DataListID;
            }

            try
            {
                // Invoke Remote WF Here ;)
                result = ExecutePostRequest(connection, serviceName, dataListFragment);
                IList<IDebugState> msg = DataObject.IsDebug? FetchRemoteDebugItems(connection):new List<IDebugState>();
                DataObject.RemoteDebugItems = msg; // set them so they can be acted upon
            }
            catch (Exception e)
            {
                var errorMessage = e.Message.Contains("Forbidden") ? "Executing a service requires Execute permissions" : e.Message;
                DataObject.Environment.Errors.Add(errorMessage);
                Dev2Logger.Error(e);
            }

            // Create tmpDL
            ExecutionEnvironmentUtils.UpdateEnvironmentFromOutputPayload(DataObject,result.ToStringBuilder(),DataObject.RemoteInvokeResultShape.ToString(), update);
            Dev2Logger.Info(String.Format("Completed Remote Execution. Service Name:{0} Resource Id:{1} Mode:{2}", DataObject.ServiceName, DataObject.ResourceID, DataObject.IsDebug ? "Debug" : "Execute"));

            return Guid.Empty;
        }

        private string ExecutePostRequest(Connection connection, string serviceName, string payload, bool isDebugMode = true)
        {
            var result = string.Empty;

            var serviceToExecute = connection.WebAddress + "Services/" + serviceName;
            var req = BuildPostRequest(serviceToExecute, payload, connection.AuthenticationType, connection.UserName, connection.Password, isDebugMode);
            Dev2Logger.Debug("Executing the remote request.");
            if (req != null)
            {
                using (var response = req.GetResponse() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        // ReSharper disable AssignNullToNotNullAttribute
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        // ReSharper restore AssignNullToNotNullAttribute
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }

            return result;
        }

        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity)
        {
            return null;
        }

        protected virtual IList<IDebugState> FetchRemoteDebugItems(Connection connection)
        {
            var data = ExecutePostRequest(connection, "FetchRemoteDebugMessagesService", "InvokerID=" + DataObject.RemoteInvokerID);

            if (data != null)
            {
                IList<IDebugState> fetchRemoteDebugItems = RemoteDebugItemParser.ParseItems(data);
                fetchRemoteDebugItems.ForEach(state => state.SessionID = DataObject.DebugSessionID);
                return fetchRemoteDebugItems;
            }

            return null;
        }

        public bool ServerIsUp()
        {
            var connection = GetConnection(DataObject.EnvironmentID);
            if (connection == null)
            {
                return false;
            }
            try
            {
                var returnData = ExecuteGetRequest(connection, "ping", "<DataList></DataList>",false);
                if (!string.IsNullOrEmpty(returnData))
                {
                    if (returnData.Contains("Pong"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        protected virtual string ExecuteGetRequest(Connection connection, string serviceName, string payload, bool isDebugMode=true)
        {
            var result = string.Empty;

            var serviceToExecute = connection.WebAddress + "Services/" + serviceName;
            var requestUri = serviceToExecute + "?" + payload;
            var req = BuildGetWebRequest(requestUri, connection.AuthenticationType, connection.UserName, connection.Password,isDebugMode) ?? BuildPostRequest(serviceToExecute, payload, connection.AuthenticationType, connection.UserName, connection.Password, isDebugMode);
            Dev2Logger.Debug("Executing the remote request.");
            if(req != null)
            {
                using (var response = req.GetResponse() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        // ReSharper disable AssignNullToNotNullAttribute
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            // ReSharper restore AssignNullToNotNullAttribute
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }
            }

            return result;
        }

        private WebRequest BuildPostRequest(string serviceToExecute, string payload, AuthenticationType authenticationType, string userName, string password, bool isDebug)
        {
            var escapeUriString = Uri.EscapeUriString(serviceToExecute);
            var req = WebRequest.Create(escapeUriString);
            req.Method = "POST";
            UpdateRequest(authenticationType, userName, password, isDebug, req);

            byte[] data = Encoding.ASCII.GetBytes(payload);

            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;

            using(Stream requestStream = req.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
            }       
            return req;
        }

        private void UpdateRequest(AuthenticationType authenticationType, string userName, string password, bool isDebug, WebRequest req)
        {
            if(authenticationType == AuthenticationType.Windows)
            {
                req.UseDefaultCredentials = true;
            }
            else
            {
                req.UseDefaultCredentials = false;

                // we to default to the hidden public user name of \, silly know but that is how to get around ntlm auth ;)
                if(authenticationType == AuthenticationType.Public)
                {
                    userName = GlobalConstants.PublicUsername;
                    password = string.Empty;
                }

                req.Credentials = new NetworkCredential(userName, password);
            }
            var remoteInvokerId = DataObject.RemoteInvokerID;
            if(remoteInvokerId == Guid.Empty.ToString())
            {
                throw new Exception("Remote Server ID Empty");
            }
            req.Headers.Add(HttpRequestHeader.From, remoteInvokerId); // Set to remote invoke ID ;)
            req.Headers.Add(HttpRequestHeader.Cookie, isDebug ? GlobalConstants.RemoteServerInvoke : GlobalConstants.RemoteDebugServerInvoke);
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        WebRequest BuildGetWebRequest(string requestUri, AuthenticationType authenticationType, string userName, string password,bool isdebug)
        {
            try
            {
                var req = BuildSimpleGetWebRequest(requestUri);
                UpdateRequest(authenticationType, userName, password, isdebug, req);
                return req;
            }
            catch (Exception)
            {
                return null;
            }
        }

        WebRequest BuildSimpleGetWebRequest(string requestUri)
        {
            try
            {
                var escapeUriString = Uri.EscapeUriString(requestUri);
                var req = WebRequest.Create(escapeUriString);
                req.Method = "GET";
                return req;
            }
            catch (Exception)
            {
                return null;
            }
        }

        Connection GetConnection(Guid environmentId)
        {
            if (environmentId == Guid.Empty)
            {
                var localhostConnection = new Connection
                {
                    Address = EnvironmentVariables.WebServerUri,
                    AuthenticationType = AuthenticationType.Windows
                };
                return localhostConnection;
            }
            var xml = _resourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, environmentId);

            if (xml == null || xml.Length == 0)
            {
                return null;
            }

            var xe = xml.ToXElement();
            return new Connection(xe);
        }

        public SerializableResource FetchRemoteResource(Guid serviceId, string serviceName, bool isDebugMode)
        {
            var connection = GetConnection(DataObject.EnvironmentID);
            if (connection == null)
            {
                return null;
            }
            try
            {
                var returnData = ExecuteGetRequest(connection, "FindResourceService", string.Format("ResourceType={0}&ResourceName={1}&ResourceId={2}", "TypeWorkflowService", serviceName, serviceId), isDebugMode);
                if (!string.IsNullOrEmpty(returnData))
                {
                    Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                    var serializableResources = serializer.Deserialize<IList<SerializableResource>>(returnData);
                    return serializableResources.FirstOrDefault(resource => resource.ResourceType == ResourceType.WorkflowService);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
    }
}
