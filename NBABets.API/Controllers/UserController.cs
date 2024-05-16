using Microsoft.AspNetCore.Mvc;
using NBABets.Models;
using NBABets.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NBABets.API
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly ILogger _log;
        private readonly IMapper<UserMapRequest, UserDto> _userMapper;
        private readonly IUserAdapter _userAdapter;
        public UserController(IUserAdapter userAdapter, IMapper<UserMapRequest, UserDto> userMapper)
        {
            _log = Log.ForContext<UserController>();
            _userAdapter = userAdapter;
            _userMapper = userMapper;

        }
        /// <summary>
        /// Authorises users into the application
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>Returns a user Dto which has a bit more information than a regular user object</returns>
        [HttpGet("authorise", Name = "Authorise")]
        public ActionResult<UserDto> Authorise(string userName)
        {
            // Check if the user exists
            var user = _userAdapter.Get(userName);

            // If null, create them, else return them
            if (user == null)
            {
                User newUser = new User()
                {
                    ID = Guid.NewGuid(),
                    Name = userName,
                };

                _userAdapter.Add(newUser);

                UserMapRequest userMapRequest = new UserMapRequest()
                {
                    user = newUser,
                };

                return _userMapper.Map(userMapRequest);
            }
            else
            {
                UserMapRequest userMapRequest = new UserMapRequest()
                {
                    user = user,
                };
                return _userMapper.Map(userMapRequest);
            }
        }
        /// <summary>
        /// Gets all users in the database
        /// </summary>
        /// <returns>Returns a list of all users</returns>
        [HttpGet("", Name = "GetAllUsers")]
        public ActionResult<List<UserDto>> GetAllUsers()
        {
            List<User> users = _userAdapter.GetAll();
            // map users
            List<UserDto> userDtos = users.Select(user =>
            {
                UserMapRequest request = new UserMapRequest { user = user };
                return _userMapper.Map(request);
            }).ToList();

            return userDtos;
        }

        /// <summary>
        /// Gets a single user
        /// </summary>
        /// <param name="UserIDorName"></param>
        /// <returns>Returns a single user DTO</returns>
        [HttpGet("{UserIDorName}", Name = "GetUser")]
        public ActionResult<UserDto> GetUser([FromRoute] string UserIDorName)
        {
            var user = _userAdapter.Get(UserIDorName);

            if (user != null)
            {
                UserMapRequest userMapRequest = new UserMapRequest()
                {
                    user = user
                };
                return _userMapper.Map(userMapRequest);
            }
            _log.Information($"User not found: {UserIDorName}");
            return BadRequest($"User not found: {UserIDorName}");
        }
    }
}
