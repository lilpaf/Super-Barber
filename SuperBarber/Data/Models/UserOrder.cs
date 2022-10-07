namespace SuperBarber.Data.Models
{
    public class UserOrder
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}
