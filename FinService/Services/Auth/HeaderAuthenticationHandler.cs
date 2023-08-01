using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace FinService.Services.Auth
{
    public class HeaderAuthenticationHandler : AuthenticationHandler<HeaderAuthenticationOptions>
    {
        public HeaderAuthenticationHandler(IOptionsMonitor<HeaderAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Получаем значения заголовков X-UserId и X-Digest
            var xUserId = Request.Headers["X-UserId"].FirstOrDefault();
            var xDigest = Request.Headers["X-Digest"].FirstOrDefault();

            if (string.IsNullOrEmpty(xUserId))
            {
                return await Task.FromResult(AuthenticateResult.Fail("Authentication failed"));
            }

            // Получаем данные из тела запроса
            string data;
            using (var reader = new StreamReader(Request.Body))
            {
                data = await reader.ReadToEndAsync();
            }

            // Проверяем хэш-сумму
            if (!IsValidDigest(data, xDigest))
            {
                return await Task.FromResult(AuthenticateResult.Fail("Authentication failed"));
            }

            // Создаем идентификационную информацию о клиенте
            var claims = new[] { new Claim("X-UserId", xUserId) };
            var identity = new ClaimsIdentity(claims, "Header");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // Метод для проверки хэш-суммы
        private bool IsValidDigest(string data, string digest)
        {
            var secretKey = "my_own_secret_key";

            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                var hashString = string.Join("", hash.Select(b => b.ToString("x2")));
                return digest == hashString;
            }
        }
    }
}
