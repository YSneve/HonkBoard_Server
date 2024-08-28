using HonkBoard_Backend.Core.Controller;
using HonkBoard_Backend.Core.Structures;

namespace HonkBoard_Backend.Core.Games.JustOne
{
    public class HubHandler : IUsersHandler
    {
        private readonly Dictionary<string, GameController> _lobbies;
        private readonly Dictionary<string, string> _playerLobbyPair;

        public HubHandler() 
        {

            _lobbies = new();
            _playerLobbyPair = new();
        
        }

        public bool HasLobby(string lobbyId)
        {

            return _lobbies.ContainsKey(lobbyId);

        }


        public bool HasUserConnected(string connectionId) 
        {

            return _playerLobbyPair.ContainsKey(connectionId);
        
        }

        public void AddLobby(string lobbyId, GameController controller)
        {

            _lobbies.Add(lobbyId, controller);

        }

        public GameController GetGameController(string lobbyId)
        {

            return _lobbies[lobbyId];

        }

        public void AddConnection(string connectionId, string lobbyId)
        {
            _playerLobbyPair.Add(connectionId, lobbyId);

        }

        public string GetLobbyId(string connectionId)
        {

            return _playerLobbyPair[connectionId];

        }

        public void RemoveUser(string connectionId)
        {
            var lobbyId = _playerLobbyPair[connectionId];

            _playerLobbyPair.Remove(connectionId);

            _lobbies[lobbyId].DisconnectPlayer(connectionId);
        }

        public bool IsLobbyEmpty(string lobbyId)
        {

            return _playerLobbyPair.Values.ToList().FindAll(elem => elem == lobbyId).Count <= 0;

        }

        public void RemoveLobby(string lobbyId)
        {

            _lobbies.Remove(lobbyId);

        } 

        public bool HasUsersLastConnect(string lobbyId, string connectionId)
        {

            if (!_lobbies.ContainsKey(lobbyId))
            {

                return false;

            }

            var lobby = _lobbies[lobbyId];

            if (lobby.GameInfo.PlayersList.Any(player => player.ConnectionId == connectionId)) 
            {

                return true;
            
            }

            return false;

        }
        
        public string GetLink()
        {
            return "/just-one";
        }
    }
}
