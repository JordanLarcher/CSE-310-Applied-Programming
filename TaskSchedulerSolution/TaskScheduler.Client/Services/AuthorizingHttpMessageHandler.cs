using System.Net.Http.Headers;

namespace TaskScheduler.Client.Services
{
    public class AuthorizingHttpMessageHandler : DelegatingHandler
    {
        private readonly CustomAuthenticationStateProvider _authStateProvider;

        public AuthorizingHttpMessageHandler(CustomAuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _authStateProvider.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
