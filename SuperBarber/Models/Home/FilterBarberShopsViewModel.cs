using System.ComponentModel.DataAnnotations;

namespace SuperBarber.Models.Home
{
    public class FilterBarberShopsViewModel
    {
        public int City { get; init; }
        
        public IEnumerable<BarberShopCityViewModel> Cities { get; set; }

        public int District { get; init; }

        public IEnumerable<BarberShopDistrictViewModel> Districts { get; set; }

        [Required]
        public string Date { get; init; }

        [Required]
        public string Time { get; init; }
    }
}
