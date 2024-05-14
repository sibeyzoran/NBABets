using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBABets.Services
{
    public interface IDatabaseAdapter<T>
    {
        /// <summary>
        /// Adds a game, user or bet to the Type<T> table SQL database
        /// </summary>
        /// <param name="item"></param>
        void Add(T item);
        /// <summary>
        /// Edits a game, user or bet entry in the Type<T> table SQL database
        /// </summary>
        /// <param name="item"></param>
        void Edit(T item);
        /// <summary>
        /// Removes a game, user or bet entry in the Type<T> table SQL database
        /// </summary>
        /// <param name="IDorName"></param>
        void Delete(string IDorName);
        /// <summary>
        /// Gets a game, user or bet entry in the Type<T> table SQL database
        /// </summary>
        /// <param name="IDorName"></param>
        /// <returns></returns>
        T Get(string IDorName);
        /// <summary>
        /// Gets all the entries in a single table
        /// </summary>
        /// <returns></returns>
        List<T> GetAll();
    }
}
