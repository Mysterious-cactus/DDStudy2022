using Api.Configs;
using Api.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class UserService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;

        public UserService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;

            Console.WriteLine("us " + Guid.NewGuid());
        }

        private Func<UserModel, string?>? _linkGenerator;
        public void SetLinkGenerator(Func<UserModel, string?> linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public async Task<bool> CheckUserExist(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        public async Task AddCommentToPost(Guid userId, CommentModel model)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            var post = await _context.Posts.Include(x => x.PostComments).FirstOrDefaultAsync(x => x.Id == model.PostId);
            if (user != null && post != null)
            {
                var comment = new Comment { Author = user, CommentText = model.CommentText, Created = model.Created, PostId = model.PostId };
                post.PostComments.Add(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<GetCommentsRequestModel>> GetCommentsFromPost(Guid postId)
        {
            var post = await _context.Posts.Include(x => x.PostComments).Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == postId);
            List<GetCommentsRequestModel> comments = new List<GetCommentsRequestModel>();
            if (post != null)
            {
                foreach (var comment in post.PostComments)
                {
                    comments.Add(_mapper.Map<GetCommentsRequestModel>(comment));
                }
            }
            return comments;
        }

        public async Task AddPost(Guid userId, PostModel model, string[] filePaths)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                List<Attach> attaches = new List<Attach>();
                int i = 0;
                foreach (var item in model.PostAttaches)
                {
                    attaches.Add(new Attach { Author = user, FilePath = filePaths[i], MimeType = item.MimeType, Name = item.Name, Size = item.Size });
                    i++;
                }
                var post = new Post { Author = user, Description = model.Description, PostAttaches = attaches, AttachPaths = filePaths, Created = model.Created };
                user.Posts.Add(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddPost(Guid userId, PostModel model)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                var post = new Post { Author = user, Description = model.Description };
                user.Posts.Add(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<GetPostRequestModel>> GetPosts(Guid userId)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            List<GetPostRequestModel> posts = new List<GetPostRequestModel>();
            if (user != null && user.Posts != null)
            {
                foreach (var post in user.Posts)
                {
                    posts.Add(_mapper.Map<GetPostRequestModel>(post));
                }
            }
            return posts;
        }

        public async Task<GetPostRequestModel> GetPostById(Guid userId, Guid postId)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            var temp = await _context.Posts.FirstOrDefaultAsync((x => x.Id == postId));
            var post = _mapper.Map<GetPostRequestModel>(temp);
            return post;
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel meta, string filePath)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                var avatar = new Avatar { Author = user, MimeType = meta.MimeType, FilePath = filePath, Name = meta.Name, Size = meta.Size };
                user.Avatar = avatar;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<AttachModel> GetUserAvatar(Guid userId)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
            var attach = _mapper.Map<AttachModel>(user.Avatar);
            return attach;
        }

        public async Task Delete(Guid id)
        {
            var dbUser = await GetUserById(id);
            if (dbUser != null)
            {
                _context.Users.Remove(dbUser);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Guid> CreateUser(CreateUserModel model)
        {
            var dbUser = _mapper.Map<DAL.Entities.User>(model);
            var temp = await _context.Users.AddAsync(dbUser);
            await _context.SaveChangesAsync();
            return temp.Entity.Id;
        }
        public async Task<IEnumerable<UserAvatarModel>> GetUsers()
        {
            var users = await _context.Users.AsNoTracking().ProjectTo<UserModel>(_mapper.ConfigurationProvider).ToListAsync();
            return users.Select(x => new UserAvatarModel(x, _linkGenerator));
        }

        public async Task<UserAvatarModel> GetUser(Guid id)
        {
            var user = await GetUserById(id); ;
            return new UserAvatarModel(_mapper.Map<UserModel>(user), _linkGenerator);
        }

        private async Task<DAL.Entities.User> GetUserById(Guid id)
        {
            //ищем пользователя в таблице Users
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                throw new Exception("user not found");
            }
            return user;
        }
    }
}