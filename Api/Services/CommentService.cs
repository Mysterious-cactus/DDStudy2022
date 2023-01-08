using Api.Models;
using Api.Models.Like;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace Api.Services
{
    public class CommentService
    {
        private readonly DAL.DataContext _context;
        private readonly IMapper _mapper;

        public CommentService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task AddCommentToPost(Guid userId, CommentModel model)
        {
            var user = await _context.Users.Include(x => x.Posts).FirstOrDefaultAsync(x => x.Id == userId);
            var post = await _context.Posts.Include(x => x.PostComments).FirstOrDefaultAsync(x => x.Id == model.PostId);
            if (user != null && post != null)
            {
                var comment = new Comment { Author = user, CommentText = model.CommentText, Created = DateTimeOffset.UtcNow, PostId = model.PostId };
                post.PostComments.Add(comment);
                await _context.SaveChangesAsync();
            }
        }

        //public async Task<List<GetCommentsRequestModel>> GetCommentsFromPost(Guid postId)
        //{
        //    var comments = await _context.Comments
        //        .Include(x => x.Author).ThenInclude(x => x.Avatar)
        //        .Where(x => x.PostId == postId)
        //        .Select(x => _mapper.Map<GetCommentsRequestModel>(x))
        //        .ToListAsync();
        //    return comments;
        //}

        public async Task<IEnumerable<Guid>> GetCommentLikes(Guid commentId)
        {
            var likes = await _context.LikesComments
                .Where(x => x.CommentId == commentId)
                .Select(x => x.AuthorId)
                .ToListAsync();
            return likes;
        }

        public async Task AddLikeToComment(Guid userId, Guid commentId)
        {
            var like = new LikeComment { AuthorId = userId, CommentId = commentId};
            await _context.LikesComments.AddAsync(like);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveLikeFromComment(Guid userId, Guid commentId)
        {
            var like = await _context.LikesComments
                .Where(x=> x.CommentId == commentId && x.AuthorId == userId)
                .FirstOrDefaultAsync();
            if (like != null)
            {
                _context.LikesComments.Remove(like);
                await _context.SaveChangesAsync();
            }
        }
    }
}
