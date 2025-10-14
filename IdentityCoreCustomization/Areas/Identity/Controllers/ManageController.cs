using IdentityCoreCustomization.Areas.Identity.Models;
using IdentityCoreCustomization.Data;
using IdentityCoreCustomization.Models.Identity;
using IdentityCoreCustomization.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace IdentityCoreCustomization.Areas.Identity.Controllers
{
    [Authorize]
    [Area("Identity")]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISmsService smsService;
        private readonly IEmailSender _emailSender;
        private ApplicationDbContext db;

        public ManageController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            IEmailSender emailSender,
            ISmsService smsService)
        {
            db = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            this.smsService = smsService;
        }
        
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"یافتن کاربری با شناسه '{_userManager.GetUserId(User)}' میسر نیست.");
            }
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            var model = new ManageIndexModel()
            {
                PhoneNumber = phoneNumber,
                Username = userName,
                PhoneNumberConfirmed = await _userManager.IsPhoneNumberConfirmedAsync(user)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ManageIndexModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            if (user == null)
            {
                return NotFound($"یافتن کاربری با شناسه '{_userManager.GetUserId(User)}' میسر نیست.");
            }

            if (!ModelState.IsValid)
            {
                
                model = new ManageIndexModel()
                {
                    PhoneNumber = phoneNumber,
                    Username = userName,
                    PhoneNumberConfirmed = await _userManager.IsPhoneNumberConfirmedAsync(user)
                };
                return View(model);
            }

            //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            //if (model.PhoneNumber != phoneNumber)
            //{
            //    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
            //    if (!setPhoneResult.Succeeded)
            //    {
            //        model.StatusMessage = "خطا در تغییر شماره موبایل.";
            //        return RedirectToAction("Index");
            //    }
            //}

            await _signInManager.RefreshSignInAsync(user);
            model.StatusMessage = "Your profile has been updated";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Email()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = await _userManager.GetEmailAsync(user);
            

            var model  = new EmailChangeModel()
            {
                Email = email,
                NewEmail = email,
                IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Email(EmailChangeModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = await _userManager.GetEmailAsync(user);
            if (model.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(
                    action:"ConfirmEmailChange",
                    controller:"Manage",
                    values: new { userId = userId, email = model.NewEmail, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    model.NewEmail,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                model.StatusMessage = "لینک تایید برای شما ایمیل شد. لطفا ایمیل تان را چک کنید.";
                return View(model);
            }

            model.StatusMessage = "ایمیل شما تغییری نکرد.";
            return View(model);
        }

        public async Task<IActionResult> SendVerificationEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                
                return View();
            }

            

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action(
                action:"ConfirmEmail",
                controller:"Manage",
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            //ModelBinderFactory.StatusMessage = "Verification email sent. Please check your email.";
            return View();
        }

        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }

            await _signInManager.RefreshSignInAsync(user);
            //_logger.LogInformation("User changed their password successfully.");
            //StatusMessage = "Your password has been changed.";
            return View();
        }

        public async Task<IActionResult> TwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"کاربری با شناسه '{_userManager.GetUserId(User)}' یافت نشد.");
            }

            var model = new TwoFactorAuthenticationModel()
            {
                Is2faEnabled = user.TwoFactorEnabled,
                CanEnable2fa = user.PhoneNumberConfirmed && (!string.IsNullOrEmpty(user.PhoneNumber))
            };
            return View(model);
        }

        public async Task<IActionResult> SetTwoFactorAuthentication(bool enabled)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"کاربری با شناسه '{_userManager.GetUserId(User)}' یافت نشد.");
            }

            if (user.PhoneNumberConfirmed && (!string.IsNullOrEmpty(user.PhoneNumber)))
            {
                user.TwoFactorEnabled = enabled;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction("TwoFactorAuthentication");
        }
        
        public async Task<IActionResult> AddPhoneNumber()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.PhoneNumber);
                if (user != null)
                {
                    ModelState.AddModelError("", $"شماره موبایل {model.PhoneNumber} توسط کاربر دیگری ثبت شده است.");
                    return View(model);
                }

                var phoneTokenModel = new UserPhoneToken();
                phoneTokenModel.Initialize();
                phoneTokenModel.ExpireTime = DateTime.Now.AddMinutes(5);
                phoneTokenModel.PhoneNumber = model.PhoneNumber;
                
                await db.UserPhoneTokens.AddAsync(phoneTokenModel);
                await db.SaveChangesAsync();
                await smsService.SendSms(
                    $"کد امنیتی شما: {phoneTokenModel.AuthenticationCode}\r\n\r\n",
                    new List<string>() { model.PhoneNumber });
                return RedirectToAction("VerifyPhoneNumber", new { Key = phoneTokenModel.AuthenticationKey });
            }
            return View(model);
        }

        public IActionResult VerifyPhoneNumber(string Key)
        {
            if (string.IsNullOrEmpty(Key))
            {
                return RedirectToAction("AddPhoneNumber");
            }
            var model = new UserPhoneTokenConfirmViewModel() { AuthenticationKey = Key };
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async  Task<IActionResult> VerifyPhoneNumber(UserPhoneTokenConfirmViewModel model)
        {
            if (ModelState.IsValid)
            {
                var confirmRow =
                    db.UserPhoneTokens.FirstOrDefault(pr => pr.AuthenticationKey == model.AuthenticationKey);
                if (confirmRow == null)
                {
                    ModelState.AddModelError("", "کلید شناسایی معتبر نیست");
                    return View(model);
                }
                else
                {
                    if (model.AuthenticationCode != confirmRow.AuthenticationCode)
                    {
                        ModelState.AddModelError("", "کد امنیتی وارد شده صحیح نیست");
                        return View(model);
                    }
                    else
                    {
                        var user = await _userManager.GetUserAsync(User);
                        confirmRow.Confirmed = true;
                        await db.SaveChangesAsync();
                              

                        var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, confirmRow.PhoneNumber);
                        if (!setPhoneResult.Succeeded)
                        {
                            ModelState.AddModelError("", "خطا در ثبت شماره موبایل.");
                            return View(model);
                        }

                        user.PhoneNumberConfirmed = true;
                        await _userManager.UpdateAsync(user);
                        await _signInManager.RefreshSignInAsync(user);

                        
                        return RedirectToAction("Index");
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> PersonalData()
        {

            return View();
        }

        public string GenerateNewAuthenticationCode()
        {
            Random rnd = new Random();
            return rnd.Next(1000000, 9999999).ToString();
        }

        // NEW: Confirm email for the current email address
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("ConfirmEmail", new ManageIndexModel { StatusMessage = "درخواست نامعتبر است." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"کاربر با شناسه '{userId}' یافت نشد.");
            }

            var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, decoded);
            var message = result.Succeeded ? "ایمیل شما با موفقیت تایید شد." : "خطا در تایید ایمیل.";
            return View("ConfirmEmail", new ManageIndexModel { StatusMessage = message });
        }

        // NEW: Confirm change of email address
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string code)
        {
            if (userId == null || email == null || code == null)
            {
                return View("ConfirmEmailChange", new ManageIndexModel { StatusMessage = "درخواست نامعتبر است." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"کاربر با شناسه '{userId}' یافت نشد.");
            }

            var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ChangeEmailAsync(user, email, decoded);
            if (result.Succeeded)
            {
                // Ensure EmailConfirmed stays true after change
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }
                return View("ConfirmEmailChange", new ManageIndexModel { StatusMessage = "ایمیل شما با موفقیت تغییر یافت." });
            }

            return View("ConfirmEmailChange", new ManageIndexModel { StatusMessage = "خطا در تغییر ایمیل." });
        }
    }
}
