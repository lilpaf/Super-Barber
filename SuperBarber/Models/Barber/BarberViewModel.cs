namespace SuperBarber.Models.Barber
{
    public class BarberViewModel
    {
        public int BarberId { get; init; }

        public string BarberName { get; init; }

        public bool IsOwner { get; init; }
        
        public bool IsAvailable { get; init; }

        public string UserId { get; init; }
    }
}
