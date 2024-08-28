using HonkBoard_Backend.Core.Games.JustOne.Structures;

namespace HonkBoard_Backend.Core.Controller
{
    public class WordsController
    {

        private readonly DatabaseWrapper _wrapper;

        private readonly Dictionary<string, List<int>> _gameUsedWords;

        private readonly Random _random;

        public WordsController(DatabaseWrapper wrapper)
        {
            _wrapper = wrapper; 

            _gameUsedWords = new();

            _random = new (); 
        }

        public void AddLobby(string lobbyid)
        {

            if (!_gameUsedWords.ContainsKey(lobbyid))
            {

                _gameUsedWords.Add(lobbyid, new List<int>());

            }

        }

        public void RemoveLobby(string lobbyid) {
        
            if (_gameUsedWords.ContainsKey(lobbyid))
            {

                _gameUsedWords.Remove(lobbyid);

            }
        }


        public void ClearUsedWords(string lobbyId)
        {

            if (_gameUsedWords.ContainsKey(lobbyId))
            {

                _gameUsedWords[lobbyId].Clear();

            }

        }

        public async Task<string> GetWord(string lobbyId, string categoryKey) {

            var maxWords = _wrapper.GetMaxWords(categoryKey);

            if (maxWords == 0)
            {

                return "Ошибка категории";

            }

            if (!_gameUsedWords.ContainsKey(lobbyId))
            {

                return "Ошибка контроллера";

            }


            int wordId;
            
            do
            {
                wordId = _random.Next(0, maxWords);

                var word = await _wrapper.GetWord(categoryKey, wordId);

                if (word == null)
                {

                    await _wrapper.UpdateWordsCount(categoryKey);

                    maxWords = _wrapper.GetMaxWords(categoryKey);

                    if (maxWords == 0)
                    {

                        return "Ошибка сборника";

                    }

                }

                else if (word == "")
                {

                    return "Ошибка сборника";

                }

                else if (!_gameUsedWords[lobbyId].Contains(wordId))
                {
                    //Добавление идентификатора слова в исключение из выборки
                    _gameUsedWords[lobbyId].Add(wordId);

                    return word;

                };

            }
            while (_gameUsedWords[lobbyId].Contains(wordId));

            return "Ошибка контроллера";
        
        }

    }
}
