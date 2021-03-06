﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.String.Json;
using Warewolf.Core;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("WebMethods", "POST", ToolType.Native, "6AEB1038-6332-46F9-8BDD-752DE4EA038E", "Dev2.Activities", "1.0.0.0", "Legacy", "HTTP Web Methods", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfWebPostActivity:DsfActivity
    {
        public IList<INameValue> Headers { get; set; }
        public string QueryString { get; set; }
        public IOutputDescription OutputDescription { get; set; }
        public string PostData { get; set; }

        public DsfWebPostActivity()
        {
            Type = "POST Web Method";
            DisplayName = "POST Web Method";
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DataGridActivity;
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null) return _debugInputs;
            base.GetDebugInputs(env, update);

            IEnumerable<NameValue> head = null;
            if (Headers != null)
            {
                head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(a.Value, update)))).Where(a => !(String.IsNullOrEmpty(a.Name) && String.IsNullOrEmpty(a.Value)));
            }

            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            string headerString=string.Empty;
            if (head != null)
                headerString = string.Join(" ", head.Select(a => a.Name + " : " + a.Value));

            DebugItem debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "URL"), debugItem);
            AddDebugItem(new DebugEvalResult(url.Address, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Query String"), debugItem);
            AddDebugItem(new DebugEvalResult(QueryString, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Post Data"), debugItem);
            AddDebugItem(new DebugEvalResult(PostData, "", env, update), debugItem);
            _debugInputs.Add(debugItem);
            debugItem = new DebugItem();
            AddDebugItem(new DebugItemStaticDataParams("", "Headers"), debugItem);
            AddDebugItem(new DebugEvalResult(headerString, "", env, update), debugItem);
            _debugInputs.Add(debugItem);

            return _debugInputs;
        }

        #region Overrides of DsfActivity
       

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            tmpErrors = new ErrorResultTO();
            IEnumerable<NameValue> head = null;
            if (Headers != null)
            {
                head = Headers.Select(a => new NameValue(ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(a.Name, update)), ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(a.Value, update))));
            }
            var query = "";
            if (QueryString != null)
            {
                query = ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(QueryString, update));
            }
            var postData = "";
            if (PostData != null)
            {
                postData = ExecutionEnvironment.WarewolfEvalResultToString(dataObject.Environment.Eval(PostData, update));    
            }
            var url = ResourceCatalog.GetResource<WebSource>(Guid.Empty, SourceId);
            var webRequestResult = PerformWebPostRequest(head, query, url, postData);
            PushXmlIntoEnvironment(webRequestResult, update, dataObject);
        }

        private void PushXmlIntoEnvironment(string input, int update, IDSFDataObject dataObj)
        {
            if (OutputDescription == null)
            {
                dataObj.Environment.AddError("There are no outputs");
                return;
            }
            int i = 0;
            foreach (var serviceOutputMapping in Outputs)
            {
                OutputDescription.DataSourceShapes[0].Paths[i].OutputExpression = DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedFrom);
                i++;
            }
            if (OutputDescription.DataSourceShapes.Count == 1 && OutputDescription.DataSourceShapes[0].Paths.All(a => a is StringPath))
            {
                dataObj.Environment.Assign(Outputs.First().MappedTo, input, update);
                return;
            }
            var formater = OutputFormatterFactory.CreateOutputFormatter(OutputDescription);
            try
            {
                input = formater.Format(input).ToString();
                if (input != string.Empty)
                {

                    string toLoad = DataListUtil.StripCrap(input); // clean up the rubish ;)
                    XmlDocument xDoc = new XmlDocument();
                    toLoad = string.Format("<Tmp{0}>{1}</Tmp{0}>", Guid.NewGuid().ToString("N"), toLoad);
                    xDoc.LoadXml(toLoad);

                    if (xDoc.DocumentElement != null)
                    {
                        XmlNodeList children = xDoc.DocumentElement.ChildNodes;
                        IDictionary<string, int> indexCache = new Dictionary<string, int>();
                        var outputDefs = Outputs.Select(a => new Dev2Definition(a.MappedFrom, a.MappedTo, "", a.RecordSetName, true, "", true, a.MappedTo) as IDev2Definition).ToList();
                        TryConvert(children, outputDefs, indexCache, update, dataObj);
                    }
                }

            }
            catch (Exception e)
            {
                dataObj.Environment.AddError(e.Message);
                Dev2Logger.Error(e.Message, e);
            }
        }

        void TryConvert(XmlNodeList children, IList<IDev2Definition> outputDefs, IDictionary<string, int> indexCache, int update, IDSFDataObject dataObj, int level = 0)
        {
            // spin through each element in the XML
            foreach (XmlNode c in children)
            {
                // scalars and recordset fetch
                if (level > 0)
                {
                    var c1 = c;
                    var recSetName = outputDefs.Where(definition => definition.RecordSetName == c1.Name);
                    var dev2Definitions = recSetName as IDev2Definition[] ?? recSetName.ToArray();
                    if (dev2Definitions.Length != 0)
                    {
                        // fetch recordset index
                        int fetchIdx;
                        var idx = indexCache.TryGetValue(c.Name, out fetchIdx) ? fetchIdx : 1;
                        // process recordset
                        var nl = c.ChildNodes;
                        foreach (XmlNode subc in nl)
                        {
                            // Extract column being mapped to ;)
                            foreach (var definition in dev2Definitions)
                            {
                                if (definition.MapsTo == subc.Name || definition.Name == subc.Name)
                                {
                                    dataObj.Environment.AssignWithFrame(new AssignValue(definition.RawValue, UnescapeRawXml(subc.InnerXml)), update);
                                }
                            }
                        }
                        // update this recordset index
                        dataObj.Environment.CommitAssign();
                        indexCache[c.Name] = ++idx;
                    }
                    else
                    {
                        var nameToMatch = c1.Name;
                        var useValue = false;
                        if (c.Name == GlobalConstants.NaughtyTextNode)
                        {
                            if (c1.ParentNode != null)
                            {
                                nameToMatch = c1.ParentNode.Name;
                                useValue = true;
                            }
                        }
                        var scalarName = outputDefs.FirstOrDefault(definition => definition.Name == nameToMatch);
                        if (scalarName != null)
                        {
                            var value = UnescapeRawXml(c1.InnerXml);
                            if (useValue)
                            {
                                value = UnescapeRawXml(c1.Value);
                            }
                            dataObj.Environment.AssignWithFrame(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(scalarName.MapsTo), value), update);
                        }
                    }
                }
                else
                {
                    if (level == 0)
                    {
                        // Only recurse if we're at the first level!!
                        TryConvert(c.ChildNodes, outputDefs, indexCache, update, dataObj, ++level);
                    }
                }
            }
        }
        
        string UnescapeRawXml(string innerXml)
        {
            if (innerXml.StartsWith("&lt;") && innerXml.EndsWith("&gt;"))
            {
                return new StringBuilder(innerXml).Unescape().ToString();
            }
            return innerXml;
        }

        [ExcludeFromCodeCoverage]
        protected virtual string PerformWebPostRequest(IEnumerable<NameValue> head, string query, WebSource source, string postData)
        {
            var webclient = CreateClient(head, query, source);
            if (webclient != null)
            {
                var address = source.Address;
                if (query != null)
                {
                    address = address + query;
                }
                return webclient.UploadString(address, postData);
            }
            return null;
        }

        public WebClient CreateClient(IEnumerable<NameValue> head, string query, WebSource source)
        {
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => true;
            var webclient = new WebClient();
            if (head != null)
            {
                foreach (var nameValue in head)
                {
                    if (!String.IsNullOrEmpty(nameValue.Name) && !String.IsNullOrEmpty(nameValue.Value))
                        webclient.Headers.Add(nameValue.Name, nameValue.Value);
                }
            }

            if (source.AuthenticationType == AuthenticationType.User)
            {
                webclient.Credentials = new NetworkCredential(source.UserName, source.Password);
            }

            webclient.Headers.Add("user-agent", GlobalConstants.UserAgentString);
            var address = source.Address;
            if (query != null)
            {
                address = address + query;
            }
            webclient.BaseAddress = address;
            return webclient;
        }

        #endregion
    }
}
