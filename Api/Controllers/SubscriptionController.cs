using Api.Consts;
using Api.Models.Subcribes;
using Api.Services;
using Common.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Api")]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionService _subsService;

        public SubscriptionController(SubscriptionService subsService)
        {
            _subsService = subsService;
        }

        [HttpPost]
        public async Task Subscribe(Guid onWhom)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _subsService.Subscribe(userId, onWhom);
            }
        }

        [HttpPost]
        public async Task UnSubscribe(Guid fromWhom)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _subsService.UnSubscribe(userId, fromWhom);
            }
        }
    }
}
