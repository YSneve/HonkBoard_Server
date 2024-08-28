namespace HonkBoard_Backend.Core.Structures
{
    public class StatisticsField
    {
        public string StatMesure { get; set; }
        public string StatText { get; set; }

        public dynamic StatValue { get; set; }
        
        public Type StatType { get; set; }
    }

}