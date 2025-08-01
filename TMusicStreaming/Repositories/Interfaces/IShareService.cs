namespace TMusicStreaming.Repositories.Interfaces
{
    public interface IShareService
    {
        string CreateShareLink(int songId, int expireInMinutes = 60);
        (int? songId, bool isValid, DateTime? expireAt, DateTime? createdAt) ValidateShareLink(string encodedData);
    }
}
