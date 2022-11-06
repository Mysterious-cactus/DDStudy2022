using DAL.Entities;

namespace Api.Models
{
    public class GetPostRequestModel
    {
        public virtual Guid UserId { get; set; }
        public string[] AttachPaths { get; set; }
        public string? Description { get; set; }
    }
}
