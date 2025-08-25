using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stock.Models;
using System.Globalization;
using System.Net.Http;

namespace Stock.RealTime.Api.Services.Stocks
{
    public class StocksClient : IStocksClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StocksClient> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache memoryCache;

        public StocksClient(HttpClient httpClient, ILogger<StocksClient> logger, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            this.memoryCache = memoryCache;
        }

        public async Task<StockPriceResponse?> GetDataForTicker(string ticker, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting stock price information for {Ticker}", ticker);

                StockPriceResponse? stockPriceResponse = await memoryCache.GetOrCreateAsync($"stocks-{ticker}", async entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                    return await GetStockPrice(ticker, cancellationToken);
                });

                if (stockPriceResponse is null)
                {
                    _logger.LogWarning("Failed to get stock price information for {Ticker}", ticker);
                }
                else
                {
                    _logger.LogInformation(
                        "Completed getting stock price information for {Ticker}, {@Stock}",
                        ticker,
                        stockPriceResponse);
                }

                return stockPriceResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching data for ticker {Ticker}", ticker);
                return null;
            }
        }

        private async Task<StockPriceResponse?> GetStockPrice(string ticker, CancellationToken cancellationToken)
        {
            string tickerDataString = await _httpClient.GetStringAsync(
                $"?function=TIME_SERIES_INTRADAY&symbol={ticker}&interval=15min&apikey={_configuration["Stocks:ApiKey"]}",
                cancellationToken);

            AlphaVantageData? tickerData = JsonConvert.DeserializeObject<AlphaVantageData>(tickerDataString);

            TimeSeriesEntry? lastPrice = tickerData?.TimeSeries.FirstOrDefault().Value;

            if (lastPrice is null)
            {
                return null;
            }

            return new StockPriceResponse
            {
                Ticker = ticker,
                Price = decimal.Parse(lastPrice.High, CultureInfo.InvariantCulture)
            };
        }
    }
}
