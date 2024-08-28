namespace HonkBoard_Backend.Core.Structures
{
    
    public class ClientGameParameter : ServerGameParameter
    {

        public string Text { get; set; }
        public int? MinValue { get; set; } = null;
        public int? MaxValue { get; set; } = null;
        
    }
}
