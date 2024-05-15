﻿
namespace NBABets.Services
{
    public class Game
    {
        public Guid ID { get; set; }
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string Score { get; set; }

    }
}
