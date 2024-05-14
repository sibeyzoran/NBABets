namespace NBABets.Services
{
    public interface IGameAdapter
    {
        /// <summary>
        /// Adds a game to the games table SQL database
        /// </summary>
        /// <param name="item"></param>
        void Add(Game item);
        /// <summary>
        /// Edits a game to the games table SQL database
        /// </summary>
        /// <param name="item"></param>
        void Edit(Game item);
        /// <summary>
        /// Removes a game to the games table SQL database
        /// </summary>
        /// <param name="IDorName"></param>
        void Delete(string IDorName);
        /// <summary>
        /// Gets a game to the games table SQL database
        /// </summary>
        /// <param name="IDorName"></param>
        /// <returns></returns>
        Game? Get(string IDorName);
        /// <summary>
        /// Gets all the entries in the games table
        /// </summary>
        /// <returns></returns>
        List<Game> GetAll();
    }
}
