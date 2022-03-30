using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Service.Liquidity.TradingPortfolio.Cache
{
    public interface IManualTradeCacheStorage
    {
        void Add(string id, ManualTradeCacheElement bridge);
        List<KeyValuePair<string, ManualTradeCacheElement>> GetAll();
        void CleanUp();
    }

    public class ManualTradeCacheStorage : IManualTradeCacheStorage
    {
        private ConcurrentDictionary<string, ManualTradeCacheElement> _data = new ConcurrentDictionary<string, ManualTradeCacheElement>();

        public ManualTradeCacheStorage(IReadOnlyCollection<(string, ManualTradeCacheElement)> pairs)
        {
            foreach (var pair in pairs)
            {
                Add(pair.Item1, pair.Item2);
            }
        }

        public void Add(string id, ManualTradeCacheElement bridge)
        {
            _data[id] = bridge;
        }

        public List<KeyValuePair<string, ManualTradeCacheElement>> GetAll()
        {
            return _data.ToList();
        }

        public ManualTradeCacheElement Get(string id)
        {
            if (_data.TryGetValue(id, out var element))
            {
                return element;
            }

            return null;
        }

        public void CleanUp()
        {
            var olderThen = DateTime.UtcNow.AddMinutes(-20);
            foreach (var kvp in _data.Where(b => b.Value.Date < olderThen))
            {
                // Please note that by now the frame may have been already
                // removed by another thread.
                _data.TryRemove(kvp.Key, out var ignored);
            }
        }
    }
}
