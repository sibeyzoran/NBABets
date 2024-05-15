using NBABets.Services;
using NBABets.Models;

namespace NBABets.API.Mappers
{
    public class UserMapper : IMapper<UserMapRequest, UserDto>
    {
        public UserDto Map(UserMapRequest item)
        {
            UserDto userDto = new UserDto()
            {
                ID = item.user.ID,
                Name = item.user.Name,
            };

            if (item.user.BetsPlaced.Any())
            {
                userDto.Bets = LookupBets(item.user.BetsPlaced);

            }

            return userDto;
        }

        private List<BetDto> LookupBets(List<Guid> bets)
        {
            var result = new List<BetDto>();
            IUserAdapter userAdapter = new UsersAdapter();
            IBetsAdapter betsAdapter = new BetsAdapter(userAdapter);
            BetMapper betMapper = new BetMapper();

            foreach (Guid bet in bets)
            {
                var retrievedBet = betsAdapter.Get(bet.ToString());
                BetMapRequest betMapRequest = new BetMapRequest()
                {
                    bet = retrievedBet
                };
                result.Add(betMapper.Map(betMapRequest));
            }

            return result;
        }
    }
}
