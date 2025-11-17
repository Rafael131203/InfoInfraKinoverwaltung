using Microsoft.AspNetCore.Components;
using KinoAppWeb.Services;

namespace KinoAppWeb.Pages
{
    public class DashboardBase : ComponentBase
    {
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected UserSession Session { get; set; } = default!;

        protected bool _loaded;
        protected bool _isAuthenticated;
        protected bool _logoutBusy;

        protected override async Task OnInitializedAsync()
        {
            // Load stored session once
            await Session.InitializeAsync();

            _isAuthenticated = Session.IsAuthenticated;
            _loaded = true;
            _logoutBusy = false;

            if (!_isAuthenticated)
            {
                Nav.NavigateTo("/login", forceLoad: true);
            }
        }

        protected async Task OnLogout()
        {
            if (_logoutBusy) return;

            _logoutBusy = true;
            StateHasChanged();

            await Session.LogoutAsync();

            Nav.NavigateTo("/login", forceLoad: true);

            _logoutBusy = false;
        }
    }
}
