namespace FinService.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
        public decimal Balance { get; set; }

        public List<WalletTransaction> Transactions { get; set; }
    }
}
