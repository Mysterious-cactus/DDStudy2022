﻿using Api.Consts;
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
    public class LikeController : ControllerBase
    {
        private readonly UserService _userService;

        public LikeController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize]
        public async Task AddLikeToPost(LikeModel request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
            {
                throw new Exception("not authorize");
            }
            request.AuthorId = userId;
            await _userService.AddLikeToPost(request);
        }

        [HttpPost]
        [Authorize]
        public async Task AddLikeToComment(LikeModel request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId == default)
            {
                throw new Exception("not authorize");
            }
            request.AuthorId = userId;
            await _userService.AddLikeToComment(request);
        }

        [HttpGet]
        public async Task<List<LikeModel>> GetPostLikes(Guid postId)
        {
            return _userService.GetPostLikes(postId);
        }

        [HttpGet]
        public async Task<List<LikeModel>> GetCommentLikes(Guid commentId)
        {
            return _userService.GetCommentLikes(commentId);
        }
    }
}
