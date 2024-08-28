using HonkBoard_Backend.Core.Controller;
using HonkBoard_Backend.Core.Structures;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Extensions;

namespace HonkBoard_Backend.Core.Games.JustOne
{
   
    /*!
     * \brief Класс, описывающий функции сокета игры Just One доступного по пути 89.31.35.68:4090/just-one
     */
    public class Hub : Microsoft.AspNetCore.SignalR.Hub
    {
        
        private readonly ILogger<Hub> _logger;
        private readonly WordsController _wordsController;
        private readonly HubHandler _hubHandler;
        private readonly IDataAccess _warpper;

        public Hub(IUsersHandler handler, WordsController wrodsController, ILogger<Hub> logger, IDataAccess wrapper)
        {

            _logger = logger;
            _wordsController = wrodsController;
            _warpper = wrapper;

            // Как то не очень. Нужно поправить
            _hubHandler = handler as HubHandler;

        }

        /*!
         * \brief Метод добавления пользвателя в комнату с игрой
         * \param lobbyId идентификатор
         * \param participant объект класса participant.cs
         * \param lastConnectionId айди прошлого подключения, который сохранен для этого лобби. При передаче и успешном нахождении участника в списке игроков,
         * перезаписывает старый айди подключения существовашего элемента на новый.
         * \returns string ID подключения
         */
        public void CreateLobby(string lobbyId, List<ServerGameParameter> parametrs)
        {

            if (!_hubHandler.HasLobby(lobbyId))
            {

                _hubHandler.AddLobby(lobbyId, new GameController(ClientsReceiveGameFrame, ClientsReceiveCurrentTime, ClientsReceiveBaseTime, _warpper, _wordsController, lobbyId, parametrs));

                _logger.LogDebug($"CONTROLLER CREATED | {DateTime.Now} - RID : {lobbyId}");

            }  
            
        }


        /*!
         * \breif Функция подключения к конкретной, после создания подключеня к сокету
         * \params lobbyId лобби, к которому нужно подключить пользователя
         * \param user пользователь, которого подключаем к лобби
         * \lastConnectionId идентификатор подключения, который был в момент прошлого нахождения в данном лобби (опционально). Используется для переподключения.
         * \returns string идентификатор подключения к текущему сокету
         */
        public async Task Join(string lobbyId, User user, string? lastConnectionId = null)
        {

            var context = Context;

            Clients.Caller.SendAsync("ReceiveConnectionId", context.ConnectionId);

            Groups.AddToGroupAsync(context.ConnectionId, lobbyId);

            _hubHandler.AddConnection(context.ConnectionId, lobbyId);

            var gameController = _hubHandler.GetGameController(lobbyId);

            gameController.JoinGame(user, context.ConnectionId, lastConnectionId);

            ClientsReceiveGameFrame(lobbyId);

            _logger.LogDebug($"PLAYER JOINED | {DateTime.Now} - IP : {context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress} " +
                    $"ConID : {context.ConnectionId} " +
                    $"RID {lobbyId} " +
                    $"LConID : {(lastConnectionId == null ? "null" : lastConnectionId)}");

           
        }
        
        /*!
         * \brief Метод паузы/анпаузы игры
         */
        public async Task ChangeGameState()
        {
            var context = Context;

            var lobbyId = _hubHandler.GetLobbyId(context.ConnectionId);

            _hubHandler.GetGameController(lobbyId).ChangeGameState();

            await ClientsReceiveGameFrame(lobbyId);
        }

        /*!
         * \brief Функция для угадывающего для попытки угадать загаданное слово
         * \param word строка, являющаяся вводом угадывающего
         */
        public async Task MakeGuess(string word)
        {

            var context = Context;

            var lobbyId = _hubHandler.GetLobbyId(context.ConnectionId);

            _hubHandler.GetGameController(lobbyId).MakeGuess(word);

            ClientsReceiveGameFrame(lobbyId);

            _logger.LogDebug($"GUESS ADDED | {DateTime.Now} - W : {word}");

        }

        /*!
         * \brief Метод для игрока-подсказчика, позволяющий добавить слово в список подсказок
         * \param word строка, являющаяся вводом игрока-подсказчика
         */
        public async Task AddWord(string word)
        {

            var context = Context;

            var lobbyId = _hubHandler.GetLobbyId(context.ConnectionId);

            _hubHandler.GetGameController(lobbyId).AddWord(word, context.ConnectionId);

            ClientsReceiveGameFrame(lobbyId);

            _logger.LogDebug($"WORD ADDED | {DateTime.Now} - W : {word}");

        }

