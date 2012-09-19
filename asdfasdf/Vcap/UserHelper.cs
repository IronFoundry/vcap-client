﻿namespace IronFoundry.Vcap
{
    using System;
    using System.Collections.Generic;
    using Properties;
    using Models;
    using Newtonsoft.Json.Linq;
    using RestSharp;

    internal class UserHelper : BaseVmcHelper
    {
        public UserHelper(VcapUser proxyUser, VcapCredentialManager credentialManager)
            : base(proxyUser, credentialManager) { }

        public void Login(string email, string password)
        {
            VcapJsonRequest r = BuildVcapJsonRequest(Method.POST, Constants.USERS_PATH, email, "tokens");
            r.AddBody(new { password });

            try
            {
                IRestResponse response = r.Execute();
                var parsed = JObject.Parse(response.Content);
                string token = parsed.Value<string>("token");
                CredentialManager.RegisterToken(token);
            }
            catch (VcapAuthException)
            {
                throw new VcapAuthException(string.Format(Resources.Vmc_LoginFail_Fmt, CredentialManager.CurrentTarget));
            }
        }

        public void ChangePassword(string user, string newPassword)
        {
            VcapRequest request = BuildVcapRequest(Constants.USERS_PATH, user);
            IRestResponse response = request.Execute();

            JObject parsed = JObject.Parse(response.Content);
            parsed["password"] = newPassword;

            VcapJsonRequest put = BuildVcapJsonRequest(Method.PUT, Constants.USERS_PATH, user);
            put.AddBody(parsed);
            put.Execute();
        }

        public void AddUser(string email, string password)
        {
            VcapJsonRequest r = BuildVcapJsonRequest(Method.POST, Constants.USERS_PATH);
            r.AddBody(new { email, password });
            r.Execute();
        }

        public void DeleteUser(string email)
        {
            // TODO: doing this causes a "not logged in" failure when the user
            // doesn't exist, which is kind of misleading.

            var appsHelper = new AppsHelper(ProxyUser, CredentialManager);
            foreach (Application a in appsHelper.GetApplications(email))
            {
                appsHelper.Delete(a.Name);
            }

            var servicesHelper = new ServicesHelper(ProxyUser, CredentialManager);
            foreach (ProvisionedService ps in servicesHelper.GetProvisionedServices())
            {
                servicesHelper.DeleteService(ps.Name);
            }

            VcapJsonRequest r = BuildVcapJsonRequest(Method.DELETE, Constants.USERS_PATH, email);
            r.Execute();
        }

        public VcapUser GetUser(string email)
        {
            VcapJsonRequest r = BuildVcapJsonRequest(Method.GET, Constants.USERS_PATH, email);
            return r.Execute<VcapUser>();
        }

        public IEnumerable<VcapUser> GetUsers()
        {
            VcapJsonRequest r = BuildVcapJsonRequest(Method.GET, Constants.USERS_PATH);
            return r.Execute<VcapUser[]>();
        }
    }
}