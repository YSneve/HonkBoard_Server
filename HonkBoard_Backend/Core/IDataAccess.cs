using System.Net;
using HonkBoard_Backend.Core.Structures;

namespace HonkBoard_Backend.Core
{

    public interface IDataAccess
    {
        public Task<bool> IsRegistered(string googleId);
        
        public  Task<User> WriteInfo(User user);

        public Task<HttpStatusCode> PatchUser(User user);

        public Task<User> GetUser(string googleId);

        public Task<string> PostImage(string googleId, IFormFile image);

        public Task UpdateUserStatistics(string googleId, string gameName, string statName, StatisticsField statData);
        public Task UpdateGlobalStatistics(string gameName, string statName, StatisticsField statData);
    }
}