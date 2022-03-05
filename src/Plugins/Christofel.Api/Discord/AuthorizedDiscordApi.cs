//
//   AuthorizedDiscordApi.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth2;
using RestSharp.Serializers.NewtonsoftJson;

namespace Christofel.Api.Discord
{
    /// <summary>
    /// Discord API with assigned accessToken.
    /// </summary>
    public class AuthorizedDiscordApi
    {
        private readonly string _accessToken;
        private readonly RestClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizedDiscordApi"/> class.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="options">The options of the discord api.</param>
        internal AuthorizedDiscordApi(string accessToken, DiscordApiOptions options)
        {
            _client = new RestClient(options.BaseUrl)
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(accessToken, "Bearer"),
            };
            _client.UseNewtonsoftJson();

            _accessToken = accessToken;
        }

        private void AddAuthHeaders(HttpWebRequest request)
        {
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
        }

        /// <summary>
        /// Calls /users/@me to retrieve information about the current user.
        /// </summary>
        /// <returns>Information about current user.</returns>
        /// <exception cref="Exception">Thrown if the request was not successful.</exception>
        public async Task<DiscordUser> GetMe()
        {
            var request = new RestRequest("/users/@me", Method.Get);
            var response = await _client.ExecuteAsync<DiscordUser>(request);
            if (!response.IsSuccessful)
            {
                throw new Exception
                (
                    $"Discord request for user wasn't successful {response.StatusCode} {response.ErrorMessage} {response.Content}",
                    response.ErrorException
                );
            }

            return response.Data ?? throw new Exception("Could not parse /users/@me.");
        }
    }
}