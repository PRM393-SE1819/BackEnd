using System;

namespace AiNutritionTracking.API.DTOs.Admin.FoodManagement
{
    public class AdminFoodQueryDto
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? FoodType { get; set; }
        public bool? IsVerified { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminFoodItemDto
    {
        public int FoodId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public double Calories { get; set; }
        public double? Protein { get; set; }
        public double? Carbs { get; set; }
        public double? Fat { get; set; }
        public string? FoodType { get; set; }
        public bool? IsVerified { get; set; }
        public string? Status { get; set; }
        public bool? IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByUsername { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class AdminFoodDetailDto : AdminFoodItemDto
    {
        public double? Fiber { get; set; }
        public double? Sugar { get; set; }
        public double? Sodium { get; set; }
        public string? ServingSize { get; set; }
        public string? Barcode { get; set; }
        public string? ImageUrl { get; set; }
        public int? UpdatedBy { get; set; }
        public string? UpdatedByUsername { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class AdminCreateFoodDto
    {
        public string? Name { get; set; } 
        public string? Description { get; set; }
        public double? Calories { get; set; }
        public double? Protein { get; set; }
        public double? Carbs { get; set; }
        public double? Fat { get; set; }
        public double? Fiber { get; set; }
        public double? Sugar { get; set; }
        public double? Sodium { get; set; }
        public string? ServingSize { get; set; }
        public string? Barcode { get; set; }
        public string? ImageUrl { get; set; }
        public string? FoodType { get; set; }
    }

    public class AdminUpdateFoodStatusDto
    {
        public string Status { get; set; } = null!;
    }
    public class UploadImageDto
    {
        public IFormFile File { get; set; } = null!;
    }
}