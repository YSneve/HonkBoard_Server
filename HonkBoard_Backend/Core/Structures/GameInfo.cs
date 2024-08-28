using Microsoft.Extensions.Options;

namespace HonkBoard_Backend.Core.Structures
{
    public class GameInfo
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Link { get; set; }

        public int MinPlayers { get; set; }

        public int MaxPlayers { get; set; }

        public List<ClientGameParameter>? ParametersList { get; set; }
    }
}
