using BlogApi.Data;
using BlogApi.Models;

namespace BlogApi.Services
{
    public class ApiLogger : IApiLogger
    {
        private readonly BloggingContext _context;

        public ApiLogger(BloggingContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string api, string payload, string response, int? userId)
        {
            var log = new ApiLog
            {
                Api = api,
                Payload = payload,
                Response = response,
                UserId = userId
            };

            _context.ApiLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}