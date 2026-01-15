using System.Net.Http.Json;
using fs_2025_assessment_1_82706_blazor.Models;

namespace fs_2025_assessment_1_82706_blazor.Services
{
    // Very small client to call the Dublin Bikes API V2
    public class StationsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        public string LastErrorMessage { get; private set; } = string.Empty;

        public StationsApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["DublinBikesApi:BaseUrl"] ?? "http://localhost:5044";
            if (_baseUrl.EndsWith("/"))
            {
                _baseUrl = _baseUrl.TrimEnd('/');
            }
        }

        // Load stations with filters
        public async Task<List<StationDto>> GetStationsAsync(
            string status,
            int? minBikes,
            string searchText,
            string sort,
            string dir,
            int page,
            int pageSize)
        {
            LastErrorMessage = string.Empty;

            try
            {
                // Si no hay status o viene "All" -> por defecto OPEN
                string statusValue = string.IsNullOrWhiteSpace(status) || status == "All"
                    ? "OPEN"
                    : status;

                // Si no hay minBikes o es <= 0 -> por defecto 1
                int minBikesEffective = (!minBikes.HasValue || minBikes.Value <= 0)
                    ? 1
                    : minBikes.Value;

                string minBikesValue = minBikesEffective.ToString();

                string searchValue = searchText ?? string.Empty;

                string url = _baseUrl + "/api/v2/stations" +
                             "?status=" + statusValue +
                             "&minBikes=" + minBikesValue +
                             //"&q=" + searchValue +
                             "&sort=" + (string.IsNullOrWhiteSpace(sort) ? "name" : sort) +
                             "&dir=" + (string.IsNullOrWhiteSpace(dir) ? "asc" : dir) +
                             "&page=" + (page <= 0 ? 1 : page).ToString() +
                             "&pageSize=" + (pageSize <= 0 ? 10 : pageSize).ToString();

                var result = await _httpClient.GetFromJsonAsync<List<StationDto>>(url);
                return result ?? new List<StationDto>();
            }
            catch (HttpRequestException httpEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(httpEx.Message);
                return new List<StationDto>();
            }
            catch (TaskCanceledException cancelEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(cancelEx.Message);
                return new List<StationDto>();
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return new List<StationDto>();
            }
        }


        // Load one station
        public async Task<StationDto?> GetStationAsync(int number)
        {
            LastErrorMessage = string.Empty;
            try
            {
                string url = _baseUrl + "/api/v2/stations/" + number;
                return await _httpClient.GetFromJsonAsync<StationDto>(url);
            }
            catch (HttpRequestException httpEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(httpEx.Message);
                return null;
            }
            catch (TaskCanceledException cancelEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(cancelEx.Message);
                return null;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return null;
            }
        }

        // Load summary info
        public async Task<StationsSummaryDto?> GetSummaryAsync()
        {
            LastErrorMessage = string.Empty;
            try
            {
                string url = _baseUrl + "/api/v2/stations/summary";
                return await _httpClient.GetFromJsonAsync<StationsSummaryDto>(url);
            }
            catch (HttpRequestException httpEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(httpEx.Message);
                return null;
            }
            catch (TaskCanceledException cancelEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(cancelEx.Message);
                return null;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return null;
            }
        }

        // Create a new station
        public async Task<bool> CreateStationAsync(StationDto station)
        {
            LastErrorMessage = string.Empty;
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_baseUrl + "/api/v2/stations", station);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                LastErrorMessage = response.ReasonPhrase ?? "Could not create station";
                return false;
            }
            catch (HttpRequestException httpEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(httpEx.Message);
                return false;
            }
            catch (TaskCanceledException cancelEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(cancelEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return false;
            }
        }

        // Update a station
        public async Task<bool> UpdateStationAsync(int number, StationDto station)
        {
            LastErrorMessage = string.Empty;
            try
            {
                var response = await _httpClient.PutAsJsonAsync(_baseUrl + "/api/v2/stations/" + number, station);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                LastErrorMessage = response.ReasonPhrase ?? "Could not update station";
                return false;
            }
            catch (HttpRequestException httpEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(httpEx.Message);
                return false;
            }
            catch (TaskCanceledException cancelEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(cancelEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return false;
            }
        }

        // Delete a station
        public async Task<bool> DeleteStationAsync(int number)
        {
            LastErrorMessage = string.Empty;
            try
            {
                var response = await _httpClient.DeleteAsync(_baseUrl + "/api/v2/stations/" + number);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                LastErrorMessage = response.ReasonPhrase ?? "Could not delete station";
                return false;
            }
            catch (HttpRequestException httpEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(httpEx.Message);
                return false;
            }
            catch (TaskCanceledException cancelEx)
            {
                LastErrorMessage = BuildConnectionErrorMessage(cancelEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                LastErrorMessage = ex.Message;
                return false;
            }
        }

        private string BuildConnectionErrorMessage(string technicalMessage)
        {
            return $"No se ha podido conectar con la API DublinBikes. Revisa que la API V2 esté levantada en {_baseUrl}. Detalle: {technicalMessage}";
        }
    }
}
