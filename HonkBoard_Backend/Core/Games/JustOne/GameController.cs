using HonkBoard_Backend.Core.Controller;
using HonkBoard_Backend.Core.Games.JustOne.Structures;
using HonkBoard_Backend.Core.Games.JustOne.Structures.Enums;
using HonkBoard_Backend.Core.Structures;
using System.Numerics;

namespace HonkBoard_Backend.Core.Games.JustOne
{
    /*!
     * \brief Класс отвечающий за логику игры, а также за параметры такие как: список игроков, слов, кто ведущий и т.д.
     */
    public class GameController
    {
        private readonly string _lobbyId;
        private readonly string _wordsCategory;

        private readonly WordsController _wordsController;

        private readonly Random _rand = new();

        private readonly Func<string, Task> _frameUpdaterAction;
        private readonly Func<string, int, Task> _currentTimeUpdater;
        private readonly Func<string, int, Task> _baseTimeUpdater;

        private readonly IDataAccess _warpper;

        private int _baseUpdateThreshold = 5;
        private int _updateTreshold;

        private int _timeStatBaseUpdateThreshold = 60;
        private int _timeStatUpdateThreshold;

        private int CurrentTime;

        private System.Threading.Timer _timer;

        private readonly System.Timers.Timer _threadTimer;
        public int SuggestTime { get; set; } = 0;
        public int VerifyTIme { get; set; } = 0;
        public int DecideTime { get; set; } = 0;
        public int AftermathTime { get; set; } = 0;

        public bool Initilized { get; set; } = false;

        public Game GameInfo { get; set; }

        ~GameController()
        {
            _wordsController.RemoveLobby(_lobbyId);
        }

        public GameController(Func<string, Task> frameUpdater, Func<string, int, Task> currentTimeUpdater, Func<string, int, Task> baseTimeUpdater, IDataAccess wrapper ,WordsController wordsController, string lobbyId, List<ServerGameParameter> parameters)
        {

            GameInfo = new Game();
            _lobbyId = lobbyId;
            _wordsController = wordsController;
            _warpper = wrapper;
            _wordsController.AddLobby(_lobbyId);

            _wordsCategory = "Simple";

            var parametrsDictionary = parameters.ToDictionary(elem => elem.Key, elem => elem);

            SuggestTime = parametrsDictionary.ContainsKey("SuggestTime") ? (int)parametrsDictionary["SuggestTime"].IntValue : 45;

            VerifyTIme = parametrsDictionary.ContainsKey("VerifyTIme") ? (int)parametrsDictionary["VerifyTIme"].IntValue : 10;

            DecideTime = parametrsDictionary.ContainsKey("DecideTime") ? (int)parametrsDictionary["DecideTime"].IntValue : 40;

            AftermathTime = parametrsDictionary.ContainsKey("AftermathTime") ? (int)parametrsDictionary["AftermathTime"].IntValue : 15;

            GameInfo.ScoreToWin = (parametrsDictionary.ContainsKey("ScoreToWin") ? (int)parametrsDictionary["ScoreToWin"].IntValue : 15);

            //_hub = hub;
            _frameUpdaterAction = frameUpdater;
            _currentTimeUpdater = currentTimeUpdater;
            _baseTimeUpdater = baseTimeUpdater;

            _updateTreshold = _baseUpdateThreshold;
            _timeStatUpdateThreshold = _timeStatBaseUpdateThreshold;

            CurrentTime = 0;

            _timer = new System.Threading.Timer(new TimerCallback(TimerTick), null, 500, 1000);

            //_threadTimer = new System.Timers.Timer();
            //_threadTimer.Elapsed += new ElapsedEventHandler(this.TimerTick);
            //_threadTimer.Interval = 1000;
            //_threadTimer.Enabled = true;
        }

        public async Task JoinGame(User user, string connectionId, string? lastConnectionId = null)
        {

            if (lastConnectionId != null)
            {
                var playerIndex = GameInfo.PlayersList.FindIndex(playerElem =>
                    playerElem.Disconnected && playerElem.ConnectionId == lastConnectionId);

                if (playerIndex != -1)
                {
                    GameInfo.PlayersList[playerIndex].Disconnected = false;
                    GameInfo.PlayersList[playerIndex].ConnectionId = connectionId;
                    GameInfo.PlayersList[playerIndex].RoundsDisconnected = 0;
                }
            }

            if (GameInfo.PlayersList.Find(playerElem => playerElem.ConnectionId == connectionId) == null)
            {

                GameInfo.PlayersList.Add(new Player(user, connectionId));

            }

            if (!Initilized && GameInfo.PlayersList.Count >= 2)
            {

                NextGuesser();
                StartGame();
                Initilized = true;
            }
        }

