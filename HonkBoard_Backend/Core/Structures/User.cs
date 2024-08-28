using System.Runtime.Serialization;

namespace HonkBoard_Backend.Core.Structures
{
    public class User
    {
        public string? Avatar { get; set; }

        public string? Name { get; set; }

        // public Image? avatar;
        public string? Id { get; set; }

        public User(string? avatar = null, string? name = null, string? id = null)
        {

            Avatar = avatar;
            Name = name;
            Id = id;

        }

        public User(User user)
        {

            Avatar = user.Avatar;
            Name = user.Name;
            Id = user.Id;

        }

        public User() { }

        public bool IsEmpty()
        {

            return string.IsNullOrEmpty(Id);

        }

        public User GetUser() => this;
    }
}
