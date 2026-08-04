using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FRPAMSystem.BusinessTier.SignalR
{
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
    }
}
