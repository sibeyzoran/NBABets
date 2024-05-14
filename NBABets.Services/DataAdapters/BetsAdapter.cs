using Serilog;
using System.Data.SQLite;
using ILogger = Serilog.ILogger;

namespace NBABets.Services
{
    public class BetsAdapter : IBetsAdapter
    {
        private string _connectionString;
        private ILogger _log;
        private IUserAdapter _userAdapter;

        public BetsAdapter(IUserAdapter userAdapter)
        {
            // setup SQL connection string
            string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NBABets.db");
            _connectionString = $"Data Source={dbFilePath};Version=3;";

            // init logger
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs\\BetsAdapter.log");
            _log = new LoggerConfiguration()
                .WriteTo.File($"{logPath}")
                .CreateLogger();
            // init user adapter
            _userAdapter = userAdapter;
        }

        public void Add(Bet item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    _log.Information($"Updating user: {item.UserID}'s bets placed");
                    var user = _userAdapter.Get(item.UserID.ToString());
                    if (user != null)
                    {
                        // Update the users bets placed
                        user?.BetsPlaced?.Add(item.ID);
                        _userAdapter.Edit(user);

                        // Open connection to the SQL database
                        connection.Open();

                        // Craft command
                        var command = new SQLiteCommand(connection);
                        command.CommandText = "INSERT INTO Bets (ID, Amount, GameID, Result, Name, UserID) VALUES (@ID, @Amount, @GameID, @Result, @Name, @UserID)";
                        command.Parameters.AddWithValue("@ID", item.ID.ToString());
                        command.Parameters.AddWithValue("@Amount", item.Amount);
                        command.Parameters.AddWithValue("@GameID", item.GameID.ToString());
                        command.Parameters.AddWithValue("@Result", item.Result);
                        command.Parameters.AddWithValue("@Name", item.Name);
                        command.Parameters.AddWithValue("@UserID", item.UserID.ToString());
                        _log.Information($"Adding Bet: {item.Name}");

                        // Execute command
                        command.ExecuteNonQuery();
                        _log.Information($"Bet added successfully: {item.Name}");
                    }
                    else
                    {
                        _log.Error($"User: {item.UserID} doesn't exist.");
                    }
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
                    // Clean up users bets placed
                    var bet = Get(IDorName);
                    var user = _userAdapter.Get(bet.UserID.ToString());

                    if (user != null && user.BetsPlaced != null)
                    {
                        if (user.BetsPlaced.Contains(bet.ID))
                        {
                            user.BetsPlaced.Remove(bet.ID);
                            _userAdapter.Edit(user);
                        }
                    }

                    // Open connection
                    connection.Open();

                    // create command
                    var commandText = Guid.TryParse(IDorName, out var ID)
                        ? $"DELETE FROM Bets WHERE ID = @ID"
                        : $"DELETE FROM Bets Where Name = @Name";
                    var command = new SQLiteCommand(commandText, connection);

                    if (ID != Guid.Empty)
                    {
                        command.Parameters.AddWithValue("@ID", ID.ToString());
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@Name", IDorName);
                    }
                    _log.Information($"Deleting bet: {IDorName}");

                    // Execute command
                    command.ExecuteNonQuery();

                    _log.Information($"Bet: {IDorName} deleted successfully.");
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }
            }
        }

        public void Edit(Bet item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();

                    // Craft command
                    var command = new SQLiteCommand("UPDATE Bets SET Amount = @Amount WHERE ID = @ID", connection);
                    command.Parameters.AddWithValue("@ID", item.ID.ToString());
                    command.Parameters.AddWithValue("@Amount", item.Amount);

                    _log.Information($"Editing {item.Name}'s waged amount");

                    // Execute command
                    command.ExecuteNonQuery();
                    _log.Information($"{item.Name} edited successfully");
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }
            }
        }

        public Bet? Get(string IDorName)
        {
            Bet? result = null;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    _log.Information($"Retrieving bet: {IDorName}");
                    // Open connection
                    connection.Open();

                    // Craft command
                    var commandText = Guid.TryParse(IDorName, out var ID)
                        ? "SELECT ID, Name, Amount, GameID, Result, UserID FROM Bets Where ID = @ID"
                        : "SELECT ID, Name, Amount, GameID, Result, UserID FROM Bets Where Name = @Name";
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
                            var amount = reader["Amount"];
                            var gameid = Guid.Parse(reader["GameID"].ToString());
                            var readerResult = reader["Result"].ToString();
                            var userid = Guid.Parse(reader["UserID"].ToString());

                            result = new Bet();
                            result.ID = id;
                            result.Name = name;
                            result.Amount = Convert.ToDouble(amount);
                            result.GameID = gameid;
                            result.Result = readerResult;
                            result.UserID = userid;
                            _log.Information($"Successfully retrieved bet: {IDorName}");
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

        public List<Bet> GetUsersBets(string IDorName)
        {
            var result = new List<Bet>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    _log.Information($"Retrieving bets for user: {IDorName}");
                    var user = _userAdapter.Get(IDorName);

                    if (user != null && user.BetsPlaced != null && user.BetsPlaced.Any())
                    {
                        // Open connection
                        connection.Open();

                        // Craft command
                        var commandText = $"SELECT ID, Name, Amount, GameID, Result, UserID FROM Bets WHERE UserID = @UserID";
                        var command = new SQLiteCommand(commandText, connection);
                        command.Parameters.AddWithValue("@UserID", user.ID.ToString());

                        // Execute command
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var bet = new Bet
                                {
                                    ID = Guid.Parse(reader["ID"].ToString()),
                                    Name = reader["Name"].ToString(),
                                    Amount = Convert.ToDouble(reader["Amount"]),
                                    GameID = Guid.Parse(reader["GameID"].ToString()),
                                    Result = reader["Result"].ToString(),
                                    UserID = user.ID
                                };
                                result.Add(bet);
                            }
                        }
                    }
                    else
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }
            }
            _log.Information($"Successfully retrieved bets for: {IDorName}");
            return result;
        }

        public List<Bet> GetAll()
        {
            var result = new List<Bet>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    // Open Connection
                    connection.Open();

                    // Craft command
                    var command = new SQLiteCommand("SELECT * FROM Bets", connection);
                    _log.Information("Retrieving all bets");

                    // Execute command
                    using (var reader = command.ExecuteReader())
                    { while (reader.Read())
                        {
                            var bet = new Bet
                            {
                                ID = Guid.Parse(reader["ID"].ToString()),
                                Name = reader["Name"].ToString(),
                                Amount = Convert.ToDouble(reader["Amount"]),
                                GameID = Guid.Parse(reader["GameID"].ToString()),
                                Result = reader["Result"].ToString(),
                                UserID = Guid.Parse(reader["UserID"].ToString())
                            };
                            result.Add(bet);
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
