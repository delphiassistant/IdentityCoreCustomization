using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityCoreCustomization.Data;
using IdentityCoreCustomization.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityCoreCustomization
{
    public class UserStore : IUserStore<ApplicationUser>
            , IUserSecurityStampStore<ApplicationUser>
            , IUserPasswordStore<ApplicationUser>
            , IUserEmailStore<ApplicationUser>
            , IUserPhoneNumberStore<ApplicationUser>
            , IUserLockoutStore<ApplicationUser>
            , IUserTwoFactorStore<ApplicationUser>
            , IQueryableUserStore<ApplicationUser>
            , IUserRoleStore<ApplicationUser>
            , IUserClaimStore<ApplicationUser>
            , IUserLoginStore<ApplicationUser>
            , IUserAuthenticationTokenStore<ApplicationUser>
            , IUserAuthenticatorKeyStore<ApplicationUser>
            , IUserTwoFactorRecoveryCodeStore<ApplicationUser>
            , IAsyncDisposable

    {
        private readonly ApplicationDbContext _db;
        private bool _disposed;

        // Constants used by ASP.NET Core Identity for storing tokens
        private const string InternalLoginProvider = "[AspNetUserStore]";
        private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
        private const string RecoveryCodesTokenName = "RecoveryCodes";

        public UserStore(ApplicationDbContext context)
        {
            _db = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // DbContext is managed by DI container, don't dispose it here
                _disposed = true;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            // DbContext is managed by DI container, don't dispose it here
            _disposed = true;
            await Task.CompletedTask;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #region IUserStore Implementation
        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                // FIXED: Ensure ConcurrencyStamp is generated for new users
                if (string.IsNullOrEmpty(user.ConcurrencyStamp))
                {
                    user.ConcurrencyStamp = Guid.NewGuid().ToString();
                }

                // IMPROVED: Ensure SecurityStamp is generated for new users
                if (string.IsNullOrEmpty(user.SecurityStamp))
                {
                    user.SecurityStamp = Guid.NewGuid().ToString();
                }

                var dbUser = new ApplicationUser()
                {
                    AccessFailedCount = user.AccessFailedCount,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    ConcurrencyStamp = user.ConcurrencyStamp, // Now guaranteed to have value
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    NormalizedEmail = user.NormalizedEmail,
                    NormalizedUserName = user.NormalizedUserName,
                    PasswordHash = user.PasswordHash,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    SecurityStamp = user.SecurityStamp,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    UserName = user.UserName
                };
                
                await _db.Users.AddAsync(dbUser, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                user.Id = dbUser.Id;
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError 
                { 
                    Code = "CreateFailure",
                    Description = $"An error occurred while creating the user: {ex.Message}" 
                });
            }
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                // FIXED: Generate new ConcurrencyStamp for every update to prevent concurrent modification
                user.ConcurrencyStamp = Guid.NewGuid().ToString();

                var dbUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
                if (dbUser == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "User not found." });
                }

                // IMPROVED: More comprehensive property mapping
                dbUser.AccessFailedCount = user.AccessFailedCount;
                dbUser.Email = user.Email;
                dbUser.EmailConfirmed = user.EmailConfirmed;
                dbUser.ConcurrencyStamp = user.ConcurrencyStamp; // Now properly updated
                dbUser.LockoutEnabled = user.LockoutEnabled;
                dbUser.LockoutEnd = user.LockoutEnd;
                dbUser.NormalizedEmail = user.NormalizedEmail;
                dbUser.NormalizedUserName = user.NormalizedUserName;
                dbUser.PasswordHash = user.PasswordHash;
                dbUser.PhoneNumber = user.PhoneNumber;
                dbUser.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                dbUser.SecurityStamp = user.SecurityStamp;
                dbUser.TwoFactorEnabled = user.TwoFactorEnabled;
                dbUser.UserName = user.UserName;

                await _db.SaveChangesAsync(cancellationToken);
                return IdentityResult.Success;
            }
            catch (DbUpdateConcurrencyException)
            {
                return IdentityResult.Failed(new IdentityError 
                { 
                    Code = "ConcurrencyFailure",
                    Description = "Optimistic concurrency failure, object has been modified." 
                });
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError 
                { 
                    Code = "UpdateFailure",
                    Description = $"An error occurred while updating the user: {ex.Message}" 
                });
            }
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                var dbUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
                if (dbUser == null)
                {
                    return IdentityResult.Success; // User already doesn't exist
                }

                _db.Users.Remove(dbUser);
                await _db.SaveChangesAsync(cancellationToken);
                return IdentityResult.Success;
            }
            catch (DbUpdateConcurrencyException)
            {
                return IdentityResult.Failed(new IdentityError 
                { 
                    Code = "ConcurrencyFailure",
                    Description = "Optimistic concurrency failure, object has been modified." 
                });
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError 
                { 
                    Code = "DeleteFailure",
                    Description = $"An error occurred while deleting the user: {ex.Message}" 
                });
            }
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (!int.TryParse(userId, out var id))
            {
                return null;
            }

            var dbUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            return dbUser;
        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(normalizedUserName))
            {
                throw new ArgumentNullException(nameof(normalizedUserName));
            }

            // IMPROVED: More efficient query with better normalization and indexing considerations
            var dbUser = await _db.Users.FirstOrDefaultAsync(u =>
                    u.NormalizedUserName == normalizedUserName.ToUpperInvariant()
                 || u.NormalizedEmail == normalizedUserName.ToUpperInvariant()
                 || u.PhoneNumber == normalizedUserName, // Phone numbers are not normalized
                cancellationToken);

            return dbUser;
        }
        #endregion

        #region IUserSecurityStampStore Implementation
        public Task SetSecurityStampAsync(ApplicationUser user, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }
        public Task<string> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.SecurityStamp);
        }
        #endregion

        #region IUserPasswordStore Implementation
        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }
        #endregion

        #region IUserEmailStore Implementation
        public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Email = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (string.IsNullOrEmpty(normalizedEmail))
            {
                throw new ArgumentNullException(nameof(normalizedEmail));
            }

            var dbUser = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
            return dbUser;
        }

        public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }
        #endregion

        #region IUserPhoneNumberStore Implementation
        public Task SetPhoneNumberAsync(ApplicationUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }
        #endregion

        #region IUserLockoutStore Implementation
        public Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.LockoutEnd);
        }

        public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.LockoutEnd = lockoutEnd;
            return Task.CompletedTask;
        }

        public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.LockoutEnabled);
        }
        #endregion

        #region IUserTwoFactorStore Implementation
        public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.TwoFactorEnabled);
        }
        #endregion

        #region IQueryableUserStore Implementation
        public IQueryable<ApplicationUser> Users
        {
            get
            {
                ThrowIfDisposed();
                return _db.Users.AsQueryable();
            }
        }
        #endregion

        #region IUserRoleStore Implementation
        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(roleName));
            }

            var normalizedRoleName = roleName.ToUpperInvariant();
            var role = await _db.Roles.FirstOrDefaultAsync(r => 
                r.NormalizedName == normalizedRoleName || r.Name == roleName, cancellationToken);
            
            if (role == null)
            {
                throw new InvalidOperationException($"Role '{roleName}' not found.");
            }

            var userRoleExists = await _db.UserRoles.AnyAsync(ur => 
                ur.RoleId == role.Id && ur.UserId == user.Id, cancellationToken);
            
            if (!userRoleExists)
            {
                await _db.UserRoles.AddAsync(new ApplicationUserRole()
                {
                    UserId = user.Id,
                    RoleId = role.Id
                }, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(roleName));
            }

            var normalizedRoleName = roleName.ToUpperInvariant();
            var userRole = await _db.UserRoles
                .Include(ur => ur.Role)
                .FirstOrDefaultAsync(ur => ur.UserId == user.Id && 
                    (ur.Role.NormalizedName == normalizedRoleName || ur.Role.Name == roleName), 
                    cancellationToken);

            if (userRole != null)
            {
                _db.UserRoles.Remove(userRole);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IList<string> userRoles = await _db.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync(cancellationToken);

            return userRoles;
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }

            var normalizedRoleName = roleName.ToUpperInvariant();
            var isInRole = await _db.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == user.Id && 
                    (ur.Role.NormalizedName == normalizedRoleName || ur.Role.Name == roleName), 
                    cancellationToken);

            return isInRole;
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            IList<ApplicationUser> users = await _db.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .Where(ur => ur.Role.Name == roleName)
                .Select(ur => ur.User)
                .ToListAsync(cancellationToken);
            return users;
        }
        #endregion

        #region IUserClaimStore Implementation
        public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IList<Claim> userClaims = await _db.UserClaims
                .Where(uc => uc.UserId == user.Id)
                .Select(uc => uc.ToClaim())
                .ToListAsync(cancellationToken);

            return userClaims;
        }

        public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userClaims = claims.Select(c => new ApplicationUserClaim()
            {
                ClaimType = c.Type,
                ClaimValue = c.Value,
                UserId = user.Id
            }).ToList();
            await _db.UserClaims.AddRangeAsync(userClaims, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claim == null) throw new ArgumentNullException(nameof(claim));
            if (newClaim == null) throw new ArgumentNullException(nameof(newClaim));

            var matches = await _db.UserClaims
                .Where(uc => uc.UserId == user.Id && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value)
                .ToListAsync(cancellationToken);

            if (matches.Any())
            {
                _db.UserClaims.RemoveRange(matches);
            }

            await _db.UserClaims.AddAsync(new ApplicationUserClaim()
            {
                UserId = user.Id,
                ClaimType = newClaim.Type,
                ClaimValue = newClaim.Value
            }, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null) throw new ArgumentNullException(nameof(claims));

            var claimPairs = claims.Select(c => new { c.Type, c.Value }).ToList();
            var currentClaims = await _db.UserClaims
                .Where(uc => uc.UserId == user.Id)
                .ToListAsync(cancellationToken);

            var toRemove = currentClaims
                .Where(uc => claimPairs.Any(p => p.Type == uc.ClaimType && p.Value == uc.ClaimValue))
                .ToList();

            if (toRemove.Any())
            {
                _db.UserClaims.RemoveRange(toRemove);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            IList<ApplicationUser> users = await _db.UserClaims
                .Include(c => c.User)
                .Where(uc => uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value)
                .Select(uc => uc.User)
                .ToListAsync(cancellationToken);

            return users;
        }
        #endregion

        #region IUserLoginStore Implementation
        public async Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await _db.UserLogins.AddAsync(new ApplicationUserLogin()
            {
                UserId = user.Id,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                ProviderKey = login.ProviderKey
            }, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userLogin = await _db.UserLogins.FirstOrDefaultAsync(ul =>
                ul.UserId == user.Id
                && ul.LoginProvider == loginProvider
                && ul.ProviderKey == providerKey, cancellationToken);

            if (userLogin != null)
            {
                _db.UserLogins.Remove(userLogin);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IList<UserLoginInfo> userLogins = await _db.UserLogins
                .Where(ul => ul.UserId == user.Id)
                .Select(ul => new UserLoginInfo(ul.LoginProvider, ul.ProviderKey, ul.ProviderDisplayName))
                .ToListAsync(cancellationToken);

            return userLogins;
        }

        public async Task<ApplicationUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var user = await _db.UserLogins
                .Include(ul => ul.User)
                .FirstOrDefaultAsync(ul => ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey, cancellationToken);

            return user?.User;
        }
        #endregion

        #region IUserAuthenticationTokenStore Implementation
        private Task<ApplicationUserToken> FindTokenAsync(int userId, string loginProvider, string name, CancellationToken cancellationToken)
        {
            return _db.UserTokens.FirstOrDefaultAsync(t => t.UserId == userId && t.LoginProvider == loginProvider && t.Name == name, cancellationToken);
        }

        public async Task SetTokenAsync(ApplicationUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (loginProvider == null) throw new ArgumentNullException(nameof(loginProvider));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var token = await FindTokenAsync(user.Id, loginProvider, name, cancellationToken);
            if (token == null)
            {
                token = new ApplicationUserToken
                {
                    UserId = user.Id,
                    LoginProvider = loginProvider,
                    Name = name,
                    Value = value
                };
                await _db.UserTokens.AddAsync(token, cancellationToken);
            }
            else
            {
                token.Value = value;
                _db.UserTokens.Update(token);
            }
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (loginProvider == null) throw new ArgumentNullException(nameof(loginProvider));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var token = await FindTokenAsync(user.Id, loginProvider, name, cancellationToken);
            if (token != null)
            {
                _db.UserTokens.Remove(token);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<string> GetTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (loginProvider == null) throw new ArgumentNullException(nameof(loginProvider));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var token = await FindTokenAsync(user.Id, loginProvider, name, cancellationToken);
            return token?.Value;
        }
        #endregion

        #region IUserAuthenticatorKeyStore Implementation
        public Task SetAuthenticatorKeyAsync(ApplicationUser user, string key, CancellationToken cancellationToken)
        {
            // Uses the token store under the hood
            return SetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, key, cancellationToken);
        }

        public Task<string> GetAuthenticatorKeyAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return GetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, cancellationToken);
        }
        #endregion

        #region IUserTwoFactorRecoveryCodeStore Implementation
        public Task ReplaceCodesAsync(ApplicationUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            if (recoveryCodes == null) throw new ArgumentNullException(nameof(recoveryCodes));
            var merged = string.Join(";", recoveryCodes);
            return SetTokenAsync(user, InternalLoginProvider, RecoveryCodesTokenName, merged, cancellationToken);
        }

        public async Task<bool> RedeemCodeAsync(ApplicationUser user, string code, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (code == null) throw new ArgumentNullException(nameof(code));

            var merged = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodesTokenName, cancellationToken);
            if (string.IsNullOrEmpty(merged)) return false;

            var split = merged.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var removed = split.Remove(code);
            if (!removed) return false;

            await SetTokenAsync(user, InternalLoginProvider, RecoveryCodesTokenName, string.Join(";", split), cancellationToken);
            return true;
        }

        public async Task<int> CountCodesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var merged = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodesTokenName, cancellationToken);
            if (string.IsNullOrEmpty(merged)) return 0;
            return merged.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
        #endregion
    }
}
