using System.Net.Http.Json;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Vorstellung;

namespace KinoAppWeb.Services
{
    public class VorstellungService : IVorstellungService
    {
        private readonly HttpClient _http;

        public VorstellungService(HttpClient http)
        {
            _http = http;
        }

        public async Task CreateVorstellungAsync(CreateVorstellungDTO vorstellung, CancellationToken ct)
        {
            var resp = await _http.PostAsJsonAsync("api/vorstellung/Erstellen", vorstellung, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(error);
            }
        }

        public async Task<UpdateVorstellungResultDTO?> UpdateVorstellungAsync(UpdateVorstellungDTO vorstellung, CancellationToken ct)
        {
            var response = await _http.PutAsJsonAsync("api/vorstellung/Aktualisieren", vorstellung, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(error);
            }

            var result = await response.Content.ReadFromJsonAsync<UpdateVorstellungResultDTO>(cancellationToken: ct);
            return result ?? new UpdateVorstellungResultDTO();
        }

        public async Task<List<VorstellungDTO>> GetVorstellungVonTagAsync(DateTime datum, CancellationToken ct)
        {
            var url = $"api/vorstellung/VonTag?datum={datum:O}";
            var result = await _http.GetFromJsonAsync<List<VorstellungDTO>>(url, ct);
            return result ?? new List<VorstellungDTO>();
        }

        public async Task<List<VorstellungDTO>> GetVorstellungVonKinosaalAsync(long KinosaalId, CancellationToken ct)
        {
            var url = $"api/vorstellung/VonKinosaal?kinosaalId={KinosaalId}";
            var result = await _http.GetFromJsonAsync<List<VorstellungDTO>>(url, ct);
            return result ?? new List<VorstellungDTO>();
        }

        public async Task<List<VorstellungDTO>> GetVorstellungVonKinosaalUndTagAsync(DateTime datum, long KinosaalId, CancellationToken ct)
        {
            var url = $"api/vorstellung/VonKinosaalUndTag?datum={datum:O}&kinosaalId={KinosaalId}";
            var result = await _http.GetFromJsonAsync<List<VorstellungDTO>>(url, ct);
            return result ?? new List<VorstellungDTO>();
        }

        public async Task<List<VorstellungDTO>> GetVorstellungVonFilm(string filmId, CancellationToken ct)
        {
            var url = $"api/vorstellung/VonFilm?filmId={filmId}";
            var result = await _http.GetFromJsonAsync<List<VorstellungDTO>>(url, ct);
            return result ?? new List<VorstellungDTO>();
        }


        public async Task<bool> DeleteVorstellungAsync(long id, CancellationToken ct)
        {
            var resp = await _http.DeleteAsync($"api/vorstellung/{id}", ct);
            return resp.IsSuccessStatusCode;
        }
    }
}
