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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Comment> CreateComment(string content, int laureateId, int userId)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));

            var comment = new Comment
            {
                Content = content,
                LaureateId = laureateId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<List<Comment>> GetCommentsByLaureate(int laureateId)
        {
            return await _context.Comments
                .Where(l => l.LaureateId == laureateId)
                .ToListAsync();
        }
    }
}
