// -----------------------------------------------------------------------
// <copyright file="UserHelper.cs" company="Tier 3">
// Copyright © 2012 Tier 3 Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace IronFoundry.VcapClient
{
    using System.Collections.Generic;
    using Models;
    using Newtonsoft.Json.Linq;
    using Properties;
    using RestSharp;

    internal class UserHelper : BaseVmcHelper, IUserHelper
    {
        public UserHelper(VcapUser proxyUser, VcapCredentialManager credentialManager)
            : base(proxyUser, credentialManager)
        {
        }

        public void Login(string email, string password)
        {
            var r = BuildVcapJsonRequest(Method.POST, Constants.UsersResource, email, "tokens");
            r.AddBody(new { password });

            try
            {
                var response = r.Execute();
                var parsed = JObject.Parse(response.Content);
                var token = parsed.Value<string>("token");
                credentialManager.RegisterToken(token);
            }
            catch (VcapAuthException)
            {
                throw new VcapAuthException(string.Format(Resources.Vmc_LoginFail_Fmt, credentialManager.CurrentTarget));
            }
        }

        public void ChangePassword(string user, string newPassword)
        {
            VcapRequest request = BuildVcapRequest(Constants.UsersResource, user);
            IRestResponse response = request.Execute();

            JObject parsed = JObject.Parse(response.Content);
            parsed["password"] = newPassword;

            VcapJsonRequest put = BuildVcapJsonRequest(Method.PUT, Constants.UsersResource, user);
            put.AddBody(parsed);
            put.Execute();
        }

        public void AddUser(string email, string password)
        {
            VcapJsonRequest r = BuildVcapJsonRequest(Method.POST, Constants.UsersResource);
            r.AddBody(new { email, password });
            r.Execute();
        }

        public void DeleteUser(string email)
        {
            // TODO: doing this causes a "not logged in" failure when the user
            // doesn't exist, which is kind of misleading.
            var appsHelper = new AppsHelper(proxyUser, credentialManager);
            foreach (Application a in appsHelper.GetApplications(email))
            {
                appsHelper.Delete(a.Name);
            }

            var servicesHelper = new ServicesHelper(proxyUser, credentialManager);
            foreach (ProvisionedService ps in servicesHelper.GetProvisionedServices())
            {
                servicesHelper.DeleteService(ps.Name);
            }

            VcapJsonRequest r = BuildVcapJsonRequest(Method.DELETE, Constants.UsersResource, email);
            r.Execute();
        }

        public VcapUser GetUser(string email)
        {
            VcapJsonRequest r = BuildVcapJsonRequest(Method.GET, Constants.UsersResource, email);
            return r.Execute<VcapUser>();
        }

        public IEnumerable<VcapUser> GetUsers()
        {
            VcapJsonRequest r = BuildVcapJsonRequest(Method.GET, Constants.UsersResource);
            return r.Execute<VcapUser[]>();
        }
    }
}
