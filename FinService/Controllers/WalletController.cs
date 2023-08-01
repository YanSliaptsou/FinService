using FinService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WalletController(AppDbContext context)
        {
            _context = context;
        }

        // Проверить существует ли аккаунт электронного кошелька.
        [HttpGet("exists/{walletId}")]
        public IActionResult CheckWalletExists(int walletId)
        {
            var wallet = _context.Wallets.FirstOrDefault(w => w.Id == walletId);
            if (wallet == null)
                return NotFound();

            return Ok();
        }

        // Пополнение электронного кошелька.
        [HttpPost("recharge")]
        public IActionResult RechargeWallet([FromBody] WalletTransaction transaction)
        {
            // Проверка баланса и максимального баланса
            var wallet = _context.Wallets.FirstOrDefault(w => w.Id == transaction.WalletId);
            if (wallet == null)
                return NotFound();

            if (HttpContext.User.Identity.IsAuthenticated && wallet.Balance + transaction.Amount > 100000)
                return BadRequest("Максимальный баланс для идентифицированных счетов превышен.");

            if (!HttpContext.User.Identity.IsAuthenticated && wallet.Balance + transaction.Amount > 10000)
                return BadRequest("Максимальный баланс для неидентифицированных счетов превышен.");

            wallet.Balance += transaction.Amount;
            _context.WalletTransactions.Add(transaction);
            _context.SaveChanges();

            return Ok(wallet.Balance);
        }

        // Получить общее количество и суммы операций пополнения за текущий месяц.
        [HttpGet("transactions/{walletId}")]
        public IActionResult GetTransactionsForCurrentMonth(int walletId)
        {
            var today = DateTime.Today;
            var transactions = _context.WalletTransactions
                .Where(t => t.WalletId == walletId && t.TransactionDate.Month == today.Month && t.TransactionDate.Year == today.Year)
                .ToList();

            var totalCount = transactions.Count;
            var totalAmount = transactions.Sum(t => t.Amount);

            return Ok(new { TotalCount = totalCount, TotalAmount = totalAmount });
        }

        // Получить баланс электронного кошелька.
        [HttpGet("balance/{walletId}")]
        public IActionResult GetWalletBalance(int walletId)
        {
            var wallet = _context.Wallets.FirstOrDefault(w => w.Id == walletId);
            if (wallet == null)
                return NotFound();

            return Ok(wallet.Balance);
        }
    }
}
