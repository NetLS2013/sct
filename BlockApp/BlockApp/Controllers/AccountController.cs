using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlockApp.Data.Entities;
using BlockApp.Extensions;
using BlockApp.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BlockApp.Models;
using BlockApp.Models.AccountViewModels;
using BlockApp.Services;
using Microsoft.Extensions.Configuration;

namespace BlockApp.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
        }

        [TempData]
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Login page.
        /// </summary>
        /// <param name="returnUrl">Redirect to url after authorization.</param>
        /// <returns>Returns login page.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    return RedirectToAction("Index", "Manage");
                }

                return RedirectToLocal(returnUrl);
            }
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Login by email and password. In the case of an error, we return a page with error.
        /// To enable password failures to trigger account lockout, set lockoutOnFailure: true
        /// </summary>
        /// <param name="model">Login model that contains credentials data.</param>
        /// <param name="returnUrl">Redirect to url after authorization.</param>
        /// <returns>Redisplay form with error or redirect to url.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Incorrect email or password.");
                    return View(model);
                }
            }
            
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        /// <summary>
        /// Register page.
        /// </summary>
        /// <param name="returnUrl">Redirect to url after registration.</param>
        /// <returns>Returns registration page.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    return RedirectToAction("Index", "Manage");
                }

                return RedirectToLocal(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Register a new account and sign in. In the case of an error, we return a page with a list of errors. Send an email with confirmation link.
        /// </summary>
        /// <param name="model">Register model that holds data for registration.</param>
        /// <param name="returnUrl">Redirect to url after registration.</param>
        /// <returns>Redisplay form with errors or redirect to url.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    
                    await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);
                    await _emailSender.SendEmailAsync(_configuration["EmailNotifcation"], "Notification:",
                        $"New user {model.Email} sign up on the safecryptotrades.");
                    
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    _logger.LogInformation("User created a new account with password.");
                    
                    return RedirectToLocal(returnUrl);
                }
                
                AddErrors(result);
            }
            
            return View(model);
        }

        /// <summary>
        /// Logout user from the system.
        /// </summary>
        /// <returns>Redirect to the home page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// Confirmation by e-mail at the link with the code that was sent to the mail.
        /// </summary>
        /// <param name="userId">User identification.</param>
        /// <param name="code">Unique code for confirming the email.</param>
        /// <returns>Returns the page confirm email with message result.</returns>
        /// <exception cref="ApplicationException">Unable to load user with ID.</exception>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            
            var result = await _userManager.ConfirmEmailAsync(user, code);

            ViewData["Message"] = "Thank you for confirming your email.";
            
            if (!result.Succeeded)
            {
                ViewData["Message"] = "Invalid confirmation token.";
            }

            return View();
        }

        /// <summary>
        /// Password recovery.
        /// </summary>
        /// <returns>Returns the page for password recovery.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Send a link to recover your password by email.
        /// Don't reveal that the user does not exist or is not confirmed.
        /// For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
        /// </summary>
        /// <param name="model">User email.</param>
        /// <returns>Redisplay form with error or redirect to the forgot password confirmation page with message result.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return RedirectToAction(nameof(ForgotPasswordError));
                }
                
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                
                await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            return View(model);
        }

        /// <summary>
        /// Forgot password confirmation.
        /// </summary>
        /// <returns>Returns the page with message result.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordError()
        {
            return View();
        }

        /// <summary>
        /// Reset password by code.
        /// </summary>
        /// <param name="code">Unique code for reset password.</param>
        /// <returns>Returns the reset password page.</returns>
        /// <exception cref="ApplicationException">A code must be supplied for password reset.</exception>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        /// <summary>
        /// Set new password.
        /// Don't reveal that the user does not exist.
        /// </summary>
        /// <param name="model">User email.</param>
        /// <returns>Redisplay form with error or redirect to the reset confirmation page with message result.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            
            AddErrors(result);
            
            return View();
        }

        /// <summary>
        /// Reset password confirmation.
        /// </summary>
        /// <returns>Returns the page with message result.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        /// <summary>
        /// Access denied.
        /// </summary>
        /// <returns>Returns the page with message result.</returns>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Terms page.
        /// </summary>
        /// <returns>
        /// The <see cref="IActionResult"/>.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("/[action]")]
        public IActionResult Terms()
        {
            return View();
        }
        
        /// <summary>
        /// Video guid page.
        /// </summary>
        /// <returns>
        /// The <see cref="IActionResult"/>.
        /// </returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("/[action]")]
        public IActionResult VideoGuide()
        {
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
        
        #endregion
    }
}
