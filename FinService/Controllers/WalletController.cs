using FinService.DTOs;
using FinService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Проверить существует ли электронный кошелёк.
        [HttpGet("exists/{walletId}")]
        public async Task<ActionResult<Wallet>> CheckWalletExists(int walletId)
        {
            var wallet = await _context.Wallets
                .Include(x => x.User)
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(w => w.Id == walletId);
            if (wallet == null)
                return NotFound();

            return Ok(wallet);
        }

        // Пополнение электронного кошелька.
        [HttpPost("recharge")]
        public async Task<IActionResult> RechargeWallet([FromBody] WalletTransactionDto transaction)
        {
            bool isCurrentUserAuthenticated = HttpContext.User.Identity.IsAuthenticated;

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Id == transaction.WalletId);
            if (wallet == null)
                return NotFound();

            if (!isCurrentUserAuthenticated && wallet.Balance + transaction.Amount > 100000)
                return BadRequest("Максимальный баланс для идентифицированных счетов превышен. Пополнение невозможно.");

            if (isCurrentUserAuthenticated && wallet.Balance + transaction.Amount > 10000)
                return BadRequest("Максимальный баланс для неидентифицированных счетов превышен. Пополнение невозможно.");

            wallet.Balance += transaction.Amount;

            await _context.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletId = transaction.WalletId,
                Amount= transaction.Amount,
                TransactionDate= transaction.TransactionDate,
            });

            await _context.SaveChangesAsync();

            return Ok($"Баланс успешно пополнен на {transaction.Amount} для кошелька номер {wallet.Id}");
        }

        // Получить общее количество и суммы операций пополнения за текущий месяц.
        [HttpGet("transactions/{walletId}")]
        public async Task<IActionResult> GetTransactionsForCurrentMonth(int walletId)
        {
            var today = DateTime.Today;
            var transactions = await _context.WalletTransactions
                .Where(t => t.WalletId == walletId 
                && t.TransactionDate.Month == today.Month 
                && t.TransactionDate.Year == today.Year
                && t.Amount > 0)
                .ToListAsync();

            var totalCount = transactions.Count;
            var totalAmount = transactions.Sum(t => t.Amount);

            return Ok(new { WalletId = walletId, TotalCount = totalCount, TotalAmount = totalAmount });
        }

        // Получить баланс электронного кошелька.
        [HttpGet("balance/{walletId}")]
        public async Task<IActionResult> GetWalletBalance(int walletId)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Id == walletId);
            if (wallet == null)
                return NotFound();

            return Ok($"Баланс кошелька номер {walletId} состовляет {wallet.Balance}");
        }
    }
}
