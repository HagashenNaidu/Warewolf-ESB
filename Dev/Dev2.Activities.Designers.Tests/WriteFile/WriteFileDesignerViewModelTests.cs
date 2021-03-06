
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.WriteFile
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class WriteFileDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WriteFileDesignerViewModel_Constructor")]
        public void WriteFileDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = WriteFileViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("File Name", viewModel.OutputPathLabel);
            Assert.AreEqual("", viewModel.InputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(0, viewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WriteFileDesignerViewModel_Validate")]
        public void WriteFileDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new ResourceModel(null));
            DataListSingleton.SetDataList(dataListViewModel);
            var viewModel = WriteFileViewModel();

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(0, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WriteFileDesignerViewModel_Constructor")]
        public void WriteFileDesignerViewModel_Contructor_OverwriteIsSetToTrue()
        {
            //------------Setup for test-------------------------
            var viewModel = WriteFileViewModel();
            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.Overwrite);
        }

        static TestWriteFileDesignerViewModel WriteFileViewModel()
        {
            var viewModel = new TestWriteFileDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfFileWrite()));
            return viewModel;
        }
    }
}
