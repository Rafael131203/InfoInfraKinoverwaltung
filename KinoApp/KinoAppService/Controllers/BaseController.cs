using KinoAppCore.Abstractions;
using KinoAppDB;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private readonly IKinoAppDbContextScope _scope;

        protected BaseController(IKinoAppDbContextScope scope)
        {
            _scope = scope;
        }

        /// <summary>
        /// Wrap an action in a transaction: Create -> Begin -> Commit/Rollback.
        /// </summary>
        protected async Task<IActionResult> ExecuteAsync(
            Func<CancellationToken, Task<IActionResult>> action,
            CancellationToken ct = default)
        {
            _scope.Create();
            await _scope.BeginAsync(ct);

            try
            {
                var result = await action(ct);
                await _scope.CommitAsync(ct);
                return result;
            }
            catch
            {
                await _scope.RollbackAsync(ct);
                throw;
            }
        }

        // Convenience overload if you don't need the token in your lambda
        protected Task<IActionResult> ExecuteAsync(
            Func<Task<IActionResult>> action,
            CancellationToken ct = default)
            => ExecuteAsync(_ => action(), ct);
    }
}
