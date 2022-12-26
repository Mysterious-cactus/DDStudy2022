using Api.Configs;
using Api.Models.Attach;
using Api.Models.Like;
using Api.Models.User;
using Api.Exceptions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Api.Models.Subcribes;

namespace Api.Services
{
    public class UserService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public UserService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<bool> CheckUserExist(string email)
        {

            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());

        }

        public async Task EnterAdditionalInfo(UserAdditionalInfoModel model, Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                user.Region = model.Region;
                user.City = model.City;
                await _context.SaveChangesAsync();
            }
        }

        public async Task Subscribe(Guid who, SubscriptionModel model)
        {
            var subcription = new Subscription { Who = who, OnWhom = model.OnWhom, Created = model.Created };
            await _context.Subscriptions.AddAsync(subcription);
            //var currentUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == who);
            //var userOnWhom = await _context.Users.FirstOrDefaultAsync(x => x.Id == model.OnWhom);
            //if (currentUser != null && userOnWhom != null)
            //{
            //    currentUser.Subscribes.ToList().Add(model.OnWhom);
            //    userOnWhom.Subscribers.ToList().Add(who);
            //}
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LikeModel>> GetCommentLikes(Guid commentId)
        {
            var likes = await _context.LikesComments.AsNoTracking()
                .Where(x => x.CommentId == commentId)
                .Select(x => _mapper.Map<LikeModel>(x))
                .ToListAsync();
            //var likes = from like in _context.LikesComments where like.CommentId == commentId select like;
            //List<LikeModel> likesList = new List<LikeModel>();
            //foreach (var like in likes)
            //{
            //    likesList.Add(_mapper.Map<LikeModel>(like));
            //}
            return likes;
        }

        public async Task AddLikeToComment(LikeModel model)
        {
            var like = new LikeComment { AuthorId = model.AuthorId, CommentId = model.EntityId };
            await _context.LikesComments.AddAsync(like);
            await _context.SaveChangesAsync();
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel meta, string filePath)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                var avatar = new Avatar
                {
                    Author = user,
                    MimeType = meta.MimeType,
                    FilePath = filePath,
                    Name = meta.Name,
                    Size = meta.Size
                };
                user.Avatar = avatar;

                await _context.SaveChangesAsync();
            }

        }
        public async Task<AttachModel> GetUserAvatar(Guid userId)
        {
            var user = await GetUserById(userId);
            var atach = _mapper.Map<AttachModel>(user.Avatar);
            return atach;
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
            var dbUser = _mapper.Map<User>(model);
            var t = await _context.Users.AddAsync(dbUser);
            await _context.SaveChangesAsync();
            return t.Entity.Id;
        }
        public async Task<IEnumerable<UserAvatarModel>> GetUsers() =>
            await _context.Users.AsNoTracking()
            .Include(x => x.Avatar)
            .Include(x => x.Posts)
            .Select(x => _mapper.Map<UserAvatarModel>(x))
            .ToListAsync();


        public async Task<UserAvatarModel> GetUser(Guid id)
        {
            var user = _mapper.Map<User, UserAvatarModel>(await GetUserById(id));
            user.Subscribers = await _context.Subscriptions.Where(x => x.OnWhom == id).Select(x => x.Who).ToListAsync();
            user.Subscriptions = await _context.Subscriptions.Where(x => x.Who == id).Select(x => x.OnWhom).ToListAsync();
            return user;
        }

        private async Task<User> GetUserById(Guid id)
        {
            var user = await _context.Users.Include(x => x.Avatar)
                .Include(x => x.Posts)
                //.Include(x => x.Subscribers)
                //.Include(x => x.Subscribes)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (user == null || user == default)
                throw new UserNotFoundException();
            return user;
        }

        public async Task<UserAvatarModel> FindUserByName(string username)
        {
            UserAvatarModel? user;
            var u = await _context.Users.Include(x => x.Avatar)
                .Include(x => x.Posts)
                .FirstOrDefaultAsync(x => x.Name == username);
            if (u == null || u == default)
            {
                user = null;
                throw new UserNotFoundException();
            } else
            {
                user = _mapper.Map<User, UserAvatarModel>(u);
                user.Subscribers = await _context.Subscriptions.Where(x => x.OnWhom == x.Id).Select(x => x.Who).ToListAsync();
                user.Subscriptions = await _context.Subscriptions.Where(x => x.Who == x.Id).Select(x => x.OnWhom).ToListAsync();
            }
            return user;
        }
    }
}
