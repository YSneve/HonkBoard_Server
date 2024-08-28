using HonkBoard_Backend.Core.Games.JustOne.Structures.Enums;

namespace HonkBoard_Backend.Core.Games.JustOne.Structures
{
    /*!
     * \brief Класс-структура предоставляющий информацию об игре
     */
    public class Game
    {
        public List<Player> PlayersList { get; } = new();

        public List<Word> Words { get; set; } = new();


        public GAME_STATE GameState { get; set; }
        public string? WordToGuess { get; set; }
        public string? GuessAttempt { get; set; } = null;
        public int ScoreToWin { get; set; }

        public GAME_PHASE CurrentPhase { get; set; }
    }
}
