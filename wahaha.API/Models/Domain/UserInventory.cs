namespace wahaha.API.Models.Domain
{
    public class UserInventory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ItemId { get; set; }
        public DateTime AcquiredDate { get; set; }
        public bool IsEquiped { get; set; }
    }
}
