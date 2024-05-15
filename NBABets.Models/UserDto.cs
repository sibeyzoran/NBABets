namespace NBABets.Models
{
    public class UserDto
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public List<BetDto>? Bets { get; set; }
        public UserDto()
        {
            Bets = new List<BetDto>();
        }
    }
}
