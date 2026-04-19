using GropMng.Core.Common.Exceptions;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Models.OwnerAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Controllers;

/// <summary>
/// Hosts the public registration, sign-in, email confirmation, and password reset flows for owners.
/// </summary>
[AllowAnonymous]
[Route("owner/auth")]
public class OwnerAuthController : Controller
{
    private readonly IOwnerAuthenticationService _ownerAuthenticationService;
    private readonly IOwnerAccountFlowService _ownerAccountFlowService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnerAuthController"/> class.
    /// </summary>
    public OwnerAuthController(
        IOwnerAuthenticationService ownerAuthenticationService,
        IOwnerAccountFlowService ownerAccountFlowService,
        IWebHostEnvironment webHostEnvironment)
    {
        _ownerAuthenticationService = ownerAuthenticationService ?? throw new ArgumentNullException(nameof(ownerAuthenticationService));
        _ownerAccountFlowService = ownerAccountFlowService ?? throw new ArgumentNullException(nameof(ownerAccountFlowService));
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
    }

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        return View(new OwnerLoginModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(OwnerLoginModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var owner = await _ownerAuthenticationService.ValidateCredentialsAsync(model.Email, model.Password, cancellationToken);
        if (owner is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password, or the account has not been activated yet.");
            return View(model);
        }

        await _ownerAuthenticationService.SignInAsync(owner, model.RememberMe, cancellationToken);
        TempData["SuccessMessage"] = "Welcome back.";
        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new OwnerRegisterModel());
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(OwnerRegisterModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await _ownerAccountFlowService.RegisterAsync(new OwnerRegistrationRequest(
                model.FirstName,
                model.LastName,
                model.DisplayName,
                model.Email,
                model.Password), cancellationToken);

            if (result.RequiresEmailConfirmation)
            {
                var link = Url.Action(
                    nameof(ConfirmEmail),
                    "OwnerAuth",
                    new { email = result.Owner.Email, token = result.EmailConfirmationToken },
                    Request.Scheme);

                return View("Status", new OwnerAuthStatusViewModel
                {
                    PageTitle = "Confirm your email",
                    Heading = "One more step",
                    Message = "Your account was created successfully. Please confirm your email before signing in.",
                    SecondaryMessage = _webHostEnvironment.IsDevelopment()
                        ? "Development mode is enabled, so the local confirmation link is exposed below until email delivery is integrated."
                        : "If this were a production flow, a confirmation email would be sent to your inbox.",
                    ActionText = _webHostEnvironment.IsDevelopment() ? "Open confirmation link" : "Back to sign in",
                    ActionUrl = _webHostEnvironment.IsDevelopment() ? link : Url.Action(nameof(Login), "OwnerAuth")
                });
            }

            await _ownerAuthenticationService.SignInAsync(result.Owner, cancellationToken: cancellationToken);
            TempData["SuccessMessage"] = "Registration completed successfully.";
            return RedirectToAction("Index", "Home");
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string email, string token, CancellationToken cancellationToken)
    {
        var confirmed = await _ownerAccountFlowService.ConfirmEmailAsync(email, token, cancellationToken);
        var model = confirmed
            ? new OwnerAuthStatusViewModel
            {
                PageTitle = "Email confirmed",
                Heading = "Email verified",
                Message = "Your account is now active and you can sign in.",
                ActionText = "Sign in",
                ActionUrl = Url.Action(nameof(Login), "OwnerAuth")
            }
            : new OwnerAuthStatusViewModel
            {
                PageTitle = "Confirmation failed",
                Heading = "Confirmation link is invalid",
                Message = "The confirmation link is invalid or has expired. Please register again or request support.",
                ActionText = "Create account",
                ActionUrl = Url.Action(nameof(Register), "OwnerAuth")
            };

        return View("Status", model);
    }

    [HttpGet("forgot-password")]
    public IActionResult ForgotPassword()
    {
        return View(new OwnerForgotPasswordModel());
    }

    [HttpPost("forgot-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(OwnerForgotPasswordModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _ownerAccountFlowService.RequestPasswordResetAsync(model.Email, cancellationToken);
        var resetLink = result.EmailMatched && !string.IsNullOrWhiteSpace(result.ResetToken)
            ? Url.Action(nameof(ResetPassword), "OwnerAuth", new { email = model.Email, token = result.ResetToken }, Request.Scheme)
            : null;

        return View("Status", new OwnerAuthStatusViewModel
        {
            PageTitle = "Password reset requested",
            Heading = "Check your email",
            Message = "If an account exists for that email, a password reset link is now available.",
            SecondaryMessage = _webHostEnvironment.IsDevelopment() && !string.IsNullOrWhiteSpace(resetLink)
                ? "Development mode is enabled, so the local reset link is exposed below until email delivery is integrated."
                : "For security reasons, the message stays the same whether the email exists or not.",
            ActionText = _webHostEnvironment.IsDevelopment() && !string.IsNullOrWhiteSpace(resetLink) ? "Open reset link" : "Back to sign in",
            ActionUrl = _webHostEnvironment.IsDevelopment() && !string.IsNullOrWhiteSpace(resetLink)
                ? resetLink
                : Url.Action(nameof(Login), "OwnerAuth")
        });
    }

    [HttpGet("reset-password")]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            TempData["ErrorMessage"] = "The password reset link is invalid or incomplete.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new OwnerResetPasswordModel
        {
            Email = email,
            Token = token
        });
    }

    [HttpPost("reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(OwnerResetPasswordModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resetWorked = await _ownerAccountFlowService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword, cancellationToken);
        if (!resetWorked)
        {
            ModelState.AddModelError(string.Empty, "The password reset link is invalid or has expired.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Your password has been reset. Please sign in with the new password.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _ownerAuthenticationService.SignOutAsync();
        TempData["SuccessMessage"] = "You have been signed out.";
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
