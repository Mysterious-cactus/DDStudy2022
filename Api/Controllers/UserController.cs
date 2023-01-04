﻿using Api.Consts;
using Api.Models.Attach;
using Api.Models.User;
using Api.Services;
using Api.Exceptions;
using Common.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Models.Subcribes;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Api")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService, LinkGeneratorService links)
        {
            _userService = userService;
            links.LinkAvatarGenerator = x =>
            Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
            {
                userId = x.Id,
            });
        }
        [HttpPost]
        public async Task EnterAdditionalInfo(UserAdditionalInfoModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _userService.EnterAdditionalInfo(model, userId);
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpPost]
        public async Task AddAvatarToUser(MetadataModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));
                if (!tempFi.Exists)
                    throw new Exception("file not found");
                else
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "attaches", model.TempId.ToString());
                    var destFi = new FileInfo(path);
                    if (destFi.Directory != null && !destFi.Directory.Exists)
                        destFi.Directory.Create();

                    System.IO.File.Copy(tempFi.FullName, path, true);

                    await _userService.AddAvatarToUser(userId, model, path);
                }
            }
            else
                throw new Exception("you are not authorized");

        }


        [HttpGet]
        public async Task<IEnumerable<UserAvatarModel>> GetUsers()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
               return await _userService.GetUsers(userId);
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpGet]
        public async Task<UserAvatarModel> GetCurrentUser()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {

                return await _userService.GetUser(userId);
            }
            else
                throw new Exception("you are not authorized");

        }

        [HttpGet]
        public async Task<UserAvatarModel> GetUserById(Guid userId)
        {
            return await _userService.GetUser(userId);
        }

        [HttpGet]
        public async Task<UserAvatarModel> FindUserByName(string username)
        {
            return await _userService.FindUserByName(username);
        }
    }
}
