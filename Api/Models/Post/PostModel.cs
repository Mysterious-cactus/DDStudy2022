using Api.Models.Attach;
using Api.Models.User;

namespace Api.Models.Post
{
    public class ProfilePostModel
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public List<AttachExternalModel>? Contents { get; set; } = new List<AttachExternalModel>();
        public List<GetCommentsRequestModel>? Comments { get; set; } = new List<GetCommentsRequestModel>();
        public long LikeCount { get; set; }
        public long CommentCount { get; set; }
        public DateTimeOffset Created { get; set; }
        public int LikedByMe { get; set; } = 0;
    }

    public class PostModel : ProfilePostModel
    {
        public UserAvatarModel Author { get; set; } = null!;
    }   
}
