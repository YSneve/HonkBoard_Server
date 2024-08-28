using System.Net;
using HonkBoard_Backend.Core;
using HonkBoard_Backend.Core.Controller.Lobby;
using HonkBoard_Backend.Core.Games;
using HonkBoard_Backend.Core.Structures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HonkBoard_Backend.Core.Controller

{
    /*!
     *\brief Апишка для регистрации, получения кода и прочей информации о пользователе
     * \note путь до апи: /api/honk-board
     * IP : 89.31.35.68
     * PORT : 4090
     */
    [ApiController]
    [Route("api/honk-board")]
    public class ApiController : ControllerBase
    {
        private readonly IDataAccess _access;
        private readonly LobbyUsersHandler _lobbyHandler;
        private readonly ConnectionsHandler _connectionsHandler;

        public ApiController(IDataAccess access, LobbyUsersHandler lobbyHandler, ConnectionsHandler connectionsHandler)
        {

            _access = access;
            _lobbyHandler = lobbyHandler;
            _connectionsHandler = connectionsHandler;

        }

        /*!
         * \brief Проверка на наличие пользователя в бд.
         * \param googleId проверка зарегистрирован ли пользователь в бд
         * \returns success = true/false или код 500 в случае ошибки на сервере
         * \note [HttpGet("/is-registered")]
         */
        [HttpGet("is-registered")]
        public async Task<IActionResult> IsRegistered(string googleId)
        {
            try
            {
                var isRegistered = await _access.IsRegistered(googleId);
                return Ok(new
                {
                    success = isRegistered
                });
            }

            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);

            }
        }


        /*!
         * \brief Получение данных о пользователе, находящегося в бд
         * \param googleId ID искомого пользователя.
         * \returns { Данные в структуре User.cs } или статус код 500 в случае ошибки на сервере
         * \note [HttpGet("/get-user-info")]
         */
        [HttpGet("get-user-info")]
        public async Task<IActionResult> GetUserInfo(string googleId)
        {
            try
            {

                var user = await _access.GetUser(googleId);

                if (user.IsEmpty())
                {
                    return NotFound();
                }

                return Ok(user);

            }

            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);

            }
        }

        /*!
         * \brief Проведение регистрации пользователя
         * \param user Данные в формате \ref User.cs.
         * \returns { Данные в структуре User.cs } или код 500 в случае ошибки на сервере
         * \note [HttpPost("/register-user")]
         */
        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {
            try
            {
                var databaseUser = await _access.WriteInfo(user);

                return Ok(databaseUser);

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);

            }
        }

        /*!
         * \brief Загрузка аватара пользователя
         * \param googleId идентификатор пользователя
         * \param imageFile файл медиаконтента в теле запроса в виде формы
         * \returns string ссылку на аватар или код 500 в случае ошибки на сервере (например отсутсвие ответа от сервера загрузки иозбражений)
         * \note [HttpPost("/upload-avata")]
         */
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(string googleId, [FromForm] IFormFile imageFile)
        {
            try
            {
               
                var imageUrl = await _access.PostImage(googleId, imageFile);

                return Ok(imageUrl);

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);

            }
        }

        /*!
         * \brief Патч (обновление) информации о пользователе.
         * \param user информация о пользователе в структуре \ref User.cs
         * \returns { Данные в структуре User.cs } код 200 в случае успеха, код 500 в случае ошибки на сервере
         * \note [HttpPatch("/update-user-info")]
         */
        [HttpPatch("update-user-info")]
        public async Task<IActionResult> UpdateUserInfo([FromBody] User user)
        {
            try
            {
                await _access.PatchUser(user);

                return Ok(user);
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);

            }
        }


        /*!
         * \brief Проверка создана ли комната с некоторым id
         * \param lobbyId строка, указывающая код комнаты
         * \returns success = true или false
         * \note [HttpGet("/is-room-created")]
         */
        [HttpGet("is-room-created")]
        public async Task<IActionResult>IsRoomCreated(string lobbyId)
        {
            try
            {
                var isCreated = await _lobbyHandler.IsCreated(lobbyId);

                return Ok(new {
                    success = isCreated
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);

            }
        }


        /*!
         * \brief Ищет был ли пользователь подключен куда-либо и возрващает сслыку либо null
         * \param lobbyId строка, указывающая код комнаты
         * \param lastConnectionId строка, указывающая последний id при подключении
         * \returns link = string или link = nul, статус код 500 в случае ошибки на сервере
         * \note [HttpGet("/was-connected")]
         */
        [HttpGet("was-connected")]
        public async Task<IActionResult> WasConnected(string lobbyId, string lastConnectionId)
        {
            try
            {

                var connectionLink = await _connectionsHandler.HasConnectedUser(lobbyId, lastConnectionId);

                return Ok(new
                {
                    link = connectionLink
                });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);

            }
        }
    }
}
