using System;
using System.Collections.Generic;
using Users.API.Common.Enum.User;
using Users.API.Dto.Profile;

namespace Users.API.Dto.User
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Cpf { get; set; }
        public string RegistrationNumber { get; set; }
        public UserTypeEnum UserType { get; set; }
        public string UserTypeDescription { get; set; }
        public string Password { get; set; }
        public string PasswordToken { get; set; }
        public DateTime? TokenExpirationDate { get; set; }
        public string FailedLogins { get; set; }
        public long? GroupId { get; set; }
        public string GroupName { get; set; }
        public long? ShopId { get; set; }
        public string ShopName { get; set; }
        public string ShopDocument { get; set; }
        public long? ProfileId { get; set; }
        public string Status { get; set; }
        public ProfileDto Profile { get; set; }

        public List<UserShopDto> UserShop { get; set; }

        public string MothersName { get; set; }
        public DateTime? BirthDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public UserDto CreatedByUser { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
