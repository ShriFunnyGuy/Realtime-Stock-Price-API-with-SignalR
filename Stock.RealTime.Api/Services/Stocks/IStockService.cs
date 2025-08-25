using Stock.Models;

namespace Stock.RealTime.Api.Services.Stocks
{
    public interface IStockService
    {
        Task<StockPriceResponse?> GetLatestPriceAsync(string ticker, CancellationToken cancellationToken = default);
    }
}
