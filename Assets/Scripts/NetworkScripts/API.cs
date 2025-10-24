using System.Collections.Generic;
using ManagerScripts;

namespace NetworkScripts
{
    public static class API
    {
        public static Networking.Get<Void> Log(string message)
        {
            return new Networking.Get<Void>($"/debug/log?message={message}");
        }

        public static Networking.Get<List<PlayerStatus>> GetPlayerData()
        {
            return new Networking.Get<List<PlayerStatus>>("/api/game/player-status");
        }

        public static Networking.Get<StockStatus> GetStockData()
        {
            return new Networking.Get<StockStatus>("/api/game/stock-status");
        }
    }
}