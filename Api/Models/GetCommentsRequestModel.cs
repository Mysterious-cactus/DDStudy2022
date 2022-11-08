namespace Api.Models
{
    public class GetCommentsRequestModel
    {
        public string CommentText { get; set; }
        public DateTimeOffset Created { get; set; }
        public Guid UserId { get; set; }
    }
}
