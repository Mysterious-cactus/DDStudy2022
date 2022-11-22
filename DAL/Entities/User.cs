using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "empty";
        public string Email { get; set; } = "empty";
        public string PasswordHash { get; set; } = "empty"; 
        public DateTimeOffset BirthDay { get; set; }
        public string Region { get; set; } = "empty";
        public string City { get; set; } = "empty";
        public virtual Avatar? Avatar { get; set; }
        public virtual ICollection<UserSession>? Sessions { get; set; }
        public virtual ICollection<Post>? Posts { get; set; }
        //public virtual ICollection<Guid> Subscribes { get; set; } = new List<Guid>();
        //public virtual ICollection<Guid> Subscribers { get; set; } = new List<Guid>();
    }
}
