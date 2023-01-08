using Api.Consts;
using Api.Models.Like;
using Api.Services;
using Common.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    public class LikeController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly PostService _postService;
        private readonly CommentService _commentService;

        public LikeController(UserService userService, PostService postService, CommentService commentService)
        {
            _userService = userService;
            _postService = postService;
            _commentService = commentService;
        }

        [HttpPost]
        [Authorize]
        public async Task AddLikeToPost(Guid postId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
            {
                throw new Exception("not authorize");
            }
            await _postService.AddLikeToPost(userId, postId);
        }

        [HttpPost]
        [Authorize]
        public async Task AddLikeToComment(Guid commentId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
            {
                throw new Exception("not authorize");
            }
            await _commentService.AddLikeToComment(userId, commentId);
        }

        [HttpPost]
        [Authorize]
        public async Task RemoveLikeFromComment(Guid commentId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
            {
                throw new Exception("not authorize");
            }
            await _commentService.RemoveLikeFromComment(userId, commentId);
        }

        [HttpPost]
        [Authorize]
        public async Task RemoveLikeFromPost(Guid postId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
            {
                throw new Exception("not authorize");
            }
            await _postService.RemoveLikeFromPost(userId, postId);
        }

        [HttpGet]
        public async Task<IEnumerable<Guid>> GetPostLikes(Guid postId)
        {
            return await _postService.GetPostLikes(postId);
        }

        [HttpGet]
        public async Task<IEnumerable<Guid>> GetCommentLikes(Guid commentId)
        {
            return await _commentService.GetCommentLikes(commentId);
        }
    }
}
