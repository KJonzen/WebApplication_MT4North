﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace WebApplication_MT4North.Resources
{
    public class UserLoginResult
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("role")]
        public List<string> Role { get; set; }

        //[JsonPropertyName("originalUserName")]
        //public string OriginalUserName { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        //[JsonPropertyName("refreshToken")]
        //public string RefreshToken { get; set; }
    }
}
