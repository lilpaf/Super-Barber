using System.ComponentModel.DataAnnotations;

namespace SuperBarber.Models.Home
{
    public class FilterBarberShopsViewModel
    {
        [Required]
        public string City { get; init; }
        
        public IEnumerable<string> Cities { get; init; }

        [Required]
        public string District { get; init; }

        public IEnumerable<string> Districts { get; init; }

        [Required]
        public string Date { get; init; }

        [Required]
        public string Time { get; init; }
        
        public bool IsFound { get; init; }
    }
}
