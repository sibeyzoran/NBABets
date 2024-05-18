# NBABets
A simple UI and API that interacts with a SQLite database to place bets on upcoming NBA games. It implements data adapters to perform CRUD operations on the SQLite database, these are called "Services". There is a service for each table: Users, Bets, Games. Unit tests have been written to assure their performance. A test playlist should come with the repository but, if not, the unit tests should be executed in the following order:

## Users tests
1. SuccessfullyAddUser
1. SuccessfullyGetAllUsers
1. SuccessfullyGetUser
1. SuccessfullyEditUserAddBet
1. SuccessfullyEditUserRemoveBet
1. SuccessfullyDeleteUser

## Bets Tests
1. SuccessfullyAddBet
1. SuccessfullyGetAllBets
1. SuccessfullyGetUsersBets
1. SuccessfullyEditBet
1. SuccessfullyDeleteBet

## Games Tests
1. SuccessfullyAddGame
1. SuccessfullyGetAllGames
1. SuccessfullyGetGame
1. SuccessfullyEditGame
1. SuccessfullyDeleteGame


# Tech used
NBA Bets uses entirely .NET. It implements a front end using Blazor in .NET 8. The front end project (NBABets.UI) uses a client to interact with the back end and ensures separation of concern. DTO's (Data transfer objects) are used so that the front end has fully fleshed out objects to work with. This helps minimise calls to the API and back end from the front end. The API speaks directly to the database using the data adapters and then maps back end objects (e.g. Game to GameDto) before returning them to the caller. 

It was decided to create a client due to the ease of implementing future programs that may need to interact with the API. This extensibility ensures that new projects only need to have access to the client DLL to be able to make fully formed requests to the API.

# Setup Instructions
1. Install the latest version of Visual Studio Preview 2022
1. Ensure you have .NET 7 & 8 installed
1. Clone the repository to your local disk
1. Ensure that you have entered an API key for the [NBA API](https://rapidapi.com/api-sports/api/api-nba) in the secrets.json file in the API project
1. Set the startup projects to include both the API project and the UI project
1. Ensure that the correct port and url are set in the UI projects appsettings.json file. These should point to the API e.g. Port: 7148, ApiUrl: localhost
1. Run the project!

#Screenshots
## Home page
![image](https://github.com/sibeyzoran/NBABets/assets/31067342/fd01e1a5-b172-427b-b74c-b014f5b50e66)

## Betting dialog
![image](https://github.com/sibeyzoran/NBABets/assets/31067342/fae31aeb-dcc2-4843-ad2b-52e78f107886)


## Your bets page
![image](https://github.com/sibeyzoran/NBABets/assets/31067342/92f13b0a-78b3-4fbc-ad2f-e74b45eb7e3a)


# Things to improve
1. More tests - API tests in particular. Mock responses/API could be used here effectively.
1. Small frontend design touches - $ signs to be added wherever there is an amount. Currently they're doubles.
1. A cleaner login splash screen that implements oAuth (allows for logins from more places e.g. Gmail, Azure more seamlessly)
1. A sign up process - currently as soon as someone successfully authenticates their user is created in the database
1. Streaming of scores - if a game is live currently it will  grab the score but won't update it as the game is being played
