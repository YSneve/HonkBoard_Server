using HonkBoard_Backend.Core.Games.JustOne;
using HonkBoard_Backend.Core.Structures;
using Microsoft.Extensions.Options;

namespace HonkBoard_Backend.Core.Controller.Lobby
{
    /*!
     * \brief Контроллер лобби
     */
    public class LobbyUsersHandler
    {

        /*
         * Словарь формата
         * lobbyId : List<LobbyInfo>
         */
        private Dictionary<string, LobbyInfo> _lobbyInfo = new();

        private Dictionary<string, string> _userConnectionToLobby = new();

        private readonly Random random = new();

        private readonly Hub JustOneHub;


        /*!
         * \brief Структура списка игр, с их айди и конфигурацией. Стркутура конфигурации игр GameInfo.cs
         */
        public List <GameInfo> GamesList { get; } = new();


        public LobbyUsersHandler(IOptions<List<GameInfo>> gamesInfo, Hub justOneHub)
        {
            // по идее можно все это гомно вынести в один класс gameConfig, а в appsettings игры перчислять списком..? 
            GamesList = gamesInfo.Value;
            JustOneHub = justOneHub;
        }


        public Task<bool> IsCreated(string lobbyId)
        {
            return Task.FromResult(_lobbyInfo.Keys.ToList().Find(id => id == lobbyId) != null);
        }

        public string GetLobbyId(string connectionId)
        {
            return _userConnectionToLobby[connectionId];
        }

        public void AddUserToLobby(string connectionId, string lobbyId, User user)
        {
            var participant = new Participant(connectionId, false, user);

            _userConnectionToLobby.Add(connectionId, lobbyId);

            if (_lobbyInfo.ContainsKey(lobbyId))
            {
                _lobbyInfo[lobbyId].UsersList.Add(participant);
            }
            else
            {
                participant.IsHost = true;

                var lobbyInfo = new LobbyInfo()
                {
                    GameStarted = false,
                    UsersList = new List<Participant>
                    {
                        participant
                    }
                };

                _lobbyInfo.Add(lobbyId, lobbyInfo);

               
            }
        }

        public List<Participant> GetUsersList(string lobbyId)
        {
            return _lobbyInfo[lobbyId].UsersList;
        }

        public void StartGame(string connectionId, List<ServerGameParameter> parameters)
        {
            var lobbyId = _userConnectionToLobby[connectionId];

            JustOneHub.CreateLobby(lobbyId, parameters);

            _lobbyInfo[lobbyId].GameStarted = true;
        }

        public bool IsInLobby(string connectionId)
        {
            return _userConnectionToLobby.ContainsKey(connectionId);
        }

        public void RemoveUserFromLobby(string connectionId)
        {
            var lobbyId = _userConnectionToLobby[connectionId];

            _userConnectionToLobby.Remove(connectionId);

            _lobbyInfo[lobbyId].UsersList.Remove(_lobbyInfo[lobbyId].UsersList.Find(userInfo => userInfo.ConnectionId == connectionId));
        }

        public bool IsGameStarted(string connectionId)
        {
            var lobbyId = _userConnectionToLobby[connectionId];

            return _lobbyInfo[lobbyId].GameStarted;
        }

        public bool IsUserHost(string connectionId)
        {
            var lobbyId = _userConnectionToLobby[connectionId];

            return _lobbyInfo[lobbyId].UsersList.Find(user => user.ConnectionId == connectionId).IsHost;
        }

        public void RemoveLobby(string lobbyId)
        {
            _lobbyInfo.Remove(lobbyId);

        }

        public void TransferHost(string lobbyId)
        {
            var newHostIndex = random.Next(0, _lobbyInfo[lobbyId].UsersList.Count - 1);

            _lobbyInfo[lobbyId].UsersList[newHostIndex].IsHost = true;
        }

        public void SelectGame(string lobbyId, int gameId)
        {
            var lobby = _lobbyInfo[lobbyId];

            lobby.SelectedGame = gameId;
        }

        public int TotalConnectedParticipants()
        {
            return _userConnectionToLobby.Count();
        }

        public int? GetSelectedGame(string lobbyId)
        {
            var lobby = _lobbyInfo[lobbyId];

            return lobby.SelectedGame;
        }

    }
}
