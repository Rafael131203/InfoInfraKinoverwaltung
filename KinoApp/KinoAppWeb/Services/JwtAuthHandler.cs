using System.Net.Http.Headers;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// HTTP message handler that attaches a bearer token to outgoing requests.
    /// </summary>
    /// <remarks>
    /// The token is retrieved from <see cref="UserSession"/> and may be refreshed automatically
    /// before being applied to the request.
    /// </remarks>
    public class JwtAuthHandler : DelegatingHandler
    {
        private readonly UserSession _session;

        /// <summary>
        /// Creates a new <see cref="JwtAuthHandler"/>.
        /// </summary>
        /// <param name="session">User session used to resolve a valid access token.</param>
        public JwtAuthHandler(UserSession session)
        {
            _session = session;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _session.GetValidAccessTokenAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
