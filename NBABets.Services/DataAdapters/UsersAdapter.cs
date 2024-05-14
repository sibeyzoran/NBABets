using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NBABets.Services
{
    public class UsersAdapter : IUserAdapter
    {
        private string _connectionString;
        private ILogger _log;

        public UsersAdapter()
        {
            // setup SQL connection string
            string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NBABets.db");
            _connectionString = $"Data Source={dbFilePath};Version=3;";

            // init logger
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs\\UsersAdapter.log");
            _log = new LoggerConfiguration()
                .WriteTo.File($"{logPath}")
                .CreateLogger();
        }
        public void Add(User item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    // Open connection to the SQL database
                    connection.Open();

                    // Craft command
                    var command = new SQLiteCommand(connection);

                    if (item.BetsPlaced != null && item.BetsPlaced.Any())
                    {
                        command.CommandText = "INSERT INTO Users (ID, Name, BetsPlaced) VALUES (@ID, @Name, @BetsPlaced)";
                        var betsPlacedString = string.Join(",", item.BetsPlaced.Select(g => g.ToString()));
                        command.Parameters.AddWithValue("@BetsPlaced", betsPlacedString);
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO Users (ID, Name) VALUES (@ID, @Name)";
                    }

                    // Add parameters
                    command.Parameters.AddWithValue("@ID", item.ID.ToString());
                    command.Parameters.AddWithValue("@Name", item.Name);
                    _log.Information($"Adding user: {item.Name}");

                    // Execute SQL command
                    command.ExecuteNonQuery();
                    _log.Information($"User: {item.Name} successfully added to database.");
                }
                catch (Exception ex) 
                {
                    _log.Error(ex.Message);
                }
            }
        }

        public void Delete(string IDorName)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    // Open SQL connection
                    connection.Open();

                    // Do i want to delete the bets if a user is deleted? Maybe not for financial reasons?

                    // Create command 
                    var commandText = Guid.TryParse(IDorName, out var ID)
                        ? $"DELETE FROM Users WHERE ID = @ID"
                        : $"DELETE FROM Users Where Name = @Name";
                    var command = new SQLiteCommand(commandText, connection);

                    // Add parameters based on whether it's a guid or not
                    if (ID != Guid.Empty)
                    {
                        command.Parameters.AddWithValue("@ID", ID.ToString());
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@Name", IDorName);
                    }
                    _log.Information($"Executing SQL Query: {commandText}");

                    // Execute command
                    command.ExecuteNonQuery();

                    
                    _log.Information($"User {IDorName} deleted successfully.");
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }
            }

        }

        public void Edit(User item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();

                    // Craft command 
                    var command = new SQLiteCommand("UPDATE Users SET BetsPlaced = @BetsPlaced WHERE ID = @ID", connection);
                    string betsPlacedString = string.Join(",", item.BetsPlaced.Select(id => id.ToString()));

                    command.Parameters.AddWithValue("@BetsPlaced", betsPlacedString);
                    command.Parameters.AddWithValue("@ID", item.ID.ToString());

                    _log.Information($"Editing {item.Name}'s bets");

                    command.ExecuteNonQuery();

                    _log.Information($"Bets edited for {item.Name}");
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }
            }
        }

        public User? Get(string IDorName)
        {
            User? result = null;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    _log.Information($"Retrieving user: {IDorName}");
                    // Open connection
                    connection.Open();

                    // Craft command
                    var commandText = Guid.TryParse(IDorName, out var ID)
                        ? $"SELECT ID, Name, BetsPlaced FROM Users Where ID = @ID"
                        : $"SELECT ID, Name, BetsPlaced FROM Users Where Name = @Name";

                    var command = new SQLiteCommand(commandText, connection);

                    if (ID != Guid.Empty)
                    {
                        command.Parameters.AddWithValue("@ID", ID.ToString());
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@Name", IDorName);
                    }

                    // Execute command
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var id = Guid.Parse(reader["ID"].ToString());
                            var name = reader["Name"].ToString();
                            var betsPlacedString = reader["BetsPlaced"].ToString();

                            result = new User();
                            result.ID = id;
                            result.Name = name;
                            // Split bets placed into a list of Guids
                            if (betsPlacedString != null)
                            {
                                result.BetsPlaced = betsPlacedString.Split(',').Select(Guid.Parse).ToList();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }
            }
            _log.Information($"Successfully retrieved user: {IDorName}");
            return result;
        }

        public List<User> GetAll()
        {
            var result = new List<User>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    // Open connection
                    connection.Open();

                    var command = new SQLiteCommand("SELECT * FROM USERS", connection);
                    _log.Information($"Getting all users from Users table");

                    // Execute command and get each user
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var userID = reader["ID"].ToString();
                            var userName = reader["Name"].ToString();
                            var betsPlaced = reader["BetsPlaced"].ToString();

                            var user = new User()
                            {
                                ID = Guid.Parse(userID),
                                Name = userName,
                                BetsPlaced = new List<Guid>()
                            };
                            if (!string.IsNullOrEmpty(betsPlaced))
                            {
                                var betIDs = betsPlaced.Split(',');
                                foreach (var betID in betIDs)
                                {
                                    if (Guid.TryParse(betID.Trim(), out Guid parsedID))
                                    {
                                        user.BetsPlaced.Add(parsedID);
                                    }
                                }
                            }
                            result.Add(user);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }
            }
            return result;
        }
    }
}
