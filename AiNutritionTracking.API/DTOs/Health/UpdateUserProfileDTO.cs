using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AiNutritionTracking.API.DTOs.Health
{
    public class UpdateUserProfileDTO : IValidatableObject
    {
        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Giới tính phải là Male hoặc Female.")]
        public string Gender { get; set; } = null!;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        [Range(50, 300, ErrorMessage = "Chiều cao phải từ 50cm đến 300cm.")]
        public double Height { get; set; }

        [Required]
        [Range(20, 500, ErrorMessage = "Cân nặng phải từ 20kg đến 500kg.")]
        public double Weight { get; set; }

        [Required]
        [RegularExpression("^(Sedentary|LightlyActive|ModeratelyActive|VeryActive|ExtraActive)$", 
            ErrorMessage = "Cấp độ hoạt động không hợp lệ.")]
        public string ActivityLevel { get; set; } = null!;

        [Required]
        [RegularExpression("^(LoseWeight|MaintainWeight|GainWeight)$", 
            ErrorMessage = "Mục tiêu không hợp lệ.")]
        public string Goal { get; set; } = null!;

        [Range(20, 500, ErrorMessage = "Cân nặng mục tiêu (nếu có) phải từ 20kg đến 500kg.")]
        public double? TargetWeight { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            if (DateOfBirth >= today)
            {
                yield return new ValidationResult("Ngày sinh không thể là ngày hôm nay hoặc trong tương lai.", new[] { nameof(DateOfBirth) });
            }
            else if (DateOfBirth > today.AddYears(-10))
            {
                yield return new ValidationResult("Bạn phải từ đủ 10 tuổi trở lên để có thể thiết lập Hồ sơ dinh dưỡng.", new[] { nameof(DateOfBirth) });
            }
            else if (DateOfBirth < today.AddYears(-120))
            {
                yield return new ValidationResult("Độ tuổi không được vượt quá 120 tuổi.", new[] { nameof(DateOfBirth) });
            }

           
            if (TargetWeight.HasValue)
            {
                if (Goal == "LoseWeight" && TargetWeight >= Weight)
                {
                    yield return new ValidationResult("Với mục tiêu giảm cân, Cân nặng mục tiêu phải NHỎ HƠN Cân nặng hiện tại.", new[] { nameof(TargetWeight) });
                }
                else if (Goal == "GainWeight" && TargetWeight <= Weight)
                {
                    yield return new ValidationResult("Với mục tiêu tăng cân, Cân nặng mục tiêu phải LỚN HƠN Cân nặng hiện tại.", new[] { nameof(TargetWeight) });
                }
            }
        }
    }
}
