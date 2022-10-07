namespace SuperBarber.Data.Models
{
    public class BarberShopServices
    {
        public Guid BarberShopId { get; set; }
        public BarberShop BarberShop { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}
