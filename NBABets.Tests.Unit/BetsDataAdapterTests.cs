using NBABets.Services;
using Shouldly;
using System.Data.Entity.ModelConfiguration.Configuration;
using Xunit;

namespace NBABets.Tests.Unit
{
    public class BetsDataAdapterTests
    {
        [Fact]
        public void SuccessfullyAddBet()
        {
            // Arrange
            IUserAdapter userAdapter = new UsersAdapter();
            IBetsAdapter betsAdapater = new BetsAdapter(userAdapter);
            var user = userAdapter.Get("Michael Jordan");
            Bet newBet = new Bet()
            {
                Amount = 12.43,
                GameID = Guid.NewGuid(),
                Name = $"{user.Name}: Chicago Bulls to win versus Atlanta Hawks",
                Result = "To be played",
                UserID = user.ID,
                ID = Guid.NewGuid(),
            };

            // Act
            betsAdapater.Add(newBet);

            // Assert
            var result = betsAdapater.Get(newBet.ID.ToString());
            result.ShouldNotBeNull();
            result.ID.ShouldBe(newBet.ID);
            result.UserID.ShouldBe(newBet.UserID);
        }

        [Fact]
        public void SuccessfullyGetBets()
        {
            // Arrange
            IUserAdapter userAdapter = new UsersAdapter();
            IBetsAdapter betsAdapater = new BetsAdapter(userAdapter);
            var user = userAdapter.Get("Jimmy Butler");
            Bet newBet1 = new Bet()
            {
                Amount = 56.78,
                GameID = Guid.NewGuid(),
                Name = $"{user.Name}: Miami Heat to win versus Golden State Warriors",
                Result = "Win",
                UserID = user.ID,
                ID = Guid.NewGuid(),
            };
            Bet newBet2 = new Bet()
            {
                Amount = 23.45,
                GameID = Guid.NewGuid(),
                Name = $"{user.Name}: Miami Heat to win versus Denver Nuggets",
                Result = "Lose",
                UserID = user.ID,
                ID = Guid.NewGuid(),
            };
            
            // Act
            betsAdapater.Add(newBet1);
            betsAdapater.Add(newBet2);

            var result1 = betsAdapater.Get(newBet1.ID.ToString());
            var result2 = betsAdapater.Get(newBet2.Name);

            // Assert
            result1.ShouldNotBeNull();
            result1.ID.ShouldBe(newBet1.ID);

            result2.ShouldNotBeNull();
            result2.Name.ShouldBe(newBet2.Name);
        }

        [Fact]
        public void SuccessfullyGetUsersBets()
        {
            // Arrange
            IUserAdapter userAdapter = new UsersAdapter();
            IBetsAdapter betsAdapater = new BetsAdapter(userAdapter);
            string userName = "Jimmy Butler";

            // Act
            var result = betsAdapater.GetUsersBets(userName);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
        }

        [Fact]
        public void SuccessfullyGetAllBets()
        {
            // Arrange
            IUserAdapter userAdapter = new UsersAdapter();
            IBetsAdapter betsAdapter = new BetsAdapter(userAdapter);

            // Act
            var result = betsAdapter.GetAll();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(3);
        }

        [Fact]
        public void SuccessfullyEditBet()
        {
            // Arrange
            IUserAdapter userAdapter = new UsersAdapter();
            IBetsAdapter betsAdapter = new BetsAdapter(userAdapter);
            var bets = betsAdapter.GetUsersBets("Jimmy Butler");

            bets[0].Amount = 98.76;

            // Act
            betsAdapter.Edit(bets[0]);
            var result = betsAdapter.Get(bets[0].ID.ToString());

            // Assert
            result.Amount.ShouldBe(98.76);
        }

        [Fact]
        public void SuccessfullyDeleteBet()
        {
            // Arrange
            IUserAdapter userAdapter = new UsersAdapter();
            IBetsAdapter betsAdapter = new BetsAdapter(userAdapter);
            string betToDelete = "Jimmy Butler: Miami Heat to win versus Golden State Warriors";
            string user = "Jimmy Butler";
            // Act
            betsAdapter.Delete(betToDelete);
            var result = betsAdapter.Get(betToDelete);
            var userResult = userAdapter.Get(user);

            // Assert
            result.ShouldBeNull();
            userResult.BetsPlaced.Count.ShouldBe(1);
        }
    }
}
