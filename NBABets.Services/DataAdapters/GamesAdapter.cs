using Serilog;
using System.Data.SQLite;
using ILogger = Serilog.ILogger;

namespace NBABets.Services
{
    public class GamesAdapter : IGameAdapter
    {
        private string _connectionString;
        private ILogger _log;

        public GamesAdapter()
        {
            // setup SQL connection string
            string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NBABets.db");
            _connectionString = $"Data Source={dbFilePath};Version=3;";

            // init logger
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs\\GamesAdapter.log");
            _log = new LoggerConfiguration()
                .WriteTo.File($"{logPath}")
                .CreateLogger();
        }
        public void Add(Game item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    _log.Information($"Adding game: {item.Name}");

                    // Open Connection
                    connection.Open();

                    // Craft sql command
                    var command = new SQLiteCommand("INSERT INTO Games (ID, Name, Date, IsOpen) VALUES (@ID, @Name, @Date, @IsOpen)", connection);
                    command.Parameters.AddWithValue("@ID", item.ID.ToString());
                    command.Parameters.AddWithValue("@Name", item.Name);
                    command.Parameters.AddWithValue("@Date", item.Date.ToString());
                    command.Parameters.AddWithValue("@IsOpen", Convert.ToInt32(item.IsOpen));

                    // Execute command
                    command.ExecuteNonQuery();
                    _log.Information($"Successfully added game: {item.Name}");
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
                    // Open connection
                    connection.Open();

                    // Command
                    var commandText = Guid.TryParse(IDorName, out var ID)
                        ? $"DELETE FROM Games WHERE ID = @ID"
                        : $"DELETE FROM Games Where Name = @Name";
                    var command = new SQLiteCommand(commandText, connection);

                    if (ID != Guid.Empty)
                    {
                        command.Parameters.AddWithValue("@ID", ID.ToString());
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@Name", IDorName);
                    }
                    _log.Information($"Deleting game: {IDorName}");

                    // Execute
                    command.ExecuteNonQuery();
                    _log.Information($"Successfully deleted game: {IDorName}");
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }
            }
        }

        public void Edit(Game item)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    // Open
                    connection.Open();

                    // Craft command
                    var command = new SQLiteCommand("UPDATE Games SET IsOpen = @IsOpen WHERE ID = @ID", connection);
                    command.Parameters.AddWithValue("@IsOpen", Convert.ToInt32(item.IsOpen));
                    command.Parameters.AddWithValue("@ID", item.ID.ToString());

                    _log.Information($"Editing {item.Name}'s open status");

                    // Execute
                    command.ExecuteNonQuery();
                    _log.Information($"{item.Name} successfully edited.");
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }
            }
        }

        public Game? Get(string IDorName)
        {
            Game? result = null;
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    _log.Information($"Retrieving game: {IDorName}");

                    // Open
                    connection.Open();

                    // Craft 
                    var commandText = Guid.TryParse(IDorName, out var ID)
                        ? "SELECT ID, Name, Date, IsOpen FROM Games Where ID = @ID"
                        : "SELECT ID,  Name, Date, IsOpen FROM Games Where Name = @Name";
                    var command = new SQLiteCommand(commandText, connection);

                    if (ID != Guid.Empty)
                    {
                        command.Parameters.AddWithValue(@"ID", ID.ToString());
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
                            var date = DateTime.Parse(reader["Date"].ToString());
                            var isOpenString = reader["IsOpen"].ToString();
                            var isOpenInt = Convert.ToInt32(isOpenString);
                            var isOpenBool = Convert.ToBoolean(isOpenInt);

                            result = new Game() 
                            { 
                                ID = id,
                                Name = name,
                                Date = date,
                                IsOpen = isOpenBool
                            };
                            _log.Information($"Successfully returned game: {IDorName}");
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

        public List<Game> GetAll()
        {
            var result = new List<Game>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                try
                {
                    // Open
                    connection.Open();

                    // Craft
                    var command = new SQLiteCommand("SELECT * FROM Games", connection);
                    _log.Information("Retrieving all games");

                    // Execute command
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game()
                            {
                                ID = Guid.Parse(reader["ID"].ToString()),
                                Name = reader["Name"].ToString(),
                                Date = DateTime.Parse(reader["Date"].ToString()),
                            };
                            var isOpenString = reader["IsOpen"].ToString();
                            var isOpenInt = Convert.ToInt32(isOpenString);
                            var isOpenBool = Convert.ToBoolean(isOpenInt);
                            game.IsOpen = isOpenBool;
                            result.Add(game);
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

