namespace HonkBoard_Backend.Core.Games.JustOne.Structures
{

    public class Word
    {
        public int WordId { get; set; }
        public string Text { get; set; }
        public string PlayerId { get; set; }
        public bool IsVisible { get; set; }

        public string? ChangerId { get; set; }

        public Word(string text, string playerId, int wordId, bool visible = true)
        {
            Text = text;
            WordId = wordId;
            PlayerId = playerId;
            IsVisible = visible;
        }
    }
}
