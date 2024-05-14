
namespace NBABets.Services
{
    public class User
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public List<Guid> BetsPlaced { get; set; }
    }
}
