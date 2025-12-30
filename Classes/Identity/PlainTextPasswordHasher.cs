using System.Text.RegularExpressions;
using IdentityCoreCustomization.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace IdentityCoreCustomization.Classes.Identity
{
    public class PlainTextPasswordHasher<TUser> : PasswordHasher<TUser> where TUser : class
    {
        public override string HashPassword(TUser user, string password)
        {
            return password;
        }

        public override PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            return hashedPassword == providedPassword
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }

    
}
