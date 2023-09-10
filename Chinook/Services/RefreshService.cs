using Chinook.Services.Contracts;

namespace Chinook.Services
{
    public class RefreshService : IRefreshService
    {
        public event Action? RefreshRequested;

        public void CallRequestRefresh()
        {
            RefreshRequested?.Invoke();
        }
    }
}
