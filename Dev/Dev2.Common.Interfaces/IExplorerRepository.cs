﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Common.Interfaces
{
    public interface IExplorerRepository
    {
        bool Rename(IExplorerItemViewModel vm, string newName);

        Task<bool>  Move(IExplorerItemViewModel explorerItemViewModel, IExplorerTreeItem destination);

        bool Delete(IExplorerItemViewModel explorerItemViewModel);

        ICollection<IVersionInfo> GetVersions(Guid id);

        IRollbackResult Rollback(Guid resourceId, string version);

        void CreateFolder(string parentPath, string name, Guid id);

        IExplorerItem ExplorerItems { get; set; }

        Task<IExplorerItem> LoadExplorer();

        IExplorerItem FindItem(Func<IExplorerItem, bool> searchCriteria);
        IExplorerItem FindItemByID(Guid id);
    }
}
