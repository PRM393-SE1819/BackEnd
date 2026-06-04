using System.ComponentModel.DataAnnotations;

namespace AiNutritionTracking.API.DTOs.Health
{
    public class AddAllergyDTO
    {
        [Required(ErrorMessage = "Tên dị ứng không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên dị ứng không được quá 100 ký tự")]
        public string AllergyName { get; set; } = null!;
    }
}
