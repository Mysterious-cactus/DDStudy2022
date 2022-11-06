using DAL.Entities;

namespace Api.Models
{
    public class PostModel
    {
        public virtual User Author { get; set; }
        public List<MetadataModel>? PostAttaches { get; set; }
        public string? Description { get; set; }
    }
}
