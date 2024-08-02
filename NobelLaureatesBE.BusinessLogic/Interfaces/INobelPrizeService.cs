namespace NobelLaureatesBE.BusinessLogic.Interfaces
{
    public interface INobelPrizeService
    {
        Task<string> GetNobelLaureatesAsync(int offset, int limit, string gender, string birthDate, string deathDate, string nobelPrizeCategory);
        Task<string> GetNobelLaureateAsync(int id);
    }
}
