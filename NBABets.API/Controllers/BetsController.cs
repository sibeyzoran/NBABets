using Microsoft.AspNetCore.Mvc;
using NBABets.Models;
using NBABets.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NBABets.API
{
    [ApiController]
    [Route("bets")]
    public class BetsController : ControllerBase
    {
        private readonly ILogger _log;
        private readonly IMapper<BetMapRequest, BetDto> _betsMapper;
        private readonly IBetsAdapter _betsAdapter;
        public BetsController(IBetsAdapter betsAdapter, IMapper<BetMapRequest, BetDto> betsMapper)
        {
            _betsAdapter = betsAdapter;
            _betsMapper = betsMapper;
            _log = Log.ForContext<BetsController>();
        }

        /// <summary>
        /// Gets all bets in the database
        /// </summary>
        /// <returns>returns a list of all bets</returns>
        [HttpGet("", Name = "GetAllBets")]
        public ActionResult<List<BetDto>> GetAllBets()
        {
            List<Bet> bets = _betsAdapter.GetAll();

            // map bets
            List<BetDto> betDtos = bets.Select(bet =>
            {
                BetMapRequest request = new BetMapRequest { bet = bet };
                return _betsMapper.Map(request);
            }).ToList();

            return betDtos;
        }

        /// <summary>
        /// Gets a single bet
        /// </summary>
        /// <param name="IDorName"></param>
        /// <returns>Returns the rqeuested Bet</returns>
        [HttpGet("{IDorName}", Name = "GetBet")]
        public ActionResult<BetDto> GetBet([FromRoute] string IDorName)
        {
            var bet = _betsAdapter.Get(IDorName);
            if (bet != null)
            {
                BetMapRequest betMapRequest = new BetMapRequest
                {
                    bet = bet
                };
                return _betsMapper.Map(betMapRequest);
            }
            _log.Information($"Bet not found: {IDorName}");
            return BadRequest($"Bet not found: {IDorName}");
        }

        [HttpPost("add", Name = "AddBet")]
        public ActionResult AddBet([FromBody] BetDto bet)
        {
            // convert betDto into a regular bet
            Bet toAdd = new Bet()
            {
                ID = bet.ID,
                Name = bet.Name,
                Amount = bet.Amount,
                GameID = bet.Game.ID,
                UserID = bet.UserID,
                Result = bet.Result
            };

            _betsAdapter.Add(toAdd);
            if (_betsAdapter.Get(toAdd.ID.ToString()) != null)
            {
                return Ok();
            }

            _log.Information($"Error adding bet: {bet.ID}");
            return BadRequest($"Error adding bet: {bet.ID}");
        }

        /// <summary>
        /// Edits a bets amount
        /// </summary>
        /// <param name="bet"></param>
        /// <returns>Returns Ok always - probably needs to be changed at some point</returns>
        [HttpPut("editbet", Name = "EditBet")]
        public ActionResult EditBet([FromBody] BetDto bet)
        {
            // convert betDto into a regular bet
            Bet toEdit = new Bet()
            {
                ID = bet.ID,
                Name = bet.Name,
                Amount = bet.Amount,
                GameID = bet.Game.ID,
                UserID = bet.UserID,
                Result = bet.Result
            };

            _betsAdapter.Edit(toEdit);

            return Ok();
        }

        [HttpDelete("delete/{IDorName}", Name = "DeleteBet")]
        public ActionResult DeleteBet([FromRoute] string IDorName)
        {
            _betsAdapter.Delete(IDorName);
            return Ok();
        }

    }
}
