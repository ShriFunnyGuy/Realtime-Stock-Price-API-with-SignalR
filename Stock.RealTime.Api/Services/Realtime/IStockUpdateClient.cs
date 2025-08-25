using Stock.Models;

namespace Stock.RealTime.Api.Services.Realtime
{
    public interface IStockUpdateClient
    {
        Task ReceivedStockPriceUpdate(StockPriceUpdate stockPriceUpdate);
    }
}
