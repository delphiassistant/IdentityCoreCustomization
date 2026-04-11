using System.Threading;

namespace IdentityCoreCustomization.Services
{
    /// <summary>
    /// Singleton that caches whether at least one Admin user exists.
    /// Avoids a DB round-trip on every request after setup is complete.
    /// States: -1 = unknown (not yet checked), 0 = no admin, 1 = has admin.
    /// </summary>
    public sealed class AdminSetupState
    {
        private volatile int _state = -1; // -1 = unknown, 0 = no admin, 1 = has admin

        public bool IsUnknown => _state == -1;
        public bool HasAdmin => _state == 1;

        public void MarkHasAdmin() => Interlocked.Exchange(ref _state, 1);
        public void MarkNoAdmin() => Interlocked.Exchange(ref _state, 0);

        /// <summary>Resets to unknown so the next request re-checks the DB.</summary>
        public void Reset() => Interlocked.Exchange(ref _state, -1);
    }
}
