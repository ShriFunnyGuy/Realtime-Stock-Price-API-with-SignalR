
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Stock.Models;
using Stock.RealTime.Api.Services.Stocks;

namespace Stock.RealTime.Api.Services.Realtime
{
    public class StockFeedUpdater(
        ActiveTickerManager activeTickerManager,
        ILogger<StockFeedUpdater> logger,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<StocksFeedHub,IStockUpdateClient> hubContext,
        IOptions<StockUpdateOptions> options
        ) : BackgroundService
    {
        private readonly Random _random = new();
        private readonly StockUpdateOptions _options = options.Value;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateStockPrices();
                await Task.Delay(_options.UpdateInterval, stoppingToken);
            }
        }

        private async Task UpdateStockPrices()
        {
            using IServiceScope serviceScope = serviceScopeFactory.CreateScope();
            IStockService stockService = serviceScope.ServiceProvider.GetRequiredService<IStockService>();

            foreach (string ticker in activeTickerManager.GetAllTicker())
            {
                StockPriceResponse? currentPrice = await stockService.GetLatestPriceAsync(ticker);
                if (currentPrice is null)
                {
                    logger.LogWarning("No latest price found for ticker {Ticker}, skipping update.", ticker);
                    continue;
                }
                //decimal newPrice = Math.Round((decimal)(_random.NextDouble() * 1000), 2);
                //var stockPriceUpdate = new StockPriceUpdate
                //{
                //    Ticker = ticker,
                //    Price = newPrice,
                //    Timestamp = DateTime.UtcNow
                //};
                //logger.LogInformation("Broadcasting update for {Ticker}: {Price} at {Timestamp}", ticker, newPrice, stockPriceUpdate.Timestamp);
                //await hubContext.Clients.All.ReceivedStockPriceUpdate(stockPriceUpdate);
                decimal newPrice = CalculateNewPrice(currentPrice);
                var stockPriceUpdate = new StockPriceUpdate
                {
                    Ticker = ticker,
                    Price = newPrice
                };
                // This sends the update to all connected clients which is not efficient for a large number of clients
                // Consider using groups or other strategies for scalability those clients interested in specific tickers
                //await hubContext.Clients.All.ReceivedStockPriceUpdate(stockPriceUpdate);

                await hubContext.Clients.Group(ticker).ReceivedStockPriceUpdate(stockPriceUpdate);
                logger.LogInformation("Broadcasting update for {Ticker}: {Price}", ticker, newPrice);

            }
        }
        private decimal  CalculateNewPrice(StockPriceResponse currentPrice)
        {
            double change = _options.MaxPercentageChange;
            decimal priceFactor = (decimal)(1 + (_random.NextDouble() * 2 * change - change));
            decimal priceChange = currentPrice.Price * priceFactor;
            decimal newPrice = Math.Max(0,currentPrice.Price + priceChange);
            newPrice = Math.Round(newPrice, 2);
            return newPrice;
        }
    }
}
