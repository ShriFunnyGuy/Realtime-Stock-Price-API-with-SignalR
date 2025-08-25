using Microsoft.AspNetCore.Mvc;
using Stock.RealTime.Api.Services.Stocks;

namespace Stock.RealTime.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockPriceController : ControllerBase
    {
        private readonly ILogger<StockPriceController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IStockService _stockService;
        public StockPriceController(ILogger<StockPriceController> logger, IConfiguration configuration, IStockService stockService)
        {
            _logger = logger;
            _configuration = configuration;
            _stockService = stockService;
        }
        [HttpGet("api/stocks/{ticker}")]
        public async Task<IActionResult> GetStockPrice(string ticker, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                return BadRequest("Ticker symbol is required.");
            }
            try
            {
                var stockPrice = await _stockService.GetLatestPriceAsync(ticker, cancellationToken);
                if (stockPrice == null)
                {
                    return NotFound($"No data found for ticker: {ticker}");
                }
                return Ok(stockPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stock price for ticker: {Ticker}", ticker);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
