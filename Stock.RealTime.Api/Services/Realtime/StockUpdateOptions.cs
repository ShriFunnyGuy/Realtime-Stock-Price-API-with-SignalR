namespace Stock.RealTime.Api.Services.Realtime
{
    public class StockUpdateOptions
    {
        public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(5);
        public double MaxPercentageChange { get; set; } = 0.02;
    }
}
