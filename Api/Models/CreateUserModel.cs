using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class CreateUserModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [Compare(nameof(Password))]
        public string RetryPassword { get; set; }
        public DateTimeOffset BirthDay { get; set; }

        public CreateUserModel (string name, string email, string password, string retryPassword, DateTimeOffset BirthDay)
        {
            Name = name;
            Email = email;
            Password = password;
            RetryPassword = retryPassword;
            BirthDay = BirthDay;
        }
    }
}
