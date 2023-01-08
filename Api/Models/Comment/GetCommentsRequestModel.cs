using Api.Models.User;

namespace Api.Models
{
    public class GetCommentsRequestModel
    {
        public Guid Id { get; set; }
        public string CommentText { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public UserAvatarModel Author { get; set; } = null!;
    }
}
