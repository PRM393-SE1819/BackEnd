using System.ComponentModel.DataAnnotations;

namespace AiNutritionTracking.API.DTOs.Health
{
    public class AddHealthConditionDTO
    {
        [Required(ErrorMessage = "Tên bệnh lý không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên bệnh lý không được quá 100 ký tự")]
        public string ConditionName { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? Notes { get; set; }
    }
}