        public void DisconnectPlayer(string connectionId)
        {
            var player = GameInfo.PlayersList.Find(playerElem => playerElem.ConnectionId == connectionId);

            if (player != null)
            {

                player.Disconnected = true;

            }
        }

        public void StartGame()
        {
            NewWordToGuess();
            //NextGuesser();
            GameInfo.GameState = GAME_STATE.ONGOING;
            GameInfo.CurrentPhase = GAME_PHASE.SUGGEST;
            CurrentTime = SuggestTime;
            _baseTimeUpdater(_lobbyId, SuggestTime);
            _frameUpdaterAction(_lobbyId);

        }

        public void MakeGuess(string guessText)
        {
            GameInfo.GuessAttempt = guessText;

            CurrentTime = 0;

            if (guessText != GameInfo.WordToGuess) return;

            foreach (var player in GameInfo.PlayersList)
            {
                if (player.Role == PLAYER_ROLE.GUESSER)
                {

                    player.Score += 2;

                }
                else
                {
                    var playerWord = GameInfo.Words.Find(word => word.PlayerId == player.ConnectionId);

                    if (playerWord is { IsVisible: true })
                    {

                        player.Score++;

                    }

                }
            }
        }

        public void AddWord(string text, string playerId)
        {
            GameInfo.Words.Add(new Word(text, playerId, GameInfo.Words.Count));

            // Если все активные игроки, и игрок которые афк не более 1 раунда
            // написали свое слово, то мы сразу переходим к следующему этапу
            var notDisconnectedCount = GameInfo.PlayersList.Select(elem => elem.RoundsDisconnected).Count(num => num == 0);

            var guesser = GameInfo.PlayersList.Find(player => player.Role == PLAYER_ROLE.GUESSER);

            if (notDisconnectedCount - (guesser == null || guesser.RoundsDisconnected > 0 ? 0 : 1) == GameInfo.Words.Count)
            {

                CurrentTime = 0;

            }

        }

        public void ChangeWordState(int wordId, string changerId)
        {

            var word = GameInfo.Words.Find(word => word.WordId == wordId);
            word.IsVisible = !word.IsVisible;
            word.ChangerId = changerId;

        }

        public void ChangeGameState()
        {

            GameInfo.GameState = GameInfo.GameState == GAME_STATE.ONGOING ? GAME_STATE.PAUSED : GAME_STATE.ONGOING;

        }

        private void StopGame()
        {
            GameInfo.GameState = GAME_STATE.PAUSED;

            foreach (var player in GameInfo.PlayersList)
            {

                player.Score = 0;

            }

            GameInfo.Words.Clear();
        }

        private bool CheckWinner()
        {
            var winners = GameInfo.PlayersList.FindAll(player => player.Score >= GameInfo.ScoreToWin);

            if (winners.Count == 1)
            {

                return true;

            }

            var topScore = winners.Select(winner => winner.Score).Prepend(0).Max();

            return winners.FindAll(winner => winner.Score == topScore).Count == 1;

        }

        private void NewWordToGuess()
        {
            
            var wordTask = _wordsController.GetWord(_lobbyId, _wordsCategory);

            wordTask.Wait();

            var newWordToGuess = wordTask.Result;

            GameInfo.WordToGuess = newWordToGuess;

        }


        private void NextGuesser()
        {
            var hasGuesser = GameInfo.PlayersList.Any(playerElem => playerElem.Role == PLAYER_ROLE.GUESSER);

            if (!hasGuesser)
            {

                GameInfo.PlayersList[0].Role = PLAYER_ROLE.GUESSER;
                return;

            }

            hasGuesser = false;

            var currentIndex = GameInfo.PlayersList.FindIndex(elem => elem.Role == PLAYER_ROLE.GUESSER);

            GameInfo.PlayersList[currentIndex].Role = PLAYER_ROLE.SUGGESTER;

            // Добавить проверку на конец игры, если осталось меньше 3 игроков.
            // Наверное должно быть тут, что бы у оставшегося отгадывать слово был шанс доиграть хотя бы его ход
            
            while (!hasGuesser)
            {

                currentIndex = currentIndex + 1 == GameInfo.PlayersList.Count ? 0 : currentIndex + 1;

                if (GameInfo.PlayersList[currentIndex].Disconnected) continue;
                {

                    GameInfo.PlayersList[currentIndex].Role = PLAYER_ROLE.GUESSER;

                }

                hasGuesser = true;
            }
        }

