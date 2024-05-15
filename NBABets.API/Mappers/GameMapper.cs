using NBABets.Models;

namespace NBABets.API
{
    public class GameMapper : IMapper<GameMapRequest, GameDto>
    {
        public GameDto Map(GameMapRequest item)
        {
            GameDto dto = new GameDto()
            {
                ID = item.game.ID,
                Name = item.game.Name,
                StartDate = item.game.StartDate,
                EndDate = item.game.EndDate,
                Status = item.game.Status,
                Score = item.game.Score,
            };

            return dto;
        }
    }
}
