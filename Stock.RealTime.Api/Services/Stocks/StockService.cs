using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Stock.Models;
using Stock.RealTime.Api.Services.Realtime;

namespace Stock.RealTime.Api.Services.Stocks
{
    public class StockService : IStockService
    {
        private readonly IStocksClient _stocksClient;
        private readonly ILogger<StockService> _logger;
        private readonly NpgsqlDataSource _dataSource;
        private readonly ActiveTickerManager _activeTickerManager;
        private readonly IConfiguration _configuration;

        public StockService(ILogger<StockService> logger, NpgsqlDataSource dataSource, ActiveTickerManager activeTickerManager, IStocksClient stocksClient, IConfiguration configuration)
        {
            _logger = logger;
            _dataSource = dataSource;
            _activeTickerManager = activeTickerManager;
            _stocksClient = stocksClient;
            _configuration = configuration;
        }

        public async Task<StockPriceResponse?> GetLatestPriceAsync(string ticker, CancellationToken cancellationToken = default)
        {
            // First, try to get the latest price from the database
            StockPriceResponse? dbPrice = await GetLatestPriceFromDatabase(ticker);
            if (dbPrice is not null)
            {
                _activeTickerManager.AddTicker(ticker);
                return dbPrice;
            }

            // If not found in the database, fetch from the external API
            StockPriceResponse? apiPrice = await _stocksClient.GetDataForTicker(ticker, cancellationToken);

            if (apiPrice == null)
            {
                _logger.LogWarning("No data returned from external API for ticker: {Ticker}", ticker);
                return null;
            }

            // Save the new price to the database
            await SavePriceToDatabase(apiPrice);

            _activeTickerManager.AddTicker(ticker);

            return apiPrice;
        }

        private async Task<StockPriceResponse?> GetLatestPriceFromDatabase(string ticker)
        {
            const string sql =
                """
                SELECT ticker, price, timestamp
                FROM public.stock_prices
                WHERE ticker = @Ticker
                ORDER BY timestamp DESC
                LIMIT 1
                """;

            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();
            StockPriceRecord? result = await connection.QueryFirstOrDefaultAsync<StockPriceRecord>(sql, new
            {
                Ticker = ticker
            });

            if (result is not null)
            {
                return new StockPriceResponse
                {
                    Ticker = result.Ticker,
                    Price = result.Price
                };
            }

            return null;
        }

        private async Task SavePriceToDatabase(StockPriceResponse price)
        {
            const string sql =
                """
                INSERT INTO public.stock_prices (ticker, price, timestamp)
                VALUES (@Ticker, @Price, @Timestamp)
                """;

            await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();
            await connection.ExecuteAsync(sql, new
            {
                price.Ticker,
                price.Price,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
