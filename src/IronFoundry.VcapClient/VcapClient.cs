// -----------------------------------------------------------------------
// <copyright file="VcapClient.cs" company="Tier 3">
// Copyright © 2012 Tier 3 Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using IronFoundry.Extensions;
using IronFoundry.Models;

namespace IronFoundry.VcapClient
{
    public class VcapClient : IVcapClient
    {
        private static readonly Regex FileRe;
        private static readonly Regex DirRe;
        private VcapCredentialManager _credMgr;
        private VcapUser _proxyUser;

        static VcapClient()
        {
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            string validFileNameRegexStr = String.Format(@"^([^{0}]+)\s+([0-9]+(?:\.[0-9]+)?[KBMG])$",
                                                         new String(invalidFileNameChars));
            FileRe = new Regex(validFileNameRegexStr, RegexOptions.Compiled);

            char[] invalidPathChars = Path.GetInvalidPathChars();
            string validPathRegexStr = String.Format(@"^([^{0}]+)/\s+-$", new String(invalidPathChars));
            DirRe = new Regex(validPathRegexStr, RegexOptions.Compiled);
        }

        public VcapClient()
        {
            _credMgr = new VcapCredentialManager();
        }

        public VcapClient(string uri)
        {
            Target(uri);
        }

        public VcapClient(Uri uri, IPAddress ipAddress)
        {
            _credMgr = new VcapCredentialManager(uri, ipAddress);
        }

        #region IVcapClient Members

        public void ProxyAs(VcapUser user)
        {
            _proxyUser = user;
        }

        public string CurrentUri
        {
            get { return _credMgr.CurrentTarget.AbsoluteUriTrimmed(); }
        }

        public string CurrentToken
        {
            get { return _credMgr.CurrentToken; }
        }

        public Info GetInfo()
        {
            var helper = new MiscHelper(_proxyUser, _credMgr);
            return helper.GetInfo();
        }

        public void Target(string uri)
        {
            Target(uri, null);
        }

        public void Target(string uri, IPAddress ipAddress)
        {
            if (uri.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("uri");
            }

            Uri validatedUri;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out validatedUri))
            {
                validatedUri = new Uri("http://" + uri);
            }

