﻿using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016.ExceptionTests
{
    [TestClass]
    public class DropboxFileNotFoundExceptionTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenMessege_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropboxFileNotFoundException = new DropboxFileNotFoundException();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxFileNotFoundException);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenMessege_ShouldHaveMessegeSet()
        {
            //---------------Set up test pack-------------------
            var dropboxFileNotFoundException = new DropboxFileNotFoundException();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxFileNotFoundException);
            //---------------Execute Test ----------------------
            var message = dropboxFileNotFoundException.Message;
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.DropboxPathNotFoundException, message);
            
        }
    } 
    
    [TestClass]
    public class DropboxPathNotFileFoundExceptionTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenMessege_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropboxPathNotFileFoundException = new DropboxPathNotFileFoundException();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxPathNotFileFoundException);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenMessege_ShouldHaveMessegeSet()
        {
            //---------------Set up test pack-------------------
            var dropboxPathNotFileFoundException = new DropboxPathNotFileFoundException();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dropboxPathNotFileFoundException);
            //---------------Execute Test ----------------------
            var message = dropboxPathNotFileFoundException.Message;
            //---------------Test Result -----------------------
            Assert.AreEqual(GlobalConstants.DropboxPathNotFileException, message);
            
        }
    }
}
