using Api.Configs;
using Api.Models;
using Api.Models.Attach;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using DAL;
using Api.Exceptions;
using DAL.Entities;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Api.Models.Like;
using Api.Models.Subcribes;
using System.ComponentModel.Design;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore.Internal;

namespace Api.Services
{
    public class PostService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        public PostService(IMapper mapper, IOptions<AuthConfig> config, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task CreatePost(CreatePostRequest request)
        {
            var model = _mapper.Map<CreatePostModel>(request);

            model.Contents.ForEach(x =>
            {
                x.AuthorId = model.AuthorId;
                x.FilePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "attaches",
                    x.TempId.ToString());

                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), x.TempId.ToString()));
                if (tempFi.Exists)
                {
                    var destFi = new FileInfo(x.FilePath);
                    if (destFi.Directory != null && !destFi.Directory.Exists)
                        destFi.Directory.Create();

                    File.Move(tempFi.FullName, x.FilePath, true);
                }
            });

            var dbModel = _mapper.Map<Post>(model);
            await _context.Posts.AddAsync(dbModel);
            await _context.SaveChangesAsync();

        }
        public async Task AddLikeToPost(LikeModel model)
        {
            var like = new LikePost { AuthorId = model.AuthorId, PostId = model.EntityId };
            await _context.LikesPosts.AddAsync(like);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PostModel>> GetPosts(int skip, int take, Guid userId)
        {
            //var user = await _context.Users.Include(x => x.Subscribes).FirstOrDefaultAsync(x => x.Id == userId);
             var subs = await _context.Subscriptions
                 .Where(x => x.Who == userId)
                 .Select(x => _mapper.Map<SubscriptionModel>(x)).ToListAsync();
            List<Guid> subscribesIds = new List<Guid>();
            foreach (var sub in subs)
            {
                subscribesIds.Add(sub.OnWhom);
            }
            List<PostModel> posts = new List<PostModel>();
            //if (user != null)
            //{
                    posts = await _context.Posts
                    .Include(x => x.Author).ThenInclude(x => x.Avatar)
                    .Include(x => x.PostComments).ThenInclude(x => x.Author)
                    .Include(x => x.PostContents).AsNoTracking().OrderByDescending(x => x.Created).Skip(skip).Take(take)
                    
                    .Where(x => subscribesIds.Contains(x.AuthorId))
                    .Select(x => _mapper.Map<PostModel>(x))
                    .ToListAsync();
            //}
            foreach (var post in posts)
            {
                post.LikeCount = (await GetPostLikes(post.Id)).Count();
            }
            return posts;

        }

        public async Task<PostModel> GetPostById(Guid id)
        {
            var post = await _context.Posts
                  .Include(x => x.Author).ThenInclude(x => x.Avatar)
                  .Include(x => x.PostContents).AsNoTracking()
                  .Include(x => x.PostComments).ThenInclude(x => x.Author)
                  .Where(x => x.Id == id)
                  .Select(x => _mapper.Map<PostModel>(x))
                  .FirstOrDefaultAsync();
            if (post == null)
                throw new PostNotFoundException();
            post.LikeCount = (await GetPostLikes(id)).Count();
            return post;
        }

        public async Task<AttachModel> GetPostContent(Guid postContentId)
        {
            var res = await _context.PostContents.FirstOrDefaultAsync(x => x.Id == postContentId);

            return _mapper.Map<AttachModel>(res);
        }

        public async Task<IEnumerable<LikeModel>> GetPostLikes(Guid postId)
        {
            //var likes = _context.LikesPosts.Include(x => x.PostId).Include(x => x.AuthorId).Select(x => x.PostId == postId);
            var likes = await _context.LikesPosts.AsNoTracking()
                .Where(x => x.PostId == postId)
                .Select(x => _mapper.Map<LikeModel>(x))
                .ToListAsync();
            //var likes = from like in _context.LikesPosts where like.PostId == postId select like;
            //List<LikeModel> likesList = new List<LikeModel>();
            //foreach (var like in likes)
            //{
            //    likesList.Add(_mapper.Map<LikeModel>(like));
            //}
            return likes;
        }
    }
}
