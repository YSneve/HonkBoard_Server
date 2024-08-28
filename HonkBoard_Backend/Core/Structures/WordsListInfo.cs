using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace HonkBoard_Backend.Core.Structures
{
    public class WordsListInfo
    {
        public string Text { get; set; }

        public string Link { get; set; }

        public int Count { private get; set; }

        public string GetListLink() { return Link; }

        public int GetCount() { return Count; }

    }
}
