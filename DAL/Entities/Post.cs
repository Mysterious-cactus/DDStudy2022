using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Post
    {
        public long Id { get; set; }
        public virtual User Author { get; set; } = null!;
        public List<Attach> PostAttaches { get; set; } = new List<Attach>();
        public string[] AttachPaths { get; set; }
        public string? Description { get; set; }
    }
}
