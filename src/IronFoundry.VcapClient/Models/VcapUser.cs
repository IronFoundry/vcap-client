// -----------------------------------------------------------------------
// <copyright file="VcapUser.cs" company="Tier 3">
// Copyright © 2012 Tier 3 Inc., All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace IronFoundry.Models
{
    using System;
    using Newtonsoft.Json;

    [Serializable]
    public class VcapUser
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "admin")]
        public bool Admin { get; set; }

        [JsonProperty(PropertyName = "apps")]
        public VcapUserApp[] Apps { get; set; }
    }
}
