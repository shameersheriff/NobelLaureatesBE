using Microsoft.EntityFrameworkCore;
using NobelLaureatesBE.BusinessLogic.Services;
using NobelLaureatesBE.Repositories.Models;

namespace NobelLaureatesBE.Tests
{
    [TestFixture]
    public class CommentServiceTests
    {
        private CommentService _commentService;
        private DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private ApplicationDbContext _context;

        [SetUp]
        public void Setup()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new ApplicationDbContext(_dbContextOptions);
            _commentService = new CommentService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task CreateComment_CreatesCommentSuccessfully()
        {
            // Arrange
            var content = "This is a test comment";
            var laureateId = 1;
            var userId = 1;

            // Act
            var comment = await _commentService.CreateComment(content, laureateId, userId);

            // Assert
            Assert.IsNotNull(comment);
            Assert.AreEqual(content, comment.Content);
            Assert.AreEqual(laureateId, comment.LaureateId);
            Assert.AreEqual(userId, comment.CreatedBy);

            var createdComment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == comment.Id);
            Assert.IsNotNull(createdComment);
            Assert.AreEqual(content, createdComment.Content);
            Assert.AreEqual(laureateId, createdComment.LaureateId);
            Assert.AreEqual(userId, createdComment.CreatedBy);
        }

        [Test]
        public async Task GetCommentsByLaureate_ReturnsCorrectComments()
        {
            // Arrange
            var laureateId = 1;
            var comments = new List<Comment>
            {
                new Comment { Content = "Comment 1", LaureateId = laureateId, CreatedBy = 1, CreatedAt = DateTime.UtcNow },
                new Comment { Content = "Comment 2", LaureateId = laureateId, CreatedBy = 2, CreatedAt = DateTime.UtcNow },
                new Comment { Content = "Comment 3", LaureateId = 2, CreatedBy = 1, CreatedAt = DateTime.UtcNow }
            };

            _context.Comments.AddRange(comments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _commentService.GetCommentsByLaureate(laureateId);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(c => c.Content == "Comment 1"));
            Assert.IsTrue(result.Any(c => c.Content == "Comment 2"));
            Assert.IsFalse(result.Any(c => c.Content == "Comment 3"));
        }
    }
}
