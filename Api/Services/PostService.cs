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
        private readonly DataContext _context;
        public PostService(IMapper mapper, DataContext context)
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
        public async Task AddLikeToPost(Guid userId, Guid postId)
        {
            var like = new LikePost { AuthorId = userId, PostId = postId };
            await _context.LikesPosts.AddAsync(like);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PostModel>> GetPosts(int skip, int take, Guid userId)
        {
             var subs = await _context.Subscriptions
                 .Where(x => x.Who == userId)
                 .Select(x => _mapper.Map<SubscriptionModel>(x)).ToListAsync();
            List<Guid> subscribesIds = new List<Guid>();
            foreach (var sub in subs)
            {
                subscribesIds.Add(sub.OnWhom);
            }
            var posts = await _context.Posts
                    .Include(x => x.Author).ThenInclude(x => x.Avatar)
                    .Include(x => x.PostComments).ThenInclude(x => x.Author)
                    .Where(x => subscribesIds.Contains(x.AuthorId))
                    .Include(x => x.PostContents).AsNoTracking().OrderByDescending(x => x.Created).Skip(skip).Take(take)
                    .Select(x => _mapper.Map<PostModel>(x))
                    .ToListAsync();
            foreach (var post in posts)
            {
                var likes = await GetPostLikes(post.Id);
                post.LikedByMe = likes.Contains(userId) ? 1 : 0;
                post.LikeCount = likes.Count();
                post.CommentCount = post.Comments != null ? post.Comments.Count() : 0;
            }
            return posts;

        }

        public async Task<PostModel> GetPostById(Guid id, Guid userId)
        {
            var post = await _context.Posts
                  .Include(x => x.Author).ThenInclude(x => x.Avatar)
                  .Include(x => x.PostContents).AsNoTracking()
                  .Include(x => x.PostComments).ThenInclude(x => x.Author).ThenInclude(x => x.Avatar)
                  .Where(x => x.Id == id)
                  .Select(x => _mapper.Map<PostModel>(x))
                  .FirstOrDefaultAsync();
            if (post == null)
                throw new PostNotFoundException();
            var likes = await GetPostLikes(id);
            post.LikeCount = likes.Count();
            post.LikedByMe =  likes.Contains(userId) ? 1 : 0;
            return post;
        }

        public async Task<AttachModel> GetPostContent(Guid postContentId)
        {
            var res = await _context.PostContents.FirstOrDefaultAsync(x => x.Id == postContentId);

            return _mapper.Map<AttachModel>(res);
        }

        public async Task<IEnumerable<Guid>> GetPostLikes(Guid postId)
        {
            var likes = await _context.LikesPosts.AsNoTracking()
                .Where(x => x.PostId == postId)
                .Select(x => x.AuthorId)
                .ToListAsync();
            return likes;
        }

        public async Task<List<ProfilePostModel>> GetCurrentUserPosts(Guid userId)
        {
            var posts = await _context.Posts
                .Include(x => x.PostContents).AsNoTracking()
                .Include(x => x.PostComments).ThenInclude(x => x.Author)
                .OrderByDescending(x => x.Created)
                .Where(x => x.AuthorId == userId)
                .Select(x => _mapper.Map<ProfilePostModel>(x))
                .ToListAsync();
            foreach (var post in posts)
            {
                var likes = await GetPostLikes(post.Id);
                post.LikedByMe = likes.Contains(userId) ? 1 : 0;
                post.LikeCount = likes.Count();
                post.CommentCount = post.Comments != null ? post.Comments.Count() : 0;
            }
            return posts;
        }

        public async Task RemoveLikeFromPost(Guid userId, Guid postId)
        {
            var like = await _context.LikesPosts
                .Where(x => x.PostId == postId && x.AuthorId == userId)
                .FirstOrDefaultAsync();
            if (like != null)
            {
                _context.LikesPosts.Remove(like);
                await _context.SaveChangesAsync();
            }
        }
    }
}
