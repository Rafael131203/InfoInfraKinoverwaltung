using System.Net.Http.Json;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppWeb.Services
{
    public class KinosaalService : IKinosaalService
    {
        private readonly HttpClient _http;

        public KinosaalService(HttpClient http)
        {
            _http = http;
        }

        public async Task<long> CreateAsync(CreateKinosaalDTO dto, int AnzahlSitzreihen, int GrößeSitzreihen, CancellationToken ct = default)
        {
            var url = $"api/kinosaal/Erstellen?AnzahlSitzreihen={AnzahlSitzreihen}&GrößeSitzreihen={GrößeSitzreihen}";

            var resp = await _http.PostAsJsonAsync(url, dto, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(error);
            }

            var result = await resp.Content.ReadFromJsonAsync<Dictionary<string, long>>(cancellationToken: ct);
            return result?["id"] ?? 0;
        }

        public async Task<KinosaalDTO?> GetKinosaalAsync(long id, long? vorstellungId, CancellationToken ct)
        {
            string url = vorstellungId.HasValue
                ? $"api/kinosaal?id={id}&vorstellungId={vorstellungId.Value}"
                : $"api/kinosaal?id={id}";

            return await _http.GetFromJsonAsync<KinosaalDTO>(url, ct);
        }


        public async Task<SitzreiheEntity> ChangeSitzreiheKategorieAsync(ChangeKategorieSitzreiheDTO dto, CancellationToken ct)
        {
            var resp = await _http.PostAsJsonAsync("api/kinosaal/SitzreiheKategorieÄndern", dto, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException(error);
            }

            var sitzreihe = await resp.Content.ReadFromJsonAsync<SitzreiheEntity>(cancellationToken: ct);
            return sitzreihe!;
        }

        public async Task<KinosaalEntity?> DeleteAsync(long id, CancellationToken ct = default)
        {
            var url = $"api/kinosaal?Id={id}";
            var resp = await _http.DeleteAsync(url, ct);

            if (!resp.IsSuccessStatusCode)
                return null;

            return new KinosaalEntity { Id = id };
        }
    }
}

