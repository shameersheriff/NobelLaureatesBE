using NobelLaureatesBE.Repositories.Models;

namespace NobelLaureatesBE.BusinessLogic.Interfaces
{
    public interface ICommentService
    {
        Task<Comment> CreateComment(string comment, int laureateId, int userId);
        Task<List<Comment>> GetCommentsByLaureate(int laureateId);
    }
}
