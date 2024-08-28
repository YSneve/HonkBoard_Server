using HonkBoard_Backend.Core.Structures;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.ComponentModel;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace HonkBoard_Backend.Core.Controller.Lobby
{

    /*!
     * \brief Класс лобби. К нему подключаются хост и остальные игроки для выбора игры из списка и их конфигурирования.
     */
    public class LobbySocket : Hub
    {
        private readonly LobbyUsersHandler _usersHandler;
        private readonly ILogger<LobbySocket> _logger;
        public LobbySocket(LobbyUsersHandler handler, ILogger<LobbySocket> logger)
        {
            _usersHandler = handler;
            _logger = logger;
        }
        /*!
         * \brief Метод выбора игр, путем передачи gameId
         * \param gameId идентификатор (номер в массиве игр) некоторой игры
         */
        public async Task SelectGame(int gameId)
        {
            var context = Context;

            var lobbyId = _usersHandler.GetLobbyId(context.ConnectionId);

            _usersHandler.SelectGame(lobbyId, gameId);

            await ClientsReceiveSelectedGame(lobbyId);
        }

        /*!
         * \brief Функция подключения к созданному ранее лобби.
         * \param lobbyId идентификатор комнаты.
         * \param user Информация о пользвателе, подключившемся к лобби в структуре User.cs
         * \returns result = {connectionId, string}
         */
        public async Task<ClientInfoWrapper> JoinLobby(string lobbyId, User user)
        {

            var context = Context;

            await Groups.AddToGroupAsync(context.ConnectionId, lobbyId);

            _usersHandler.AddUserToLobby(context.ConnectionId, lobbyId, user) ;

            // Апдейт всем списка пользователей
            _ = ClientsReceiveUsersList(lobbyId);
            _ = ClientsReceiveSelectedGame(lobbyId);


            var result = new ClientInfoWrapper
            {
                result = new Dictionary<string, string>{
                    {"connectionId", context.ConnectionId}
                }
            };

            _logger.LogDebug($"USER JOINED ROOM | {DateTime.Now} -  IP : {context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress} " +
                $"RID: {lobbyId} " +
                $"P : {_usersHandler.GetUsersList(lobbyId).Count}");

            return result;
        }

        /*!
         * \brief Функция создания и подключения к нему.
         * \param user Информация о пользователе в стркутуре User.cs.
         * \returns result = {lobbyId, string}, {connectionId, string}
         */
        public async Task<ClientInfoWrapper> CreateLobby(User user)
        {
            var context = Context;

            var lobbyId = IdController.GetId().ToString();

            await Groups.AddToGroupAsync(context.ConnectionId, lobbyId);

            _usersHandler.AddUserToLobby(context.ConnectionId, lobbyId, user);

            await ClientsReceiveUsersList(lobbyId);
            await ClientsReceiveSelectedGame(lobbyId);

            var result = new ClientInfoWrapper {
                result = new Dictionary<string, string>{
                    {"lobbyId", lobbyId}, 
                    {"connectionId", context.ConnectionId}
                }
            };

            _logger.LogDebug(
                $"USER CREATED ROOM | {DateTime.Now} - IP : {context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress} " +
                $"RID : {lobbyId} " +
                $"P : {_usersHandler.GetUsersList(lobbyId).Count} ");

            return result;
        }

        /*!
         * \brief Функция для старта игры, рассылает всем пользователям в лобби уведомление о начала, путем вызова функции StartGame у клиентов. Ничего не передает.
         * \param parameters игровые параметры в виде List<ServerGameParameter.cs>
         */
        public async Task StartGame(ClientInfoWrapper wrappedParams)
        {
            var lobbyId = _usersHandler.GetLobbyId(Context.ConnectionId);

            var parameters = JsonConvert.DeserializeObject<List<ServerGameParameter>>(wrappedParams.result.ToString());

            _usersHandler.StartGame(Context.ConnectionId, parameters);
            
            await ClientsReceiveStartGame(lobbyId);

            _logger.LogDebug($"GAME STARTED | {DateTime.Now} -  RID : {lobbyId} P : {_usersHandler.GetUsersList(lobbyId).Count}");
        }

        /*!
         * \brief Функция для отключения от лобби при выходе игрока в главное меню
         */
        public async Task DisconnectFromLobby()
        {
            
            var context = Context;
            
            await DisconnectUser(context.ConnectionId);

            _logger.LogDebug($"LOBBY DISCONNECT | {DateTime.Now} - IP : {Context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress} " +
                $"С : {_usersHandler.TotalConnectedParticipants()}");

        }


        /*!
         * \brief Метод получения клиентом int gameId. Вызывается при выборе игры хостом у ВСЕХ клиеннтов в текущем лобби (включая хоста).
         * \returns result = int
         */
        public async Task ClientsReceiveSelectedGame(string lobbyId)
        {
            var gameId = _usersHandler.GetSelectedGame(lobbyId);

            var result = new ClientInfoWrapper { result = gameId };

            await Clients.Groups(lobbyId).SendAsync("ReceiveSelectedGame", result);
        }

        /*!
         * \brief Метод получения клиентом List<Participant.cs>. Вызывается при изменение числа игроков у ВСЕХ клиеннтов в текущем лобби (включая только что подключившегося).
         * \returns result = List<Participant.cs>
         */
        public async Task ClientsReceiveUsersList(string lobbyId)
        {
            var connectedUsers = _usersHandler.GetUsersList(lobbyId);

            var result = new ClientInfoWrapper {result = connectedUsers};

            await Clients.Group(lobbyId).SendAsync("ReceiveUsersList", result);
        }

        /*!
         * \brief Метод, вызывающий у клиента начало подключения к сокету выбранной игры. Вызывается при старте игры хостом у ВСЕХ клиеннтов в текущем лобби (включая хоста).
         * \note Ничего не передает
         * \returns Empty
         */
        public async Task ClientsReceiveStartGame(string lobbyId)
        {
            await Clients.Groups(lobbyId).SendAsync("ReceiveStartGame", DateTime.Now.ToString());
        }

        /*!
         * \brief Метод получения клиентом Dictionary<int, GameInfo>.
         * Вызывается при подключении к сокету (до подключения к лобби) только у подключившегося пользователя
         * \returns result = List<GameInfo.cs>
         */
        public async Task ClientsReceiveGamesList(ISingleClientProxy caller)
        {
            var gamesList = _usersHandler.GamesList;

            var result = new ClientInfoWrapper { result = gamesList };

            await caller.SendAsync("ReceiveGamesList", result);

            _logger.LogDebug($"SENDING GAMELIST | {DateTime.Now} - G : {gamesList.Count}");
        }

        /*!
         * \brief Функция, вызываемя автоматически, при отключения пользователя от сокета.
         */
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var context = Context;

            _ = DisconnectUser(context.ConnectionId);

            _logger.LogDebug($"LOBBY SOCKET DISCONNECTED | {DateTime.Now} - IP : {Context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress} " +
                $"ConID : {context.ConnectionId}");

            return base.OnDisconnectedAsync(exception);
        }

        /*!
         * \brief Функция, вызываемая автоматически при подключении пользователя к сокету.
         */
        public override Task OnConnectedAsync()
        {
            var context = Context;
            var caller = Clients.Caller;

            ClientsReceiveGamesList(caller);

            _logger.LogDebug($"LOBBY SOCKET CONNECTED | {DateTime.Now} - IP : {context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress} " +
                $"ConId : {context.ConnectionId}");

            return base.OnConnectedAsync();
        }

        private async Task DisconnectUser(string connectionId)
        {
            // Проверяем, подключался ли пользватель к лобби.
            if (_usersHandler.IsInLobby(connectionId))
            {
                var lobbyId = _usersHandler.GetLobbyId(connectionId);

                var isHost = _usersHandler.IsUserHost(Context.ConnectionId);
                var isStarted = _usersHandler.IsGameStarted(Context.ConnectionId);
                var connectedUsersCount = _usersHandler.GetUsersList(lobbyId).Count;

                _usersHandler.RemoveUserFromLobby(Context.ConnectionId);

                if (connectedUsersCount == 1)
                {
                    _usersHandler.RemoveLobby(lobbyId);
                }
                else if (!isStarted && isHost)
                {
                    _usersHandler.TransferHost(lobbyId);
                    await ClientsReceiveUsersList(lobbyId);
                }
            }
        }
    }
}
