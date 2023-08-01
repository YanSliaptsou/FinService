namespace FinService.Models
{
    public class WalletTransaction
    {
        public int Id { get; set; }
        public Wallet Wallet { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
