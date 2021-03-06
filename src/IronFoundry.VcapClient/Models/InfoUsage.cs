// -----------------------------------------------------------------------
// <copyright file="InfoUsage.cs" company="Tier 3">
// Copyright © 2012 Tier 3 Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace IronFoundry.Models
{
    public class InfoUsage : EntityBase
    {
        [JsonProperty(PropertyName = "memory")]
        public uint Memory { get; private set; }

        [JsonProperty(PropertyName = "apps")]
        public uint Apps { get; private set; }

        [JsonProperty(PropertyName = "services")]
        public uint Services { get; private set; }
    }
}