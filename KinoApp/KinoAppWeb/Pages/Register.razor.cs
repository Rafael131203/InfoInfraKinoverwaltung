using System.ComponentModel.DataAnnotations;
using KinoAppShared.DTOs.Authentication;
using KinoAppWeb.Services;
using Microsoft.AspNetCore.Components;

namespace KinoAppWeb.Pages
{
    public partial class Register : ComponentBase
    {
        private RegisterViewModel _model = new();
        private bool _isBusy;
        private string? _errorMessage;
        private string? _successMessage;

        [Inject] private IClientLoginService Auth { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private async Task HandleRegister()
        {
            if (_isBusy)
                return;

            Console.WriteLine("Register Clicked");

            _errorMessage = null;
            _successMessage = null;
            _isBusy = true;
            StateHasChanged();

            try
            {
                // simple manual validation
                if (!ValidateInputs())
                {
                    _isBusy = false;
                    StateHasChanged();
                    return;
                }

                var dto = new RegisterRequestDTO
                {
                    Vorname = _model.Vorname,
                    Nachname = _model.Nachname,
                    Email = _model.Email,
                    Passwort = _model.Passwort
                };

                var result = await Auth.RegisterAsync(dto, CancellationToken.None);

                if (result == null)
                {
                    _errorMessage = "Registrierung fehlgeschlagen.";
                    return;
                }

                _successMessage = "Registrierung erfolgreich! Weiterleitung...";
                await Task.Delay(1500);

                Nav.NavigateTo("/login");
            }
            catch (Exception ex)
            {
                _errorMessage = $"Fehler bei der Registrierung: {ex.Message}";
            }
            finally
            {
                _isBusy = false;
                StateHasChanged();
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(_model.Vorname) ||
                string.IsNullOrWhiteSpace(_model.Nachname) ||
                string.IsNullOrWhiteSpace(_model.Email) ||
                string.IsNullOrWhiteSpace(_model.Passwort))
            {
                _errorMessage = "Bitte fülle alle Felder aus.";
                return false;
            }

            if (!_model.Email.Contains('@'))
            {
                _errorMessage = "Bitte gib eine gültige Email ein.";
                return false;
            }

            if (_model.Passwort.Length < 6)
            {
                _errorMessage = "Passwort muss mindestens 6 Zeichen haben.";
                return false;
            }

            return true;
        }

        private sealed class RegisterViewModel
        {
            public string Vorname { get; set; } = string.Empty;
            public string Nachname { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Passwort { get; set; } = string.Empty;
        }
    }
}
