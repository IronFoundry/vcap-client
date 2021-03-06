// -----------------------------------------------------------------------
// <copyright file="InfoHelper.cs" company="Tier 3">
// Copyright © 2012 Tier 3 Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using IronFoundry.Models;
using Newtonsoft.Json;
using RestSharp;

namespace IronFoundry.VcapClient
{
    internal class InfoHelper : BaseVmcHelper
    {
        public InfoHelper(VcapUser proxyUser, VcapCredentialManager credentialManager)
            : base(proxyUser, credentialManager)
        {
        }

        public string GetLogs(Application argApp, ushort argInstance)
        {
            string logoutput = "";

            logoutput = "====stderr.log====\n";
            logoutput = logoutput + GetStdErrLog(argApp, argInstance);
            logoutput = logoutput + "\n====stdout.log====\n";
            logoutput = logoutput + GetStdOutLog(argApp, argInstance);
            logoutput = logoutput + "\n====startup.log====\n";
            logoutput = logoutput + GetStartupLog(argApp, argInstance);

            return logoutput;
        }

        public string GetStdErrLog(Application argApp, ushort argInstance)
        {
            VcapRequest r = base.BuildVcapRequest(Constants.AppsResource, argApp.Name, argInstance,
                                                  "files/logs/stderr.log");
            return r.Execute().Content;
        }

        public string GetStdOutLog(Application argApp, ushort argInstance)
        {
            VcapRequest r = base.BuildVcapRequest(Constants.AppsResource, argApp.Name, argInstance,
                                                  "files/logs/stdout.log");
            return r.Execute().Content;
        }

        public string GetStartupLog(Application argApp, ushort argInstance)
        {
            VcapRequest r = base.BuildVcapRequest(Constants.AppsResource, argApp.Name, argInstance,
                                                  "files/logs/startup.log");
            return r.Execute().Content;
        }

        public void GetFiles(Application argApp, ushort argInstance)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<StatInfo> GetStats(Application argApp)
        {
            return GetStats(null, argApp);
        }

        public IEnumerable<StatInfo> GetStats(VcapUser user, Application app)
        {
            VcapRequest r = base.BuildVcapRequest(Constants.AppsResource, app.Name, "stats");
            IRestResponse response = r.Execute();
            var tmp = JsonConvert.DeserializeObject<SortedDictionary<int, StatInfo>>(response.Content);

            var rv = new List<StatInfo>();
            foreach (var kvp in tmp)
            {
                StatInfo si = kvp.Value;
                si.ID = kvp.Key;
                rv.Add(si);
            }
            return rv.ToArrayOrNull();
        }

        public IEnumerable<ExternalInstance> GetInstances(Application argApp)
        {
            VcapRequest r = base.BuildVcapRequest(Constants.AppsResource, argApp.Name, "instances");
            var instances = r.Execute<Dictionary<string, ExternalInstance>>();
            return instances.Values.ToArrayOrNull();
        }
    }
}