        /*!
         * \brief Метод для игроков-подсказчиков, позволяющий убрать для отгадывающего отображение некоторого слова
         * \param wordId номер слова в списке
         */
        public async Task ChangeWordState(int wordId)
        {

            var context = Context;

            var lobbyId = _hubHandler.GetLobbyId(context.ConnectionId);

            _hubHandler.GetGameController(lobbyId).ChangeWordState(wordId, context.ConnectionId);

            ClientsReceiveGameFrame(lobbyId);

        }

        public async Task DisconnectFromGame(HubCallerContext? callContext = null)
        {
            var context = Context;

            if (callContext != null)
            {

                context = callContext;

            }

            if (_hubHandler.HasUserConnected(context.ConnectionId))
            {

                var lobbyId = _hubHandler.GetLobbyId(context.ConnectionId);

                _hubHandler.RemoveUser(context.ConnectionId);

                ClientsReceiveGameFrame(lobbyId);

                if (_hubHandler.IsLobbyEmpty(lobbyId))
                {

                    _hubHandler.RemoveLobby(lobbyId);

                }

                _logger.LogDebug($"GAME DISCONNECTED | {DateTime.Now} - IP : {Context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress} " +
                $"ConID : {context.ConnectionId}");
            }
        }

        public override Task OnConnectedAsync()
        {

            var context = Context;

            _logger.LogDebug($"JUST ONE SOCKET CONNECTED | {DateTime.Now} - IP : {Context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress} " +
                $"ConID : {context.ConnectionId}");

            return base.OnConnectedAsync();

        }

        public override Task OnDisconnectedAsync(Exception exception)
        {

            var context = Context;

            DisconnectFromGame(context);

            _logger.LogDebug($"JUST ONE SOCKET DISCONNECTED | {DateTime.Now} - IP : {Context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress} " +
                $"ConID : {context.ConnectionId}");
            
            return base.OnDisconnectedAsync(exception);

        }

        public async Task ClientsReceiveGameFrame(string lobbyId)
        {

            await Clients.Groups(lobbyId).SendAsync("ReceiveGameFrame", _hubHandler.GetGameController(lobbyId).GameInfo);

            var playersInfo = "";
            var players = _hubHandler.GetGameController(lobbyId).GameInfo.PlayersList;

            foreach (var player in players)
            {

                playersInfo += $"\n      PLAYER {player.Name} IS {player.Role.GetDisplayName()} CID : {player.ConnectionId}";

            }

            var info = _hubHandler.GetGameController(lobbyId).GameInfo;

            if (info.WordToGuess == "" || info.WordToGuess == null)
            {

                Console.WriteLine($"\n    Framed word to guess : {_hubHandler.GetGameController(lobbyId).GameInfo.WordToGuess}\n");
            }

            _logger.LogDebug($"SENT FRAME | {DateTime.Now} - P : {_hubHandler.GetGameController(lobbyId).GameInfo.PlayersList.Count} "
                +
                $"PHASE : {_hubHandler.GetGameController(lobbyId).GameInfo.CurrentPhase.GetDisplayName()} " +
                playersInfo
                );
                       
        }

        /*!
         * \brief метод получения клиентами значения текущего времени фазы, путем вызова сервером метода ReceiveCurrentTime у клиентов
         * \returns "currentTime": int, "baseTime": int
         */
        public async Task ClientsReceiveCurrentTime(string lobbyId, int time)
        {

            await Clients.Groups(lobbyId).SendAsync("ReceiveCurrentTime", time);

            //_logger.LogInformation($"SENT TIME | {DateTime.Now} - T: {time}");

        }

        /*!
         * \brief метод получения клиентами значения таймера, путем вызова сервером метода ReceiveBaseTime у клиентов
         * \returns "currentTime": int, "baseTime": int
         */
        public async Task ClientsReceiveBaseTime(string lobbyId, int time)
        {

            await Clients.Groups(lobbyId).SendAsync("ReceiveBaseTime", time);

            //_logger.LogInformation($"SENT TIME | {DateTime.Now} - T: {time}");

        }
    }
}
