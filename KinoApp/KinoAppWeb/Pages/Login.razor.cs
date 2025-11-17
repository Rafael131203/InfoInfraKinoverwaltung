using KinoAppShared.DTOs.Authentication;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;

namespace KinoAppWeb.Pages
{
    public partial class Login
    {
        private string _email = string.Empty;
        private string _password = string.Empty;

        private bool _isBusy;
        private string? _errorMessage;
        private int _clickCount;

        [Inject] private IClientLoginService Auth { get; set; } = default!;
        [Inject] private UserSession Session { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private async Task OnSubmit()
        {
            _clickCount++;
            _errorMessage = null;
            _isBusy = true;

            Console.WriteLine($"[Login] Click #{_clickCount}, Email={_email}, PwLen={_password.Length}");

            try
            {
                if (string.IsNullOrWhiteSpace(_email) || string.IsNullOrWhiteSpace(_password))
                {
                    _errorMessage = "Email und Passwort dürfen nicht leer sein.";
                    return;
                }

                var request = new LoginRequestDTO
                {
                    Email = _email,
                    Passwort = _password
                };

                var response = await Auth.LoginAsync(request, CancellationToken.None);

                if (response is null)
                {
                    _errorMessage = "Ungültige Login-Daten.";
                    return;
                }

                await Session.SetSessionAsync(response);
                Nav.NavigateTo("/dashboard");
            }
            catch (Exception ex)
            {
                _errorMessage = $"Fehler beim Login: {ex.Message}";
            }
            finally
            {
                _isBusy = false;
                StateHasChanged();
            }
        }
    }
}
