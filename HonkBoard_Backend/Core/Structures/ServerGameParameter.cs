namespace HonkBoard_Backend.Core.Structures
{
    public class ServerGameParameter
    {
        public string Key { get; set; }
        public string? StringValue { get; set; } = null;
        public int? IntValue { get; set; } = null;
        public bool? BoolValue { get; set; } = null;
    }
}
