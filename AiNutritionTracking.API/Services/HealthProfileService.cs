using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Health;
using AiNutritionTracking.API.Helpers;
using AiNutritionTracking.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AiNutritionTracking.API.Services
{
    public class HealthProfileService : IHealthProfileService
    {
        private readonly AinutritiontrackingContext _context;

        public HealthProfileService(AinutritiontrackingContext context)
        {
            _context = context;
        }

        public async Task<UserProfileResponseDTO?> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsDeleted == true) return null;

            double? bmi = user.Weight.HasValue && user.Height.HasValue && user.Height > 0 
                ? NutritionCalculator.CalculateBMI(user.Weight.Value, user.Height.Value) 
                : null;

            return new UserProfileResponseDTO
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Height = user.Height,
                Weight = user.Weight,
                ActivityLevel = user.ActivityLevel,
                Goal = user.Goal,
                TargetWeight = user.TargetWeight,
                CaloriesTarget = user.CaloriesTarget,
                ProteinTarget = user.ProteinTarget,
                CarbTarget = user.CarbTarget,
                FatTarget = user.FatTarget,
                BMI = bmi,
                BodyFat = user.BodyFat
            };
        }

        public async Task<(bool Success, string Message, UserProfileResponseDTO? Data)> UpdateUserProfileAsync(int userId, UpdateUserProfileDTO request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.IsDeleted == true)
                return (false, "Người dùng không tồn tại hoặc đã bị khóa.", null);

            if (request.Goal == "LoseWeight" && request.TargetWeight >= request.Weight)
                return (false, "Với mục tiêu giảm cân, cân nặng mục tiêu cần nhỏ hơn cân nặng hiện tại.", null);
                
            if (request.Goal == "GainWeight" && request.TargetWeight <= request.Weight)
                return (false, "Với mục tiêu tăng cân, cân nặng mục tiêu cần lớn hơn cân nặng hiện tại.", null);

            if (user.Weight != request.Weight)
            {
                var weightLog = new WeightLog
                {
                    UserId = userId,
                    Weight = request.Weight,
                    LoggedAt = DateTime.UtcNow
                };
                _context.WeightLogs.Add(weightLog);
            }

            user.Gender = request.Gender;
            user.DateOfBirth = request.DateOfBirth;
            user.Height = request.Height;
            user.Weight = request.Weight;
            user.ActivityLevel = request.ActivityLevel;
            user.Goal = request.Goal;
            user.TargetWeight = request.TargetWeight;

            int age = NutritionCalculator.CalculateAge(request.DateOfBirth);
            double tdee = NutritionCalculator.CalculateTDEE(request.Gender, request.Weight, request.Height, age, request.ActivityLevel);
            
            // Tính BMI và BodyFat
            double bmi = NutritionCalculator.CalculateBMI(request.Weight, request.Height);
            user.BodyFat = NutritionCalculator.CalculateBodyFat(bmi, age, request.Gender);

            var targets = NutritionCalculator.CalculateTargets(tdee, request.Goal);
            
            user.CaloriesTarget = targets.Calories;
            user.ProteinTarget = targets.Protein;
            user.CarbTarget = targets.Carbs;
            user.FatTarget = targets.Fat;
            
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = new UserProfileResponseDTO
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Height = user.Height,
                Weight = user.Weight,
                ActivityLevel = user.ActivityLevel,
                Goal = user.Goal,
                TargetWeight = user.TargetWeight,
                CaloriesTarget = user.CaloriesTarget,
                ProteinTarget = user.ProteinTarget,
                CarbTarget = user.CarbTarget,
                FatTarget = user.FatTarget,
                BMI = bmi,
                BodyFat = user.BodyFat
            };

            return (true, "Cập nhật hồ sơ dinh dưỡng sức khỏe thành công và đã tự động tính toán nhu cầu/BMI của bạn.", responseDto);
        }

        public async Task<List<HealthConditionResponseDTO>> GetHealthConditionsAsync(int userId)
        {
            return await _context.UserHealthConditions
                .Where(c => c.UserId == userId)
                .Select(c => new HealthConditionResponseDTO
                {
                    ConditionId = c.ConditionId,
                    ConditionName = c.ConditionName!,
                    Notes = c.Notes,
                    CreatedAt = c.CreatedAt
                }).ToListAsync();
        }

        public async Task<(bool Success, string Message, HealthConditionResponseDTO? Data)> AddHealthConditionAsync(int userId, AddHealthConditionDTO request)
        {
            var exists = await _context.UserHealthConditions
                .AnyAsync(c => c.UserId == userId && c.ConditionName != null && c.ConditionName.ToLower() == request.ConditionName.ToLower());
            
            if (exists) return (false, "Bệnh lý này đã tồn tại trong hồ sơ của bạn.", null);

            var condition = new UserHealthCondition
            {
                UserId = userId,
                ConditionName = request.ConditionName.Trim(),
                Notes = request.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.UserHealthConditions.Add(condition);
            await _context.SaveChangesAsync();

            var response = new HealthConditionResponseDTO
            {
                ConditionId = condition.ConditionId,
                ConditionName = condition.ConditionName!,
                Notes = condition.Notes,
                CreatedAt = condition.CreatedAt
            };

            return (true, "Thêm bệnh lý thành công.", response);
        }

        public async Task<(bool Success, string Message)> UpdateHealthConditionAsync(int userId, int conditionId, UpdateHealthConditionDTO request)
        {
            var condition = await _context.UserHealthConditions.FindAsync(conditionId);
            if (condition == null) return (false, "Không tìm thấy bệnh lý.");

            if (condition.UserId != userId) return (false, "Bạn không có quyền sửa dữ liệu này.");
            // Logic: Chỉ cập nhật khi Client truyền giá trị thực sự (khác null/rỗng)
            if (!string.IsNullOrWhiteSpace(request.ConditionName))
            {
                condition.ConditionName = request.ConditionName.Trim();
            }

            // Logic: Nếu muốn cho phép người dùng xóa note (để trống), 
            // thì không dùng IsNullOrWhiteSpace mà chỉ check null.
            // NHƯNG nếu muốn "chỉ cập nhật khi có nội dung mới" thì dùng như dưới:
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                condition.Notes = request.Notes.Trim();
            }
            await _context.SaveChangesAsync();
            return (true, "Cập nhật bệnh lý thành công.");
        }

        public async Task<(bool Success, string Message)> DeleteHealthConditionAsync(int userId, int conditionId)
        {
            var condition = await _context.UserHealthConditions.FindAsync(conditionId);
            if (condition == null) return (false, "Không tìm thấy bệnh lý.");
            if (condition.UserId != userId) return (false, "Bạn không có quyền xóa dữ liệu này.");

            _context.UserHealthConditions.Remove(condition);
            await _context.SaveChangesAsync();
            return (true, "Đã xóa bệnh lý khỏi hồ sơ.");
        }

        public async Task<List<AllergyResponseDTO>> GetAllergiesAsync(int userId)
        {
            return await _context.Allergies
                .Where(a => a.UserId == userId)
                .Select(a => new AllergyResponseDTO
                {
                    AllergyId = a.AllergyId,
                    AllergyName = a.AllergyName!,
                    CreatedAt = a.CreatedAt
                }).ToListAsync();
        }

        public async Task<(bool Success, string Message, AllergyResponseDTO? Data)> AddAllergyAsync(int userId, AddAllergyDTO request)
        {
            var exists = await _context.Allergies
                .AnyAsync(a => a.UserId == userId && a.AllergyName != null && a.AllergyName.ToLower() == request.AllergyName.ToLower());
            
            if (exists) return (false, "Dị ứng này đã tồn tại trong hồ sơ của bạn.", null);

            var allergy = new Allergy
            {
                UserId = userId,
                AllergyName = request.AllergyName.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Allergies.Add(allergy);
            await _context.SaveChangesAsync();

            var response = new AllergyResponseDTO
            {
                AllergyId = allergy.AllergyId,
                AllergyName = allergy.AllergyName!,
                CreatedAt = allergy.CreatedAt
            };

            return (true, "Thêm dị ứng thành công.", response);
        }

        public async Task<(bool Success, string Message)> UpdateAllergyAsync(int userId, int allergyId, AddAllergyDTO request)
        {
            var allergy = await _context.Allergies.FindAsync(allergyId);
            if (allergy == null) return (false, "Không tìm thấy thông tin dị ứng.");
            if (allergy.UserId != userId) return (false, "Bạn không có quyền sửa dữ liệu này.");

            allergy.AllergyName = request.AllergyName.Trim();
            
            await _context.SaveChangesAsync();
            return (true, "Cập nhật dị ứng thành công.");
        }

        public async Task<(bool Success, string Message)> DeleteAllergyAsync(int userId, int allergyId)
        {
            var allergy = await _context.Allergies.FindAsync(allergyId);
            if (allergy == null) return (false, "Không tìm thấy thông tin dị ứng.");
            if (allergy.UserId != userId) return (false, "Bạn không có quyền xóa dữ liệu này.");

            _context.Allergies.Remove(allergy);
            await _context.SaveChangesAsync();
            return (true, "Đã xóa dị ứng khỏi hồ sơ.");
        }
    }
}
