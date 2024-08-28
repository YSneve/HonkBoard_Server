using HonkBoard_Backend.Core.Structures;

namespace HonkBoard_Backend.Core.Controller.Lobby
{
    public class LobbyInfo
    {
        public int? SelectedGame { get; set; } = 0;

        public bool GameStarted;

        public List<Participant> UsersList = new();

    }
}
