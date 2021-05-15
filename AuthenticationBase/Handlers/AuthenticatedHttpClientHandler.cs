using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthenticationBase.Handlers
{
    public class AuthenticatedHttpClientHandler : HttpClientHandler
    {
        private const string AuthorizationHeaderKey = "Authorization";
        private const string Bearer = "Bearer";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _tokenSettings;

        public AuthenticatedHttpClientHandler(IHttpContextAccessor httpContextAccessor,
            IOptions<ApiSettings> tokenSettings)
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            CheckCertificateRevocationList = false;

            _httpContextAccessor = httpContextAccessor;
            _tokenSettings = tokenSettings.Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authHeader = request.Headers.Authorization;

            if (authHeader == null)
            {
                AddAuthorizationHeader(request);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private void AddAuthorizationHeader(HttpRequestMessage request)
        {
            if (_httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request.Headers.TryGetValue(AuthorizationHeaderKey, out var value))
            {
                var token = value.ToString().Split(" ").LastOrDefault();
                request.Headers.Authorization = new AuthenticationHeaderValue(Bearer, token);
            }
            else if (!string.IsNullOrWhiteSpace(_tokenSettings.Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(Bearer, _tokenSettings.Token);
            }
        }
    }
}
