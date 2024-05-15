using NBABets.Models;
using NBABets.Services;

namespace NBABets.API
{
    public class BetMapper : IMapper<BetMapRequest, BetDto>
    {
        public BetDto Map(BetMapRequest item)
        {
            BetDto dto = new BetDto()
            {
                ID = item.bet.ID,
                Name = item.bet.Name,
                Amount = item.bet.Amount,
                Game = LookUpGame(item.bet.GameID),
                UserID = item.bet.UserID,
                Result = item.bet.Result
            };
            return dto;
        }

        private GameDto LookUpGame(Guid game)
        {
            IGameAdapter gameAdapter = new GamesAdapter();
            var retrievedGame = gameAdapter.Get(game.ToString());
            GameMapper gameMapper = new GameMapper();
            GameMapRequest gameMapRequest = new GameMapRequest()
            {
                game = retrievedGame,
            };

            return gameMapper.Map(gameMapRequest);
        }
    }
}
