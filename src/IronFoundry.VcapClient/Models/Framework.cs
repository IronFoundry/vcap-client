// -----------------------------------------------------------------------
// <copyright file="Framework.cs" company="Tier 3">
// Copyright © 2012 Tier 3 Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace IronFoundry.Models
{
    public class Framework : EntityBase
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "runtimes")]
        public Runtime[] Runtimes { get; private set; }

        [JsonProperty(PropertyName = "appservers")]
        public AppServer[] AppServers { get; private set; }
    }
}