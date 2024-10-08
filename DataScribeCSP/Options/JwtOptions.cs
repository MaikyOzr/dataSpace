﻿namespace DataScribeCSP.Options
{
    public class JwtOptions
    {
        public const string Jwt = nameof(Jwt);
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
