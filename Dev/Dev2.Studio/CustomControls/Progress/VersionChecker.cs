
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
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Studio.Utils;
using Warewolf.Core;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Helpers
{
    public class VersionChecker : IVersionChecker
    {
        readonly IWarewolfWebClient _webClient;
        readonly Func<Version> _versionGetter;

        bool _isDone;
        Version _latest;
        Version _current;
        private string _latestVersionCheckSum;

        public VersionChecker()
            : this(new WarewolfWebClient(new WebClient()), VersionInfo.FetchVersionInfoAsVersion)
        {
        }

        public VersionChecker(IWarewolfWebClient webClient, Func<Version> versionGetter)
        {
            VerifyArgument.IsNotNull("webClient", webClient);
            VerifyArgument.IsNotNull("versionGetter", versionGetter);
            _webClient = webClient;
            _versionGetter = versionGetter;
            _isDone = false;
        }

        #region Latest

        public virtual Version Latest
        {
            get
            {
                Check();
                return _latest;
            }
        }

        #endregion

        #region Current

        public virtual Version Current
        {
            get
            {
                Check();
                return _current;
            }
        }

        #endregion

        #region StartPageUri

        public string StartPageUri
        {
            get
            {
                Check();
                return StringResources.Warewolf_Homepage_Start;
            }
        }

        public string LatestVersionCheckSum
        {
            get
            {
                Check();
                return _latestVersionCheckSum;
            }

        }
        public string CommunityPageUri
        {
            get
            {
                Check();
                return StringResources.Uri_Community_HomePage;
            }
        }

        #endregion

        #region IsLatest?

        public bool GetNewerVersion()
        {
            return Latest > Current;
        }

        public async Task<bool> GetNewerVersionAsync()
        {
            var latest = await GetLatestVersionAsync();
            return latest > Current;
        }

        #endregion

        #region Check

        protected virtual void Check()
        {
            if(!_isDone)
            {
                _isDone = true;
                _latest = GetLatestVersion();
                _current = GetCurrentVersion();
                _latestVersionCheckSum = GetLatestVersionCheckSum();
            }
        }

        private string GetLatestVersionCheckSum()
        {
            try
            {
                return _webClient.DownloadString(InstallerResources.WarewolfChecksum);

            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region GetLatestVersion

        async Task<Version> GetLatestVersionAsync()
        {

            try
            {
                var version = await _webClient.DownloadStringAsync(InstallerResources.WarewolfVersion);
                return new Version(version);
            }
            catch
            {
                return null;
            }
        }

        Version GetLatestVersion()
        {

            try
            {
                var version = _webClient.DownloadString(InstallerResources.WarewolfVersion);
                return new Version(version);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region GetCurrentVersion

        protected virtual Version GetCurrentVersion()
        {
            return _versionGetter();
        }

        #endregion
    }

    internal class InstallerResources
    {

        public static bool InstallerTesting
        {
            get { return ConfigurationManager.AppSettings["InstallerTesting"] == null || bool.Parse(ConfigurationManager.AppSettings["InstallerTesting"]); }
        }


        public static string WarewolfVersion
        {
            get
            {
                return InstallerTesting ? ConfigurationManager.AppSettings["TestVersionLocation"] : ConfigurationManager.AppSettings["VersionLocation"];
            }
        }
        public static string WarewolfChecksum
        {
            get
            {
                return InstallerTesting ? ConfigurationManager.AppSettings["TestCheckSumLocation"] : ConfigurationManager.AppSettings["CheckSumLocation"];
            }
        }
    }
}
