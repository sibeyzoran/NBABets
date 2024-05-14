using NBABets.Services;
using Xunit;
using Shouldly;
using System.Data.SQLite;
using System.Data.Entity.ModelConfiguration.Configuration;

namespace NBABets.Tests.Unit
{
    public class UsersDataAdapterTests
    {
        string _dbPath;
        string _shmPath;
        string _walPath;

        public UsersDataAdapterTests()
        {
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestDb\\TestNBABets.db");
            _shmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestDb\\TestNBABets.db-shm");
            _walPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestDb\\TestNBABets.db-wal");


        }
        [Fact]
        public void SuccessfullyAddUser()
        {
            // Arrange
            // Copy the empty data base out
            string[] copyDBs =
                [
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NBABets.db"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NBABets.db-shm"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NBABets.db-wal")
                ];

            foreach (var db in copyDBs)
            {
                if (File.Exists(db))
                {
                    File.Delete(db);
                }
            }
            IUserAdapter usersAdapter = new UsersAdapter();
            User user = new User()
            {
                ID = Guid.NewGuid(),
                Name = "Tony Hawk",
            };
                

            string connectionString = $"Data Source={copyDBs[0]};Version=3;";

            foreach (var db in copyDBs)
            {
                if (db.Contains("db-shm"))
                {
                    File.Copy(_shmPath, db);
                }
                else if (db.Contains("db-wal"))
                {
                    File.Copy(_walPath, db);
                }
                else
                {
                    File.Copy(_dbPath, db);
                }
            }

            // Act
            usersAdapter.Add(user);

            // Assert
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT Name FROM Users WHERE ID = @ID", connection))
                {
                    command.Parameters.AddWithValue("@ID", user.ID.ToString());
                    string userName = command.ExecuteScalar()?.ToString();
                    userName.ShouldBe(user.Name);
                }
            }            
        }


        [Fact]
        public void SuccessfullyGetAllUsers()
        {
            // Arrange
            List<User> users = new List<User>()
            {
                new User { ID = Guid.NewGuid(), Name = "Jimmy Butler"  },
                new User { ID = Guid.NewGuid(), Name = "Michael Jordan" },
                new User { ID = Guid.NewGuid(), Name = "Luka Doncic" }
            };
            IUserAdapter usersAdapter = new UsersAdapter();
            foreach (var user in users)
            {
                usersAdapter.Add(user);
            }

            // Act
            var result = usersAdapter.GetAll();
            // Assert
            result.ShouldNotBeNull();
        }

        [Fact]
        public void SuccessfullyGetUserByName()
        {
            // Arrange
            string userName = "Michael Jordan";
            IUserAdapter usersAdapter = new UsersAdapter();

            // Act
            var result = usersAdapter.Get(userName);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(userName);
            result.BetsPlaced.ShouldNotBeNull();
        }

        [Fact]
        public void SuccessfullyEditUserRemoveBet()
        {
            // Arrange
            IUserAdapter usersAdapter = new UsersAdapter();
            var getUser = usersAdapter.Get("Michael Jordan");
            getUser?.BetsPlaced?.RemoveAt(1);

            // Act
            usersAdapter.Edit(getUser);

            // Assert
            var result = usersAdapter.Get("Michael Jordan");
            result?.BetsPlaced?.Count.ShouldBe(0);
        }

        [Fact]
        public void SuccessfullyEditUserAddBet()
        {
            // Arrange
            IUserAdapter userAdapter = new UsersAdapter();
            var getUser = userAdapter.Get("Luka Doncic");
            getUser?.BetsPlaced?.Add(Guid.NewGuid());

            // Act
            userAdapter.Edit(getUser);

            // Assert
            var result = userAdapter.Get("Luka Doncic");
            result?.BetsPlaced?.Count.ShouldBe(1);
        }

        [Fact]
        public void SuccessfullyDeleteUser()
        {
            // Arrange
            // Add user "Tony Hawk" to database
            SuccessfullyAddUser();

            // Data adapter
            IUserAdapter usersAdapter = new UsersAdapter();
            // Variables
            string toDelete = "Tony Hawk";
            string db = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NBABets.db");
            string connectionString = $"Data Source={db};Version=3;";

            // Act
            usersAdapter.Delete(toDelete);

            // Assert
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Users WHERE Name = @Name", connection))
                {
                    command.Parameters.AddWithValue("@Name", toDelete);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    count.ShouldBe(0);
                }
            }
        }
    }
}