            _credMgr = ipAddress == null
                           ? new VcapCredentialManager(validatedUri)
                           : new VcapCredentialManager(validatedUri, ipAddress);
        }

        public string CurrentTarget
        {
            get { return _credMgr.CurrentTarget.AbsoluteUriTrimmed(); }
        }

        public void Login(string email, string password)
        {
            var helper = new UserHelper(_proxyUser, _credMgr);
            helper.Login(email, password);
        }

        public void ChangePassword(string newPassword)
        {
            var helper = new UserHelper(_proxyUser, _credMgr);
            Info info = GetInfo();
            helper.ChangePassword(info.User, newPassword);
        }

        public void AddUser(string email, string password)
        {
            var helper = new UserHelper(_proxyUser, _credMgr);
            helper.AddUser(email, password);
        }

        public void DeleteUser(string email)
        {
            var helper = new UserHelper(_proxyUser, _credMgr);
            helper.DeleteUser(email);
        }

        public VcapUser GetUser(string email)
        {
            var helper = new UserHelper(_proxyUser, _credMgr);
            return helper.GetUser(email);
        }

        public IEnumerable<VcapUser> GetUsers()
        {
            var helper = new UserHelper(_proxyUser, _credMgr);
            return helper.GetUsers();
        }

        public void Push(string name, string deployFQDN, ushort instances,
                         DirectoryInfo path, uint memoryMB, string[] provisionedServiceNames)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.Push(name, deployFQDN, instances, path, memoryMB, provisionedServiceNames);
        }

        public void Update(string name, DirectoryInfo path)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.Update(name, path);
        }

        public void BindService(string provisionedServiceName, string appName)
        {
            var services = new ServicesHelper(_proxyUser, _credMgr);
            services.BindService(provisionedServiceName, appName);
        }

        public void UnbindService(string provisionedServiceName, string appName)
        {
            var services = new ServicesHelper(_proxyUser, _credMgr);
            services.UnbindService(provisionedServiceName, appName);
        }

        public void CreateService(string serviceName, string provisionedServiceName)
        {
            var services = new ServicesHelper(_proxyUser, _credMgr);
            services.CreateService(serviceName, provisionedServiceName);
        }

        public void DeleteService(string provisionedServiceName)
        {
            var services = new ServicesHelper(_proxyUser, _credMgr);
            services.DeleteService(provisionedServiceName);
        }

        public IEnumerable<SystemService> GetSystemServices()
        {
            var services = new ServicesHelper(_proxyUser, _credMgr);
            return services.GetSystemServices();
        }

        public IEnumerable<ProvisionedService> GetProvisionedServices()
        {
            var services = new ServicesHelper(_proxyUser, _credMgr);
            return services.GetProvisionedServices();
        }

        public void Start(string appName)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.Start(appName);
        }

        public void Start(Application app)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.Start(app);
        }

        public void Stop(string appName)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.Stop(appName);
        }

        public void Stop(Application app)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.Stop(app);
        }

        public void Restart(string appName)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.Restart(appName);
        }

        public void Restart(Application app)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.Restart(app);
        }

        public void Delete(string appName)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.Delete(appName);
        }

        public void Delete(Application app)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.Delete(app);
        }

        public Application GetApplication(string name)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            Application rv = helper.GetApplication(name);
            return rv;
        }

        public IEnumerable<Application> GetApplications()
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            IEnumerable<Application> apps = helper.GetApplications();
            foreach (Application app in apps) // TODO not thrilled about this
            {
                app.User = _proxyUser;
            }
            return apps;
        }

        public byte[] FilesSimple(string appName, string path, ushort instance)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            return helper.Files(appName, path, instance);
        }

        public VcapFilesResult Files(string appName, string path, ushort instance)
        {
            VcapFilesResult rv;

            var helper = new AppsHelper(_proxyUser, _credMgr);
            byte[] content = helper.Files(appName, path, instance);
            if (null == content)
            {
                rv = new VcapFilesResult(false);
            }
            else if (content.Length == 0)
            {
                rv = new VcapFilesResult(content);
            }
            else
            {
                int i;
                for (i = 0; i < content.Length; ++i)
                {
                    if (content[i] == '\n')
                    {
                        break;
                    }
                }
                string firstLine = Encoding.ASCII.GetString(content, 0, i);
                if (FileRe.IsMatch(firstLine) || DirRe.IsMatch(firstLine))
                {
                    // Probably looking at a listing, not a file
                    string contentAscii = Encoding.ASCII.GetString(content);
                    string[] contentAry = contentAscii.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
                    rv = new VcapFilesResult();
                    foreach (string item in contentAry)
                    {
                        Match fileMatch = FileRe.Match(item);
                        if (fileMatch.Success)
                        {
                            string fileName = fileMatch.Groups[1].Value; // NB: 0 is the entire matched string
                            string fileSize = fileMatch.Groups[2].Value;
                            rv.AddFile(fileName, fileSize);
                            continue;
                        }

                        Match dirMatch = DirRe.Match(item);
                        if (dirMatch.Success)
                        {
                            string dirName = dirMatch.Groups[1].Value;
                            rv.AddDirectory(dirName);
                            continue;
                        }

                        throw new InvalidOperationException("Match failed.");
                    }
                }
                else
                {
                    rv = new VcapFilesResult(content);
                }
            }

            return rv;
        }

        public void UpdateApplication(Application app)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            helper.UpdateApplication(app);
        }

        public string GetLogs(Application app, ushort instanceNumber)
        {
            var info = new InfoHelper(_proxyUser, _credMgr);
            return info.GetLogs(app, instanceNumber);
        }

        public IEnumerable<StatInfo> GetStats(Application app)
        {
            var info = new InfoHelper(_proxyUser, _credMgr);
            return info.GetStats(app);
        }

        public IEnumerable<ExternalInstance> GetInstances(Application app)
        {
            var info = new InfoHelper(_proxyUser, _credMgr);
            return info.GetInstances(app);
        }

        public IEnumerable<Crash> GetAppCrash(Application app)
        {
            var helper = new AppsHelper(_proxyUser, _credMgr);
            return helper.GetAppCrash(app);
        }

        #endregion

        internal VcapRequest GetRequestForTesting()
        {
            var helper = new MiscHelper(_proxyUser, _credMgr);
            return helper.BuildInfoRequest();
        }
    }
}