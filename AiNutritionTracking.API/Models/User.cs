using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? RoleId { get; set; }

    public string Username { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public double? Height { get; set; }

    public double? Weight { get; set; }

    public double? TargetWeight { get; set; }

    public double? BodyFat { get; set; }

    public string? ActivityLevel { get; set; }

    public string? Goal { get; set; }

    public string? AvatarUrl { get; set; }

    public int? CaloriesTarget { get; set; }

    public double? ProteinTarget { get; set; }

    public double? CarbTarget { get; set; }

    public double? FatTarget { get; set; }

    public bool? EmailVerified { get; set; }

    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    public virtual ICollection<AichatSession> AichatSessions { get; set; } = new List<AichatSession>();

    public virtual ICollection<Airecommendation> Airecommendations { get; set; } = new List<Airecommendation>();

    public virtual ICollection<Airequest> Airequests { get; set; } = new List<Airequest>();

    public virtual ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<ExerciseLog> ExerciseLogs { get; set; } = new List<ExerciseLog>();

    public virtual ICollection<FavoriteFood> FavoriteFoods { get; set; } = new List<FavoriteFood>();

    public virtual ICollection<Food> FoodCreatedByNavigations { get; set; } = new List<Food>();

    public virtual ICollection<Food> FoodUpdatedByNavigations { get; set; } = new List<Food>();

    public virtual ICollection<Meal> Meals { get; set; } = new List<Meal>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<UserHealthCondition> UserHealthConditions { get; set; } = new List<UserHealthCondition>();

    public virtual ICollection<WaterLog> WaterLogs { get; set; } = new List<WaterLog>();

    public virtual ICollection<WaterGoal> WaterGoals { get; set; } = new List<WaterGoal>();

    public virtual ICollection<WaterReminder> WaterReminders { get; set; } = new List<WaterReminder>();

    public virtual ICollection<WeightLog> WeightLogs { get; set; } = new List<WeightLog>();
}
