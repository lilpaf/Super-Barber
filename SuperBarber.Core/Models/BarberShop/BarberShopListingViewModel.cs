using SuperBarber.Core.Models.Interfaces;

namespace SuperBarber.Core.Models.BarberShop
{
    public class BarberShopListingViewModel : IBarberShopModel
    {
        public int Id { get; init; }

        public string BarberShopName { get; init; }

        public string City { get; init; }
        
        public string District { get; init; }

        public string Street { get; init; }

        public string StartHour { get; init; }

        public string FinishHour { get; init; }
        
        public string ImageName { get; init; }

        public bool UserIsEmployee { get; init; }
        
        public bool UserIsOwner { get; init; }
        
        public IEnumerable<OwnerListingViewModel> OwnersInfo { get; init; }
    }
}
