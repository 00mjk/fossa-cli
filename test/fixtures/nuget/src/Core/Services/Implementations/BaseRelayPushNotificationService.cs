﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System;
using Newtonsoft.Json.Linq;
using Bit.Core.Utilities;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Bit.Core.Services
{
    public abstract class BaseRelayPushNotificationService
    {
        private dynamic _decodedToken;
        private DateTime? _nextAuthAttempt = null;
        private readonly ILogger<BaseRelayPushNotificationService> _logger;

        public BaseRelayPushNotificationService(
            GlobalSettings globalSettings,
            ILogger<BaseRelayPushNotificationService> logger)
        {
            _logger = logger;
            GlobalSettings = globalSettings;

            PushClient = new HttpClient
            {
                BaseAddress = new Uri(globalSettings.PushRelayBaseUri)
            };
            PushClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            IdentityClient = new HttpClient
            {
                BaseAddress = new Uri(globalSettings.Installation.IdentityUri)
            };
            IdentityClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected HttpClient PushClient { get; private set; }
        protected HttpClient IdentityClient { get; private set; }
        protected GlobalSettings GlobalSettings { get; private set; }
        protected string AccessToken { get; private set; }

        protected async Task<bool> HandleTokenStateAsync()
        {
            if(_nextAuthAttempt.HasValue && DateTime.UtcNow > _nextAuthAttempt.Value)
            {
                return false;
            }
            _nextAuthAttempt = null;

            if(!string.IsNullOrWhiteSpace(AccessToken) && !TokenExpired())
            {
                return true;
            }

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(string.Concat(IdentityClient.BaseAddress, "/connect/token")),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "scope", "api.push" },
                    { "client_id", $"installation.{GlobalSettings.Installation.Id}" },
                    { "client_secret", $"{GlobalSettings.Installation.Key}" }
                })
            };

            HttpResponseMessage response = null;
            try
            {
                response = await IdentityClient.SendAsync(requestMessage);
            }
            catch(Exception e)
            {
                _logger.LogError(12339, e, "Unable to auth for push.");
            }

            if(response == null)
            {
                return false;
            }

            if(!response.IsSuccessStatusCode)
            {
                if(response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _nextAuthAttempt = DateTime.UtcNow.AddDays(1);
                }

                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic tokenResponse = JsonConvert.DeserializeObject(responseContent);
            AccessToken = (string)tokenResponse.access_token;
            return true;
        }

        protected class TokenHttpRequestMessage : HttpRequestMessage
        {
            public TokenHttpRequestMessage(string token)
            {
                Headers.Add("Authorization", $"Bearer {token}");
            }

            public TokenHttpRequestMessage(object requestObject, string token)
                : this(token)
            {
                var stringContent = JsonConvert.SerializeObject(requestObject);
                Content = new StringContent(stringContent, Encoding.UTF8, "application/json");
            }
        }

        protected bool TokenExpired()
        {
            var decoded = DecodeToken();
            var exp = decoded?["exp"];
            if(exp == null)
            {
                throw new InvalidOperationException("No exp in token.");
            }

            var expiration = CoreHelpers.FromEpocSeconds(exp.Value<long>());
            return DateTime.UtcNow < expiration;
        }

        protected JObject DecodeToken()
        {
            if(_decodedToken != null)
            {
                return _decodedToken;
            }

            if(AccessToken == null)
            {
                throw new InvalidOperationException($"{nameof(AccessToken)} not found.");
            }

            var parts = AccessToken.Split('.');
            if(parts.Length != 3)
            {
                throw new InvalidOperationException($"{nameof(AccessToken)} must have 3 parts");
            }

            var decodedBytes = CoreHelpers.Base64UrlDecode(parts[1]);
            if(decodedBytes == null || decodedBytes.Length < 1)
            {
                throw new InvalidOperationException($"{nameof(AccessToken)} must have 3 parts");
            }

            _decodedToken = JObject.Parse(Encoding.UTF8.GetString(decodedBytes, 0, decodedBytes.Length));
            return _decodedToken;
        }
    }
}
