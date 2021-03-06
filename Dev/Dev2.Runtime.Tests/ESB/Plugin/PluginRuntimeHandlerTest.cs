
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
using System.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using DummyNamespaceForTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ESB.Plugin
{
    /// <summary>
    /// Summary description for PluginRuntimeHandlerTest
    /// </summary>
    [TestClass]
    public class PluginRuntimeHandlerTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region FetchNamespaceListObject

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        public void PluginRuntimeHandler_FetchNamespaceListObject_WhenValidDll_ExpectNamespaces()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource();
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.FetchNamespaceListObject(source);
                //------------Assert Results-------------------------
                Assert.IsTrue(result.Count > 0);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_FetchNamespaceListObject_WhenNullDll_ExpectException()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource();
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.FetchNamespaceListObject(null);
                //------------Assert Results-------------------------
            }
            
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_FetchNamespaceListObject_WhenNullLocationInSource_ExpectException()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource(true);
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObject(source);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_FetchNamespaceListObject_WhenNullLocationAndInvalidSourceID_ExpectException()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource(true);
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObject(source);
            }

        }

        #endregion

        #region ValidatePlugin

        //[TestMethod]
        //[Owner("Travis Frisinger")]
        //[TestCategory("PluginRuntimeHandler_ValidatePlugin")]
        //public void PluginRuntimeHandler_ValidatePlugin_WhenValidDll_ExpectBlankMessage()
        //{
        //    //------------Setup for test--------------------------
        //    var pluginRuntimeHandler = new PluginRuntimeHandler();
        //    var source = CreatePluginSource();

        //    //------------Execute Test---------------------------
        //    var result = pluginRuntimeHandler.ValidatePlugin(source.AssemblyLocation);

        //    //------------Assert Results-------------------------
        //    StringAssert.Contains(result, string.Empty);
        //}

        //[TestMethod]
        //[Owner("Travis Frisinger")]
        //[TestCategory("PluginRuntimeHandler_ValidatePlugin")]
        //public void PluginRuntimeHandler_ValidatePlugin_WhenNotADll_ExpectErrorMessage()
        //{
        //    //------------Setup for test--------------------------
        //    var pluginRuntimeHandler = new PluginRuntimeHandler();
        //    var source = CreatePluginSource();

        //    //------------Execute Test---------------------------
        //    var result = pluginRuntimeHandler.ValidatePlugin(source.AssemblyLocation + ".foo");

        //    //------------Assert Results-------------------------
        //    StringAssert.Contains(result, "Not a Dll file");
        //}

        //[TestMethod]
        //[Owner("Travis Frisinger")]
        //[TestCategory("PluginRuntimeHandler_ValidatePlugin")]
        //public void PluginRuntimeHandler_ValidatePlugin_WhenGacDll_ExpectBlankMessage()
        //{
        //    //------------Setup for test--------------------------
        //    var pluginRuntimeHandler = new PluginRuntimeHandler();

        //    //------------Execute Test---------------------------
        //    var result = pluginRuntimeHandler.ValidatePlugin("GAC:mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

        //    //------------Assert Results-------------------------
        //    StringAssert.Contains(result, string.Empty);
        //}

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_ValidatePlugin")]
        public void PluginRuntimeHandler_ValidatePlugin_WhenInvalidGacDll_ExpectErrorMessage()
        {
            //------------Setup for test--------------------------
            var pluginRuntimeHandler = new PluginRuntimeHandler();

            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.ValidatePlugin("GAC:mscorlib_foo, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                StringAssert.Contains(result, "Could not load file or assembly 'mscorlib_foo");
            }
            
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_ValidatePlugin")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_ValidatePlugin_WhenNullDll_ExpectErrorMessage()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.ValidatePlugin(null);
            }
        }

        #endregion

        #region ListNamespaces

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_ListNamespaces")]
        public void PluginRuntimeHandler_ListNamespaces_WhenValidLocation_ExpectNamespaces()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource();
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.ListNamespaces(source.AssemblyLocation, "Foo");
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_ListNamespaces")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_ListNamespaces_WhenNullLocation_ExpectException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.ListNamespaces(null, "Foo");
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_ListNamespaces")]
        public void PluginRuntimeHandler_ListNamespaces_WhenInvalidLocation_ExpectNoResults()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = isolated.Value.ListNamespaces("z:\foo\asm.dll", "Foo");
                Assert.IsFalse(result.Any());
            }            
        }

        #endregion

        #region Run

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_Run")]
        public void PluginRuntimeHandler_Run_WhenValidLocation_ExpectResult()
        {
            //------------Setup for test--------------------------
            var svc = CreatePluginService();
            var source = CreatePluginSource();
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = source.AssemblyLocation, AssemblyName = "Foo", Fullname = svc.Namespace, Method = svc.Method.Name, Parameters = svc.Method.Parameters };
                var result = isolated.Value.Run(args);
                var castResult = result as DummyClassForPluginTest;
                //------------Assert Results-------------------------
                if (castResult != null)
                {
                    StringAssert.Contains(castResult.Name, "test data");
                }
                else
                {
                    Assert.Fail("Failed Conversion for Assert");
                }
            }   

            
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_Run")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_Run_WhenNullLocation_ExpectException()
        {
            //------------Setup for test--------------------------
            var svc = CreatePluginService();
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = null, AssemblyName = "Foo", Fullname = svc.Namespace, Method = svc.Method.Name, Parameters = svc.Method.Parameters };
                var result = isolated.Value.Run(args);            
            }   
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_Run")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_Run_WhenInvalidNamespace_ExpectException()
        {
            //------------Setup for test--------------------------
            var svc = CreatePluginService();
            var source = CreatePluginSource();
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = source.AssemblyLocation, AssemblyName = "Foo", Fullname = "foo.bar", Method = svc.Method.Name, Parameters = svc.Method.Parameters };
                var result = isolated.Value.Run(args);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_Run")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_Run_WhenInvalidMethod_ExpectException()
        {
            //------------Setup for test--------------------------
            var svc = CreatePluginService();
            var source = CreatePluginSource();
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = source.AssemblyLocation, AssemblyName = "Foo", Fullname = svc.Namespace, Method = "InvalidName", Parameters = svc.Method.Parameters };
                var result = isolated.Value.Run(args);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginRuntimeHandler_Run")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_Run_WhenNullParameters_ExpectException()
        {
            //------------Setup for test--------------------------
            var svc = CreatePluginService();
            var source = CreatePluginSource();
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = source.AssemblyLocation, AssemblyName = "Foo", Fullname = svc.Namespace, Method = svc.Method.Name, Parameters = null };
                var result = isolated.Value.Run(args);
            }
        }

        #endregion

        #region Helper Methods

        static PluginSource CreatePluginSource(bool nullLocation = false, bool invalidResourceID = false)
        {
            var type = typeof(DummyClassForPluginTest);
            var assembly = type.Assembly;

            string loc = null;
            if(!nullLocation)
            {
                loc = assembly.Location;
            }

            Guid resourceID = Guid.Empty;
            if(!invalidResourceID)
            {
                resourceID = Guid.NewGuid();
            }

            return new PluginSource
            {
                AssemblyLocation = loc,
                ResourceID = resourceID,
                ResourceName = "Dummy",
                ResourceType = ResourceType.PluginSource,
                ResourcePath = "Test",
            };
        }

        public static PluginService CreatePluginService()
        {
            return CreatePluginService(new ServiceMethod
            {
                Name = "DummyMethod"
            });
        }

        public static PluginService CreatePluginService(ServiceMethod method)
        {
            var type = typeof(DummyClassForPluginTest);

            var source = CreatePluginSource();
            var service = new PluginService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DummyPluginService",
                ResourceType = ResourceType.PluginService,
                ResourcePath = "Tests",
                Namespace = type.FullName,
                Method = method,
                Source = source
            };
            return service;
        }

        #endregion
    }

    public sealed class Isolated<T> : IDisposable where T : MarshalByRefObject
    {
        private AppDomain _domain;
        private T _value;

        public Isolated()
        {
            _domain = AppDomain.CreateDomain("Isolated:" + Guid.NewGuid(),
               null, AppDomain.CurrentDomain.SetupInformation);

            Type type = typeof(T);

            _value = (T)_domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }

        public T Value
        {
            get
            {
                return _value;
            }
        }

        public void Dispose()
        {
            if (_domain != null)
            {
                AppDomain.Unload(_domain);

                _domain = null;
            }
        }
    }
}
