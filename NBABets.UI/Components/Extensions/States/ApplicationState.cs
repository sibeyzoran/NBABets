using NBABets.Models;

namespace NBABets.UI.Components
{
    public class ApplicationState
    {
        public event Action? OnStateChange;
        private void NotifyStateChanged() => OnStateChange?.Invoke();

        public UserDto? CurrentUser { get; set; } = null;
        public void SetCurrentUser(UserDto? currentUser)
        {
            CurrentUser = currentUser;
            NotifyStateChanged();
        }

    }
}
