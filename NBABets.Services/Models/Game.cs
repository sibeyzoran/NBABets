
namespace NBABets.Services
{
    public class Game
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public bool IsOpen { get; set; }
    }
}