        private void TimerTick(object source/*, ElapsedEventArgs e*/)
        {

            if (GameInfo.GameState == GAME_STATE.PAUSED)
            {

                return;

            }

            CurrentTime -= 1;
            _updateTreshold -= 1;
            _timeStatUpdateThreshold -= 1;

            if (_updateTreshold <= 0)
            {

                _updateTreshold = _baseUpdateThreshold;
                _currentTimeUpdater(_lobbyId, CurrentTime);
                //_hub.lientsReceiveTimerValue(_lobbyId);

            }

            if (_timeStatUpdateThreshold <= 0)
            {

                _timeStatUpdateThreshold = _timeStatBaseUpdateThreshold;
                AddPlayTime();

            }

            if (CurrentTime <= 0)
            {

                switch (GameInfo.CurrentPhase)
                {

                    case GAME_PHASE.SUGGEST:
                        CurrentTime = VerifyTIme;
                        GameInfo.CurrentPhase = GAME_PHASE.VERIFY;
                        GameInfo.Words = GameInfo.Words.OrderBy(item => _rand.Next()).ToList();
                        break;

                    case GAME_PHASE.VERIFY:
                        if (GameInfo.Words.Select(elem => elem.IsVisible).ToList().Count() == 0)
                        {

                            CurrentTime = AftermathTime;
                            GameInfo.CurrentPhase = GAME_PHASE.AFTERMATH;

                        }
                        else
                        {

                            CurrentTime = DecideTime;
                            GameInfo.CurrentPhase = GAME_PHASE.DECIDE;

                        }
                        break;

                    case GAME_PHASE.DECIDE:
                        CurrentTime = AftermathTime;
                        GameInfo.CurrentPhase = GAME_PHASE.AFTERMATH;
                        if (CheckWinner())
                        {

                            SaveStaistics();
                            StopGame();

                        }
                        break;

                    case GAME_PHASE.AFTERMATH:

                        if (GameInfo.PlayersList.Select(player => player.Disconnected).ToList().Count(elem => elem == false) <= 1)
                        {
                            GameInfo.GameState = GAME_STATE.PAUSED;
                        }
                        else
                        {

                            foreach (var playerElem in GameInfo.PlayersList.Where(playerElem => playerElem.Disconnected))
                            {

                                playerElem.RoundsDisconnected++;

                            }

                            GameInfo.Words.Clear();
                            NextGuesser();
                            NewWordToGuess();
                            GameInfo.GuessAttempt = null;
                            GameInfo.CurrentPhase = GAME_PHASE.SUGGEST;
                            CurrentTime = SuggestTime;

                        }

                        break;

                    default:
                        break;

                }
               
                _frameUpdaterAction(_lobbyId);
                _baseTimeUpdater(_lobbyId, CurrentTime);
                //_hub.ClientsReceiveGameFrame(_lobbyId);

            }
        }
    
        private void AddPlayTime()
        {
            foreach (var player in GameInfo.PlayersList)
            {

                if (!player.Disconnected || player.Id != null)
                {

                    var statData = new StatisticsField();

                    statData.StatMesure = "мин.";
                    statData.StatText = "Времени в игре";
                    statData.StatType = typeof(int);
                    statData.StatValue = 1;

                    Console.Write($"Stat value : {statData.StatValue}");

                    _warpper.UpdateGlobalStatistics("Just One", "PlayTime", statData);
                    _warpper.UpdateUserStatistics(player.Id, "Just One", "PlayTime", statData);

                }
            }
        }

        private async Task SaveStaistics()
        {

            foreach (var player in GameInfo.PlayersList)
            {

                if (player.Id == null)
                {

                    continue;

                }

                var statData = new StatisticsField();

                statData.StatMesure = "очк.";
                statData.StatText = "Всего очков получено";
                statData.StatType = typeof(int);
                statData.StatValue = player.Score;

                await _warpper.UpdateUserStatistics(player.Id, "Just One", "Score", statData);
                await _warpper.UpdateGlobalStatistics("Just One", "Score", statData);

                statData.StatMesure = "игр";
                statData.StatText = "Всего игр сыграно";
                statData.StatType = typeof(int);
                statData.StatValue = 1;

                await _warpper.UpdateUserStatistics(player.Id, "Just One", "GamesPlayed", statData);
                await _warpper.UpdateGlobalStatistics("Just One", "GamesPlayed", statData);

                statData.StatMesure = "";
                statData.StatText = "Всего побед";
                statData.StatType = typeof(int);
                statData.StatValue = player.Score == GameInfo.PlayersList.Select(player => player.Score).Max() ? 1 : 0;

                await _warpper.UpdateUserStatistics(player.Id, "Just One", "TotalWins", statData);

            }
        }
    }
}
