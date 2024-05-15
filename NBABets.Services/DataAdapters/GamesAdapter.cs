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
                    var command = new SQLiteCommand("INSERT INTO Games (ID, Name, StartDate, EndDate, Status, Score) VALUES (@ID, @Name, @StartDate, @EndDate, @Status, @Score)", connection);
                    command.Parameters.AddWithValue("@ID", item.ID.ToString());
                    command.Parameters.AddWithValue("@Name", item.Name);
                    command.Parameters.AddWithValue("@StartDate", item.StartDate.ToString());
                    command.Parameters.AddWithValue("@EndDate", item.EndDate.ToString());
                    command.Parameters.AddWithValue("@Status", item.Status);
                    command.Parameters.AddWithValue("@Score", item.Score);

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

                    // Retrieve current values from the database
                    var selectCommand = new SQLiteCommand("SELECT Status, Score, EndDate FROM Games WHERE ID = @ID", connection);
                    selectCommand.Parameters.AddWithValue("@ID", item.ID.ToString());

                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string currentStatus = reader["Status"].ToString();
                            string currentScore = reader["Score"].ToString();
                            string currentEndDate = reader["EndDate"].ToString();
                            DateTime? convertedEndDate = null;
                            if (currentEndDate != "")
                            {
                                convertedEndDate = Convert.ToDateTime(currentEndDate);
                            }
                            

                            // Compare with new values
                            bool statusChanged = currentStatus != item.Status;
                            bool scoreChanged = currentScore != item.Score;
                            bool endDateChanged = convertedEndDate != item.EndDate;

                            // If any value has changed, update the corresponding column in the database
                            if (statusChanged || scoreChanged || endDateChanged)
                            {
                                var updateCommand = new SQLiteCommand("UPDATE Games SET", connection);
                                if (statusChanged)
                                    updateCommand.CommandText += " Status = @Status,";
                                if (scoreChanged)
                                    updateCommand.CommandText += " Score = @Score,";
                                if (endDateChanged)
                                    updateCommand.CommandText += " EndDate = @EndDate,";

                                // Remove the trailing comma
                                updateCommand.CommandText = updateCommand.CommandText.TrimEnd(',');

                                // Add parameters for changed values
                                if (statusChanged)
                                    updateCommand.Parameters.AddWithValue("@Status", item.Status);
                                if (scoreChanged)
                                    updateCommand.Parameters.AddWithValue("@Score", item.Score);
                                if (endDateChanged)
                                    updateCommand.Parameters.AddWithValue("@EndDate", item.EndDate);

                                updateCommand.CommandText += " WHERE ID = @ID";
                                updateCommand.Parameters.AddWithValue("@ID", item.ID.ToString());

                                _log.Information($"Editing {item.Name}'s");

                                // Execute
                                updateCommand.ExecuteNonQuery();
                                _log.Information($"{item.Name} successfully edited.");
                            }
                            else
                            {
                                _log.Information($"No changes to {item.Name}.");
                            }
                        }
                        else
                        {
                            _log.Error($"Game with ID {item.ID} not found.");
                        }
                    }
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
                        ? "SELECT ID, Name, StartDate, Status, Score, EndDate FROM Games Where ID = @ID"
                        : "SELECT ID,  Name, StartDate, Status, Score, EndDate FROM Games Where Name = @Name";
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
                            var startdate = DateTime.Parse(reader["StartDate"].ToString());
                            var enddate = reader["EndDate"].ToString();
                            var status = reader["Status"].ToString();
                            var score = reader["Score"].ToString();

                            result = new Game() 
                            { 
                                ID = id,
                                Name = name,
                                StartDate = startdate,
                                Score = score,
                                Status = status
                            };
                            if (enddate != "")
                            {
                                result.EndDate = DateTime.Parse(reader["EndDate"].ToString());
                            }
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
                                StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                                EndDate = DateTime.Parse(reader["EndDate"].ToString()),
                                Status = reader["Status"].ToString(),
                                Score = reader["Score"].ToString()
                            };

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

