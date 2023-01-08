using Api.Consts;
using Api.Models;
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
    public class CommentController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        [Authorize]
        public async Task CreateComment(CommentModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {

                await _commentService.AddCommentToPost(userId, model);
            }
            else
                throw new Exception("you are not authorized");
            
        }

        //[HttpGet]
        //public async Task<List<GetCommentsRequestModel>> GetComments(Guid postId)
        //{
        //    return await _commentService.GetCommentsFromPost(postId);
        //}
    }
}
