using System.Data;
using HonkBoard_Backend.Core.Games.JustOne.Structures.Enums;
using HonkBoard_Backend.Core.Structures;

namespace HonkBoard_Backend.Core.Games.JustOne.Structures
{
    public class Player : User
    {
        public int Score { get; set; }

        public PLAYER_ROLE Role { get; set; }

        public bool Disconnected { get; set; }

        public int RoundsDisconnected;

        public string ConnectionId { get; set; }



        public Player(User user, string connectionId, int score = 0, PLAYER_ROLE role = PLAYER_ROLE.SUGGESTER, bool disconnected = false)
            : base(user)
        {
            Score = score;
            Role = role;
            Disconnected = disconnected;
            RoundsDisconnected = 0;
            ConnectionId = connectionId;
        }

        
    }
}
