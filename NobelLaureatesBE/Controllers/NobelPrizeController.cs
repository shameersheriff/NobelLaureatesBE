using Microsoft.AspNetCore.Mvc;
using NobelLaureatesBE.BusinessLogic.Interfaces;

namespace NobelLaureatesBE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NobelPrizeController : ControllerBase
    {
        private readonly INobelPrizeService _nobelPrizeService;

        public NobelPrizeController(INobelPrizeService nobelPrizeService)
        {
            _nobelPrizeService = nobelPrizeService ?? throw new ArgumentNullException(nameof(nobelPrizeService));
        }

        [HttpPost("laureates")]
        public async Task<IActionResult> GetNobelLaureates([FromBody] NobelLaureateFilter model)
        {
            if (model == null)
                return BadRequest("Invalid client request");

            if (model.Limit <= 0)
                model.Limit = 40;

            try
            {
                var result = await _nobelPrizeService.GetNobelLaureatesAsync(model.OffSet, model.Limit, model.Gender, model.BirthDate, model.DeathDate, model.NobelPrizeCategory);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while fetching Nobel laureates");
            }
        }

        [HttpGet("laureate/{id}")]
        public async Task<IActionResult> GetNobelLaureate(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid laureate ID");

            try
            {
                var result = await _nobelPrizeService.GetNobelLaureateAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching the Nobel laureate with ID {id}");
            }
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
