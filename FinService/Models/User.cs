﻿namespace FinService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsIdentified { get; set; }
        public List<Wallet> Wallets { get; set; }
    }
}
