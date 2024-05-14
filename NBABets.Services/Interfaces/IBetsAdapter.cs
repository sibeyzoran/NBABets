using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBABets.Services
{
    public interface IBetsAdapter
    {
        /// <summary>
        /// Adds a bet to the bets table SQL database
        /// </summary>
        /// <param name="item"></param>
        void Add(Bet item);
        /// <summary>
        /// Edits a bet entry in the bets table SQL database
        /// </summary>
        /// <param name="item"></param>
        void Edit(Bet item);
        /// <summary>
        /// Removes a bet entry from the bets table SQL database
        /// </summary>
        /// <param name="IDorName"></param>
        void Delete(string IDorName);
        /// <summary>
        /// Gets a bet entry in the bets table SQL database
        /// </summary>
        /// <param name="IDorName"></param>
        /// <returns></returns>
        Bet? Get(string IDorName);
        /// <summary>
        /// Gets all the entries in the bets table
        /// </summary>
        /// <returns></returns>
        List<Bet> GetAll();
        /// <summary>
        /// Gets all the bets for a particular user
        /// </summary>
        /// <returns>A list of the users bets</returns>
        List<Bet> GetUsersBets(string IDorName);
    }
}
