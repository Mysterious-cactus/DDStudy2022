using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly UserService _userService;

        public CommentController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize]
        public async Task CreateComment(Guid userId, CommentModel model)
        {
            await _userService.AddCommentToPost(userId, model);
        }

        [HttpGet]
        public async Task<List<GetCommentsRequestModel>> GetComments(Guid postId)
        {
            return await _userService.GetCommentsFromPost(postId);
        }
    }
}
