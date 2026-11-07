using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyPlanner.API.DTOs.Subscription;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(
        ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<SubscriptionStatusDto>>
        GetMySubscription()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) ||
            !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _subscriptionService
                    .GetStatusAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("checkout")]
        public async Task<ActionResult<CreateCheckoutSessionResponseDto>>
            CreateCheckoutSession()
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !int.TryParse(
                    userIdClaim,
                    out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var result =
                    await _subscriptionService
                        .CreateCheckoutSessionAsync(
                            userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult>
            CancelSubscription()
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !int.TryParse(
                    userIdClaim,
                    out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _subscriptionService
                    .CancelSubscriptionAsync(userId);

                return Ok(new
                {
                    message =
                        "Az előfizetésed a jelenlegi számlázási időszak végén megszűnik."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("reactivate")]
        public async Task<IActionResult>
            ReactivateSubscription()
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !int.TryParse(
                    userIdClaim,
                    out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _subscriptionService
                    .ReactivateSubscriptionAsync(userId);

                return Ok(new
                {
                    message =
                        "Az előfizetés automatikus megújítása ismét aktív."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("portal")]
        public async Task<IActionResult>
            CreatePortalSession()
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !int.TryParse(
                    userIdClaim,
                    out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var portalUrl =
                    await _subscriptionService
                        .CreatePortalSessionAsync(userId);

                return Ok(new
                {
                    portalUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("sync")]
        public async Task<IActionResult>
            SyncSubscription(
                [FromQuery] string? sessionId = null)
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !int.TryParse(
                    userIdClaim,
                    out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _subscriptionService
                    .SyncSubscriptionAsync(
                        userId,
                        sessionId);

                var result =
                    await _subscriptionService
                        .GetStatusAsync(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            using var reader =
                new StreamReader(
                    HttpContext.Request.Body);

            var json =
                await reader.ReadToEndAsync();

            var stripeSignature =
                Request.Headers[
                    "Stripe-Signature"
                ].ToString();

            if (string.IsNullOrWhiteSpace(
                stripeSignature))
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Hiányzik a Stripe-Signature fejléc."
                    });
            }

            try
            {
                await _subscriptionService
                    .HandleWebhookAsync(
                        json,
                        stripeSignature);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new
                    {
                        message = ex.Message
                    });
            }
        }
}