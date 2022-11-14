﻿using Api.Configs;
using Api.Models.Attach;
using Api.Models.Like;
using Api.Models.User;
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
        private readonly DataContext _context;
        private Func<User, string?>? _linkGenerator;
        public void SetLinkGenerator(Func<User, string?> linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }


        public UserService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<bool> CheckUserExist(string email)
        {

            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());

        }

        public List<LikeModel> GetPostLikes(Guid postId)
        {
            //var likes = _context.LikesPosts.Include(x => x.PostId).Include(x => x.AuthorId).TakeWhile(x => x.PostId == postId);
            var likes = from like in _context.LikesPosts where like.PostId == postId select like;
            List<LikeModel> likesList = new List<LikeModel>();
            foreach (var like in likes)
            {
                likesList.Add(_mapper.Map<LikeModel>(like));
            }
            return likesList;
        }

        public List<LikeModel> GetCommentLikes(Guid commentId)
        {
            var likes = from like in _context.LikesComments where like.CommentId == commentId select like;
            List<LikeModel> likesList = new List<LikeModel>();
            foreach (var like in likes)
            {
                likesList.Add(_mapper.Map<LikeModel>(like));
            }
            return likesList;
        }

        public async Task AddLikeToPost(LikeModel model)
        {
            var like = new LikePost { AuthorId = model.AuthorId, PostId = model.EntityId };
            await _context.LikesPosts.AddAsync(like);
            await _context.SaveChangesAsync();
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
            var dbUser = _mapper.Map<DAL.Entities.User>(model);
            var t = await _context.Users.AddAsync(dbUser);
            await _context.SaveChangesAsync();
            return t.Entity.Id;
        }
        public async Task<IEnumerable<UserAvatarModel>> GetUsers() =>
            (await _context.Users.AsNoTracking().Include(x => x.Avatar).ToListAsync())
                .Select(x => _mapper.Map<User, UserAvatarModel>(x, o => o.AfterMap(FixAvatar)));


        public async Task<UserAvatarModel> GetUser(Guid id) =>
            _mapper.Map<User, UserAvatarModel>(await GetUserById(id), o => o.AfterMap(FixAvatar));



        private void FixAvatar(User s, UserAvatarModel d)
        {
            d.AvatarLink = s.Avatar == null ? null : _linkGenerator?.Invoke(s);
        }

        private async Task<User> GetUserById(Guid id)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == id);
            if (user == null || user == default)
                throw new Exception("user not found");
            return user;
        }
    }
}
