namespace wahaha.API.Models.Domain
{
    public class Users
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int TotalPointsEarned { get; set; }
        public int CurrentPointsBalance { get; set; }
        public int Level { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
