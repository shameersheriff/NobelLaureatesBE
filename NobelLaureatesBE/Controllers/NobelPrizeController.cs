using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NobelLaureatesBE.BusinessLogic.Services;

namespace NobelLaureatesBE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NobelPrizeController : ControllerBase
    {
        private readonly NobelPrizeService _nobelPrizeService;

        public NobelPrizeController(NobelPrizeService nobelPrizeService)
        {
            _nobelPrizeService = nobelPrizeService;
        }

        [HttpPost("laureates")]
        public async Task<IActionResult> GetNobelLaureates([FromBody] NobelLaureateFilter model)
        {
            var result = await _nobelPrizeService.GetNobelLaureatesAsync(model.OffSet, model.Limit, model.Gender, model.BirthDate, model.DeathDate, model.NobelPrizeCategory);
            return Ok(result);
        }

        [HttpGet("laureate/{id}")]
        public async Task<IActionResult> GetNobelLaureate(int id)
        {
            var result = await _nobelPrizeService.GetNobelLaureateAsync(id);
            return Ok(result);
        }

        public class NobelLaureateFilter
        {
            public int Limit { get; set; }
            public int OffSet { get; set; }
            public string Gender { get; set; }
            public string BirthDate { get; set; }
            public string DeathDate { get; set; }
            public string NobelPrizeCategory { get; set; }
        }
    }
}
