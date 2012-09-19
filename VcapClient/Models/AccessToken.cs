﻿using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    using System;

    public class AccessToken
    {
        private readonly Uri uri;
        private string token;

        public AccessToken(Uri argUri, string argToken)
        {
            uri = argUri;
            token = argToken;
        }

        public AccessToken(string argUri, string argToken)
        {
            uri = new Uri(argUri);
            token = argToken;
        }

        public Uri Uri
        {
            get { return uri; }
        }

        public string Token
        {
            get { return token; }
        }

        public bool HasToken
        {
            get { return false == token.IsNullOrWhiteSpace(); }
        }
    }
}
