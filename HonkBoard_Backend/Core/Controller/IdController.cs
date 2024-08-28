using Microsoft.Extensions.Options;

namespace HonkBoard_Backend.Core.Controller
{
    public class IdController
    {
        private static List<int> reservedIds = new();
        private static readonly Random _random = new();
        private const int maxId = 100000;
        private const int minId = 10000;

        public static int GetId()
        {
            var id = _random.Next(minId, maxId);

            while (reservedIds.Contains(id))
            {
                id = _random.Next(minId, maxId);
            }

            reservedIds.Add(id);
            return id;
        }

        public static void RemoveIdFormReserve(int id)
        {
            reservedIds.Remove(id);
        }
    }
}
