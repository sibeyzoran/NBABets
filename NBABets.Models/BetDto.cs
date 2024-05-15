using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBABets.Models
{
    public class BetDto
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }
        public GameDto Game { get; set; }
        public Guid UserID { get; set; }
        public string Result { get; set; }
    }
}
