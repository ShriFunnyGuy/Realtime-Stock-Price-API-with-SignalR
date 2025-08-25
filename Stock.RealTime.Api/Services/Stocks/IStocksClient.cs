using Stock.Models;

namespace Stock.RealTime.Api.Services.Stocks
{
    public interface IStocksClient
    {
        Task<StockPriceResponse?> GetDataForTicker(string ticker, CancellationToken cancellationToken = default);
    }
}
