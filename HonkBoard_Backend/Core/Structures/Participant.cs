using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace HonkBoard_Backend.Core.Structures
{
    public class Participant : User
    {
        public string ConnectionId { get; set; }
        public bool IsHost { get; set; }

        public string LastCid ;

        public Participant(string connectionId, bool isHost, User user) : base(user.Avatar, user.Name, user.Id)
        {
            ConnectionId = connectionId;
            IsHost = isHost;
        }

        public Participant GetParticipant() => this;

    }
}
