namespace Users.API.Entities
{
    public class ProfileMenu
    {
        public long Id { get; set; }
        public long ProfileId { get; set; }
        public long MenuId { get; set; }
        public bool CanInsert { get; set; }
        public bool CanDelete { get; set; }
        public bool CanUpdate { get; set; }
    }
}
