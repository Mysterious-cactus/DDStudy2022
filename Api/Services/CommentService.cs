using Api.Models;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

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
    }
}
