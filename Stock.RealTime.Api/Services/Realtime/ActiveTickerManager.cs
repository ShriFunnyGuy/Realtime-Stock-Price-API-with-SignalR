using System.Collections.Concurrent;

namespace Stock.RealTime.Api.Services.Realtime
{
    public class ActiveTickerManager
    {
        private readonly ConcurrentBag<string> _ativeTicker = [];
        public void AddTicker(string ticker)
        {
            if (!_ativeTicker.Contains(ticker))
            {
                _ativeTicker.Add(ticker);
            }
        }
        public IReadOnlyCollection<string> GetAllTicker()
        {
            return _ativeTicker.ToArray();
        }
    }
}
