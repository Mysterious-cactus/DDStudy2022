using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Subscribe
    {
        public Guid Id { get; set; }
        public Guid Who { get; set; }
        public Guid OnWhom { get; set; }
        public DateTimeOffset Created { get; set; } 
    }
}
