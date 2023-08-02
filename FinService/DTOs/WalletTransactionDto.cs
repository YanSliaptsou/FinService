using FinService.Models;

namespace FinService.DTOs
{
    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
