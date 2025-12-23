using System.Net.Http.Json;
using KinoAppShared.DTOs.Vorstellung;

namespace KinoAppWeb.Services
{
    /// <summary>
    /// Client-side API wrapper for showtime endpoints.
    /// </summary>
    public class VorstellungService : IVorstellungService
    {
        private readonly HttpClient _http;

        /// <summary>
        /// Creates a new <see cref="VorstellungService"/>.
        /// </summary>
        public VorstellungService(HttpClient http)
        {
            _http = http;
        }

        /// <inheritdoc />
        public async Task CreateVorstellungAsync(CreateVorstellungDTO vorstellung, CancellationToken ct)
        {
            var resp = await _http.PostAsJsonAsync("api/vorstellung/Erstellen", vorstellung, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(error);
            }
        }

        /// <inheritdoc />
        public async Task<UpdateVorstellungResultDTO?> UpdateVorstellungAsync(UpdateVorstellungDTO vorstellung, CancellationToken ct)
        {
            var response = await _http.PutAsJsonAsync("api/vorstellung/Aktualisieren", vorstellung, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(error);
            }

            return await response.Content.ReadFromJsonAsync<UpdateVorstellungResultDTO>(cancellationToken: ct);
        }

        /// <inheritdoc />
        public async Task<List<VorstellungDTO>> GetVorstellungVonTagAsync(DateTime datum, CancellationToken ct)
        {
            var url = $"api/vorstellung/VonTag?datum={datum:O}";
            return await _http.GetFromJsonAsync<List<VorstellungDTO>>(url, ct) ?? new();
        }

        /// <inheritdoc />
        public async Task<List<VorstellungDTO>> GetVorstellungVonKinosaalAsync(long KinosaalId, CancellationToken ct)
        {
            var url = $"api/vorstellung/VonKinosaal?kinosaalId={KinosaalId}";
            return await _http.GetFromJsonAsync<List<VorstellungDTO>>(url, ct) ?? new();
        }

        /// <inheritdoc />
        public async Task<List<VorstellungDTO>> GetVorstellungVonKinosaalUndTagAsync(DateTime datum, long KinosaalId, CancellationToken ct)
        {
            var url = $"api/vorstellung/VonKinosaalUndTag?datum={datum:O}&kinosaalId={KinosaalId}";
            return await _http.GetFromJsonAsync<List<VorstellungDTO>>(url, ct) ?? new();
        }

        /// <inheritdoc />
        public async Task<List<VorstellungDTO>> GetVorstellungVonFilm(string filmId, CancellationToken ct)
        {
            var url = $"api/vorstellung/VonFilm?filmId={filmId}";
            return await _http.GetFromJsonAsync<List<VorstellungDTO>>(url, ct) ?? new();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteVorstellungAsync(long id, CancellationToken ct)
        {
            var resp = await _http.DeleteAsync($"api/vorstellung/{id}", ct);
            return resp.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<List<VorstellungDTO>> GetAlleVorstellungenAsync(CancellationToken ct = default)
        {
            var url = $"api/vorstellung/Alle";
            var result = await _http.GetFromJsonAsync<List<VorstellungDTO>>(url, ct);
            return result ?? new List<VorstellungDTO>();
        }

        public Task<List<VorstellungDTO>> GetAlleVorstellungenAsync()
        {
            throw new NotImplementedException();
        }
    }
}
