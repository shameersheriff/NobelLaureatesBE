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
            _commentService = commentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CommentDto model)
        {
            var result = await _commentService.CreateComment(model.Comment, model.LaureateId, model.UserId);
            return Ok(result);
        }

        [HttpGet("laureate/{id}")]
        public async Task<IActionResult> GetCommentsByLaureate(int id)
        {
            var result = await _commentService.GetCommentsByLaureate(id);
            return Ok(result);
        }


        public class CommentDto
        {
            public string Comment { get; set; }
            public int LaureateId { get; set; }
            public int UserId { get; set; }
        }
    }
}
