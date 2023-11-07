namespace Users.API.Dto.User
{
    public class UserMenuDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int GroupId { get; set; }
        public string StatusId { get; set; }
        public int SystemId { get; set; }
        public string DescriptionPerfil { get; set; }
        public int MenuId { get; set; }
        public string NameMenu { get; set; }
        public string TypeMenu { get; set; }
        public string UrlMenu { get; set; }
        public string DescriptionMenu { get; set; }
       
    }
}
