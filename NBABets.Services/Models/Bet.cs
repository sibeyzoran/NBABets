
namespace NBABets.Services
{
    public class Bet
    {
        public Guid ID { get; set; }
        public double Amount { get; set; }
        public Guid GameID { get; set; }
        public string Result { get; set; }
        public Guid UserID { get; set; }
    }
}
