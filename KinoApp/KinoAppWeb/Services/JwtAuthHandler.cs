using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace KinoAppWeb.Services
{
    public class JwtAuthHandler : DelegatingHandler
    {
        private readonly UserSession _session;

        public JwtAuthHandler(UserSession session)
        {
            _session = session;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Hole gültigen AccessToken
            var token = await _session.GetValidAccessTokenAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
