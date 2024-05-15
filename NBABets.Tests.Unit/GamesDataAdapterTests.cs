using NBABets.Services;
using Shouldly;
using Xunit;

namespace NBABets.Tests.Unit
{
    public class GamesDataAdapterTests
    {
        [Fact]
        public void SuccessfullyAddGame()
        {
            // Arrange
            IGameAdapter gameAdapter = new GamesAdapter();
            Game game = new Game()
            {
                ID = Guid.NewGuid(),
                Name = "Brooklyn Nets vs Minnesota Timberwolves",
                StartDate = DateTime.Now,
                EndDate = null,
                Status = "Scheduled",
                Score = "0-0"
            };

            // Act
            gameAdapter.Add(game);
            var result = gameAdapter.Get(game.Name);
            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Brooklyn Nets vs Minnesota Timberwolves");
        }

        [Fact]
        public void SuccessfullyGetGame()
        {
            // Arrange
            IGameAdapter gameAdapter = new GamesAdapter();
            string gameToGet = "Brooklyn Nets vs Minnesota Timberwolves";

            // Act
            var result = gameAdapter.Get(gameToGet);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Brooklyn Nets vs Minnesota Timberwolves");
        }

        [Fact]
        public void SuccessfullyGetAllGames()
        {
            // Arrange
            IGameAdapter gameAdapter = new GamesAdapter();

            // Act
            var result = gameAdapter.GetAll();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public void SuccessfullyEditGame()
        {
            // Arrange
            IGameAdapter gameAdapter = new GamesAdapter();
            var game = gameAdapter.Get("Brooklyn Nets vs Minnesota Timberwolves");
            game.Status = "Live";

            // Act
            gameAdapter.Edit(game);
            var result = gameAdapter.Get(game.ID.ToString());

            // Assert
            result?.Status.ShouldBe(game.Status);
        }

        [Fact]
        public void SuccessfullyDeleteGame()
        {
            // Arrange
            IGameAdapter gameAdapter = new GamesAdapter();

            // Act
            gameAdapter.Delete("Brooklyn Nets vs Minnesota Timberwolves");
            var result = gameAdapter.Get("Brooklyn Nets vs Minnesota Timberwolves");

            // Assert
            result.ShouldBeNull();
        }
    }
}
