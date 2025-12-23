using KinoAppDB;
using Microsoft.AspNetCore.Mvc;

namespace KinoAppService.Controllers
{
    /// <summary>
    /// Base controller providing transactional execution helpers for API endpoints.
    /// </summary>
    /// <remarks>
    /// The transaction scope is created per request and wraps the endpoint action in
    /// Create → Begin → Commit/Rollback semantics. Exceptions are rethrown to preserve
    /// standard ASP.NET Core error handling behavior.
    /// </remarks>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private readonly IKinoAppDbContextScope _scope;

        /// <summary>
        /// Creates a new <see cref="BaseController"/>.
        /// </summary>
        /// <param name="scope">Database scope used to manage context lifetime and transactions.</param>
        protected BaseController(IKinoAppDbContextScope scope)
        {
            _scope = scope;
        }

        /// <summary>
        /// Executes the given action within a database transaction.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="ct">Cancellation token.</param>
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

        /// <summary>
        /// Convenience overload for actions that do not need access to the cancellation token.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="ct">Cancellation token.</param>
        protected Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action, CancellationToken ct = default)
            => ExecuteAsync(_ => action(), ct);
    }
}
