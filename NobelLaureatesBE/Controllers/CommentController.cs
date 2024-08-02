using Microsoft.AspNetCore.Mvc;
using NobelLaureatesBE.BusinessLogic.Interfaces;

namespace NobelLaureatesBE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CommentDto model)
        {
            if (model == null)
                return BadRequest("Invalid client request");

            if (string.IsNullOrWhiteSpace(model.Comment))
                return BadRequest("Comment cannot be empty");

            try
            {
                var result = await _commentService.CreateComment(model.Comment, model.LaureateId, model.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while creating the comment");
            }
        }

        [HttpGet("laureate/{id}")]
        public async Task<IActionResult> GetCommentsByLaureate(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid laureate ID");

            try
            {
                var result = await _commentService.GetCommentsByLaureate(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while fetching comments");
            }
        }

        public class CommentDto
        {
            public string Comment { get; set; }
            public int LaureateId { get; set; }
            public int UserId { get; set; }
        }
    }
}
