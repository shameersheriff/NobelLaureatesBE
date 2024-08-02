using System.Text;
using NobelLaureatesBE.BusinessLogic.Interfaces;

namespace NobelLaureatesBE.BusinessLogic.Services
{
    public class NobelPrizeService : INobelPrizeService
    {
        private readonly HttpClient _httpClient;

        public NobelPrizeService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetNobelLaureatesAsync(int offset, int limit, string gender, string birthDate, string deathDate, string nobelPrizeCategory)
        {
            if (limit <= 0) limit = 40;

            var urlBuilder = new StringBuilder("http://api.nobelprize.org/2.0/laureates?")
                .Append("limit=").Append(limit);

            if (offset > 0)
            {
                urlBuilder.Append("&offset=").Append(offset);
            }
            if (!string.IsNullOrEmpty(gender) && (gender == "male" || gender == "female" || gender == "other"))
            {
                urlBuilder.Append("&gender=").Append(gender);
            }
            if (!string.IsNullOrEmpty(birthDate))
            {
                urlBuilder.Append("&birthDate=").Append(birthDate);
            }
            if (!string.IsNullOrEmpty(deathDate))
            {
                urlBuilder.Append("&deathDate=").Append(deathDate);
            }
            if (!string.IsNullOrEmpty(nobelPrizeCategory) && (nobelPrizeCategory == "che" || nobelPrizeCategory == "eco" || nobelPrizeCategory == "lit"))
            {
                urlBuilder.Append("&nobelPrizeCategory=").Append(nobelPrizeCategory);
            }

            var url = urlBuilder.ToString();

            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                throw new Exception("An error occurred while fetching Nobel laureates.", e);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<string> GetNobelLaureateAsync(int id)
        {
            var url = $"http://api.nobelprize.org/2.0/laureate/{id}";

            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                throw new Exception($"An error occurred while fetching the Nobel laureate with ID {id}.", e);
            }
            finally
            {
                response?.Dispose();
            }
        }
    }
}
