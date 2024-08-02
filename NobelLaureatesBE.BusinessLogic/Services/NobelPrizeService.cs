using Microsoft.IdentityModel.Tokens;
using NobelLaureatesBE.BusinessLogic.Interfaces;

namespace NobelLaureatesBE.BusinessLogic.Services
{
    public class NobelPrizeService : INobelPrizeService
    {
        private readonly HttpClient _httpClient;

        public NobelPrizeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetNobelLaureatesAsync(int offset, int limit, string gender, string birthDate, string deathDate, string nobelPrizeCategory)
        {
            string url = "http://api.nobelprize.org/2.0/laureates?";


            if (limit > 0)
            {
                url += "limit=" + limit;
            }
            else
            {
                url += "limit=40";
            }
            if (offset > 0)
            {
                url += "&offset=" + offset;
            }
            if (gender == "male" || gender == "female" || gender == "other")
            {
                url += "&gender=" + gender;
            }
            if (!birthDate.IsNullOrEmpty())
            {
                url += "&birthDate=" + birthDate;
            }
            if (!deathDate.IsNullOrEmpty())
            {
                url += "&deathDate=" + deathDate;
            }
            if (nobelPrizeCategory == "che" || nobelPrizeCategory == "eco" || nobelPrizeCategory == "lit")
            {
                url += "&nobelPrizeCategory=" + nobelPrizeCategory;
            }
            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetNobelLaureateAsync(int id)
        {
            string url = "http://api.nobelprize.org/2.0/laureate/" + id;

            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

    }
}
