using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public virtual User Author { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public List<Attach> PostAttaches { get; set; } = new List<Attach>();
        public List<Comment> PostComments { get; set; } = new List<Comment>();
        public string[] AttachPaths { get; set; }
        public string? Description { get; set; }
    }
}
