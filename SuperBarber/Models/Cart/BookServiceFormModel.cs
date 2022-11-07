using System.ComponentModel.DataAnnotations;

namespace SuperBarber.Models.Cart
{
    public class BookServiceFormModel
    {
        [Required]
        public string Date { get; init; }

        [Required]
        public string Time { get; init; }
    }
}
