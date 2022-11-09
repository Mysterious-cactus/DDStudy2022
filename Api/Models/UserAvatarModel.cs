using System.Xml.Linq;

namespace Api.Models
{
    public class UserAvatarModel : UserModel
    {
        public string? AvatarLink { get; set; }
        public UserAvatarModel(UserModel model, Func<UserModel, string?>? linkGenerator)
        {
            Id = model.Id;
            Name = model.Name;
            Email = model.Email;
            BirthDay = model.BirthDay;
            AvatarLink = linkGenerator?.Invoke(model);
        }
    }
}
