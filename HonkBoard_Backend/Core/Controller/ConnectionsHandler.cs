using HonkBoard_Backend.Core.Structures;

namespace HonkBoard_Backend.Core.Controller
{
    public class ConnectionsHandler
    {

        private readonly List<IUsersHandler> _handlersList;

        public ConnectionsHandler(IEnumerable<IUsersHandler> handlersList)
        {

            _handlersList = handlersList.ToList();

        }

        public async Task<string?> HasConnectedUser(string lobbyId, string lastConnectionId)
        {
            return await Task.Run(() =>
            {

                var controller = _handlersList.Find(elem => elem.HasUsersLastConnect(lobbyId, lastConnectionId) == true);

                if (controller != null)
                {

                    return controller.GetLink();

                }
                return null;

            });
            
        }

    }
}