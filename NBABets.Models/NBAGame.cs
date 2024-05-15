
using System.Linq.Expressions;

namespace NBABets.Models
{
    public class NBAGame
    {
        public int? id { get; set; }
        public string? league { get; set; }
        public Date date { get; set; }
        public int? stage { get; set; }
        public Status status { get; set; }
        public Periods periods { get; set; }
        public Arena arena { get; set; }
        public Teams teams { get; set; }
        public Scores scores { get; set; }
        public object[] officials { get; set; }
        public int? timesTied { get; set; }
        public int? leadChanges { get; set; }
        public string? nugget { get; set; }

    }

    public class Scores
    {
        public VisitorScore visitors { get; set; }
        public HomeScore home { get; set; }
    }

    public class HomeScore
    {
        public int? win { get; set; }
        public int? loss { get; set; }
        public Series series { get; set; }
        public object[] linescore { get; set; }
        public int? points { get; set; }
    }

    public class VisitorScore
    {
        public int? win { get; set; }
        public int? loss { get; set; }
        public Series series { get; set; }
        public object[] linescore { get; set; }
        public int? points { get; set; }
    }

    public class Series
    {
        public int? win { get; set; }
        public int? loss { get; set; }
    }

    public class Teams
    {
        public Visitor visitors { get; set; }
        public Home home { get; set; }
    }

    public class Home
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? nickname { get; set; }
        public string? code { get; set; }
        public string? logo { get; set; }
    }

    public class Visitor
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? nickname { get; set; }
        public string? code { get; set; }
        public string? logo { get; set; }
    }

    public class Arena
    {
        public string? name { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? country { get; set; }
    }

    public class Periods
    {
        public int? current { get; set; }
        public int? total { get; set; }
        public bool? endOfPeriod { get; set; }
    }

    public class Status
    {
        public object? clock { get; set; }
        public bool? halftime { get; set; }
        public int? @short { get; set; }
        public string? @long { get; set; }
    }

    public class Date
    {
        public string? start { get; set; }
        public string? end { get; set; }
        public string? duration { get; set; }
    }
}
