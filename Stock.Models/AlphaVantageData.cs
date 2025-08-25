using Newtonsoft.Json;

namespace Stock.Models
{
    public class AlphaVantageData
    {
        [JsonProperty("Meta Data")]
        public MetaData MetaData { get; set; }
        [JsonProperty("Time Series (15min)")]
        public Dictionary<string, TimeSeriesEntry> TimeSeries { get; set; }
    }
}
