﻿using System.Text.Json.Serialization;
//using WebApplication_MT4North.Services;

namespace WebApplication_MT4North.Resources
{
    public class RefreshTokenRequest
    {
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
