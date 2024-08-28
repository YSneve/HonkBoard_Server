namespace HonkBoard_Backend.Core.Controller
{
    public interface IUsersHandler
    {

        public bool HasUsersLastConnect(string lobbyId, string connectionId);

        public string GetLink();

    }
}
