
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
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class DeleteItemService : IEsbManagementEndpoint
    {
        private IExplorerServerResourceRepository _serverExplorerRepository;

        public string HandlesType()
        {
            return "DeleteItemService";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            IExplorerRepositoryResult item = null;
            var serializer = new Dev2JsonSerializer();
            try
            {
                if(values == null)
                {
                    throw new ArgumentNullException("values");
                }               
                StringBuilder itemBeingDeleted;
                StringBuilder pathBeingDeleted=null;
                if (!values.TryGetValue("itemToDelete", out itemBeingDeleted))
                {
                    if (!values.TryGetValue("folderToDelete", out pathBeingDeleted))
                    {
                        throw new ArgumentException("itemToDelete value not supplied.");
                    }
                }
                IExplorerItem itemToDelete;
                if (itemBeingDeleted != null)
                {
                    itemToDelete = ServerExplorerRepo.Find(a => a.ResourceId.ToString() == itemBeingDeleted.ToString());
                    Dev2Logger.Info("Delete Item Service." + itemToDelete);
                    item = ServerExplorerRepo.DeleteItem(itemToDelete, GlobalConstants.ServerWorkspaceID);
                }
                else if(pathBeingDeleted != null)
                {
                    itemToDelete = new ServerExplorerItem()
                    {
                        ResourceType =  ResourceType.Folder,
                        ResourcePath = pathBeingDeleted.ToString()
                        
                    };
                    item = ServerExplorerRepo.DeleteItem(itemToDelete, GlobalConstants.ServerWorkspaceID);
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error("Delete Item Error" ,e);
                item = new ExplorerRepositoryResult(ExecStatus.Fail, e.Message);
            }
            return serializer.SerializeToBuilder(item);
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><itemToAdd ColumnIODirection=\"itemToDelete\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }
        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }
    }
}
