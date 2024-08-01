using Microsoft.EntityFrameworkCore;
using NobelLaureatesBE.BusinessLogic.Interfaces;
using NobelLaureatesBE.Repositories.Models;

namespace NobelLaureatesBE.BusinessLogic.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Comment> CreateComment(string content, int laureateId, int userId)
        {
            var comment = new Comment
            {
                Content = content,
                LaureateId = laureateId,
                CreatedBy = userId,
                CreatedAt = new DateTime(),
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<List<Comment>> GetCommentsByLaureate(int laureateId)
        {
            var comments = await _context.Comments.Where(l => l.LaureateId == laureateId).ToListAsync();
            return comments;
        }

    }
}
