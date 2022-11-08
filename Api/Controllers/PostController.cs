using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly UserService _userService;

        public PostController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        //[Authorize]
        public async Task CreatePost(List<MetadataModel> attaches, string description)
        {
                var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                if (attaches != null || description != null)
                {
                    PostModel model = new PostModel { Description = description, PostAttaches = attaches, Created = DateTime.UtcNow};
                    if (attaches != null) {
                        List<string> paths = new List<string>();
                        foreach (var attach in attaches)
                        {
                            var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), attach.TempId.ToString()));
                            if (!tempFi.Exists)
                                throw new Exception("file not found");
                            else
                            {
                                var path = Path.Combine(Directory.GetCurrentDirectory(), "attaches", attach.TempId.ToString());
                                var destFi = new FileInfo(path);
                                if (destFi.Directory != null && !destFi.Directory.Exists)
                                    destFi.Directory.Create();

                                System.IO.File.Copy(tempFi.FullName, path, true);
                                paths.Add(path);
                            }
                        }
                        await _userService.AddPost(userId, model, paths.ToArray());
                    } 
                    else
                    {
                        await _userService.AddPost(userId, model);
                    }
                }
                else
                {
                    throw new Exception("empty post");
                }
            }
            else
            {
                throw new Exception("you are not authorized");
            }
            
        }
    }
}
