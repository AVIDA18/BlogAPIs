namespace BlogApi.Services
{
    public interface IApiLogger
    {
        Task LogAsync(string api, string payload, string response, int? userId);
    }
}