using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IdentityCoreCustomization.Classes.Validation
{
    /// <summary>
    /// Validates that a role name contains only English letters and numbers (no spaces or special characters)
    /// </summary>
    public class EnglishAlphanumericAttribute : ValidationAttribute
    {
        private readonly bool _allowSpaces;

        public EnglishAlphanumericAttribute(bool allowSpaces = false)
        {
            _allowSpaces = allowSpaces;
            ErrorMessage = "نام نقش فقط می‌تواند شامل حروف انگلیسی و اعداد باشد (بدون فاصله یا کاراکترهای خاص).";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            string stringValue = value.ToString();

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return ValidationResult.Success; // Let Required attribute handle this
            }

            // Pattern: Only English letters (a-z, A-Z) and numbers (0-9)
            // Optionally allow spaces if _allowSpaces is true
            string pattern = _allowSpaces ? @"^[a-zA-Z0-9\s]+$" : @"^[a-zA-Z0-9]+$";

            if (!Regex.IsMatch(stringValue, pattern))
            {
                return new ValidationResult(ErrorMessage ?? GetErrorMessage());
            }

            // Check for Persian/Arabic characters specifically
            if (Regex.IsMatch(stringValue, @"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF]"))
            {
                return new ValidationResult("نام نقش نمی‌تواند شامل حروف فارسی یا عربی باشد. فقط از حروف انگلیسی استفاده کنید.");
            }

            // Check for special characters
            if (Regex.IsMatch(stringValue, @"[^a-zA-Z0-9\s]"))
            {
                return new ValidationResult("نام نقش نمی‌تواند شامل کاراکترهای خاص (@, #, $, %, &, ...) باشد.");
            }

            // Check for spaces (if not allowed)
            if (!_allowSpaces && stringValue.Contains(" "))
            {
                return new ValidationResult("نام نقش نمی‌تواند شامل فاصله باشد. از underscore (_) یا camelCase استفاده کنید.");
            }

            return ValidationResult.Success;
        }

        private string GetErrorMessage()
        {
            return _allowSpaces
                ? "نام نقش فقط می‌تواند شامل حروف انگلیسی، اعداد و فاصله باشد."
                : "نام نقش فقط می‌تواند شامل حروف انگلیسی و اعداد باشد (بدون فاصله).";
        }
    }
}
