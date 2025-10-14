using IdentityCoreCustomization.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityCoreCustomization.Models.Identity
{
    public class AuthenticationTicket
    {
        [Key]
        public int TicketID { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public byte[] Value { get; set; }

        public DateTimeOffset? LastActivity { get; set; }

        public DateTimeOffset? Expires { get; set; }
    }
}
