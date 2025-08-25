using Microsoft.AspNetCore.SignalR;

namespace Stock.RealTime.Api.Services.Realtime
{
    public class StocksFeedHub:Hub<IStockUpdateClient>
    {
        public override async Task OnConnectedAsync()
        {
            
        }
        public async Task SubscribeToTicker(string ticker)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
        }
        public async Task UnsubscribeFromTicker(string ticker)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticker);
        }
    }
}
