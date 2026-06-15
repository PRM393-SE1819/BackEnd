using System;
using System.Collections.Generic;
using AiNutritionTracking.API.Configurations;
using AiNutritionTracking.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AiNutritionTracking.API.Data;

public partial class AinutritiontrackingContext : DbContext
{
    public AinutritiontrackingContext(DbContextOptions<AinutritiontrackingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AichatMessage> AichatMessages { get; set; }

    public virtual DbSet<AichatSession> AichatSessions { get; set; }

    public virtual DbSet<AiextractedField> AiextractedFields { get; set; }

    public virtual DbSet<Airecommendation> Airecommendations { get; set; }

    public virtual DbSet<Airequest> Airequests { get; set; }

    public virtual DbSet<Airesponse> Airesponses { get; set; }

    public virtual DbSet<Allergy> Allergies { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<ExerciseLog> ExerciseLogs { get; set; }

    public virtual DbSet<FavoriteFood> FavoriteFoods { get; set; }

    public virtual DbSet<Food> Foods { get; set; }

    public virtual DbSet<Meal> Meals { get; set; }

    public virtual DbSet<MealItem> MealItems { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RecipeItem> RecipeItems { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserHealthCondition> UserHealthConditions { get; set; }

    public virtual DbSet<WaterLog> WaterLogs { get; set; }

    public virtual DbSet<WaterGoal> WaterGoals { get; set; }

    public virtual DbSet<WaterReminder> WaterReminders { get; set; }

    public virtual DbSet<WeightLog> WeightLogs { get; set; }

    public virtual DbSet<BodyFatAnalysis> BodyFatAnalyses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BodyFatAnalysisConfiguration());
        modelBuilder.Entity<AichatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("AIChatMessages_pkey");

            entity.ToTable("AIChatMessages");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.SenderType).HasMaxLength(20);

            entity.HasOne(d => d.Session).WithMany(p => p.AichatMessages)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("AIChatMessages_SessionId_fkey");
        });

        modelBuilder.Entity<AichatSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("AIChatSessions_pkey");

            entity.ToTable("AIChatSessions");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.AichatSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("AIChatSessions_UserId_fkey");
        });

        modelBuilder.Entity<AiextractedField>(entity =>
        {
            entity.HasKey(e => e.FieldId).HasName("AIExtractedFields_pkey");

            entity.ToTable("AIExtractedFields");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.FieldType).HasMaxLength(100);
            entity.Property(e => e.ParsedValue).HasMaxLength(500);
            entity.Property(e => e.RawValue).HasMaxLength(500);

            entity.HasOne(d => d.Response).WithMany(p => p.AiextractedFields)
                .HasForeignKey(d => d.ResponseId)
                .HasConstraintName("AIExtractedFields_ResponseId_fkey");
        });

        modelBuilder.Entity<Airecommendation>(entity =>
        {
            entity.HasKey(e => e.RecommendationId).HasName("AIRecommendations_pkey");

            entity.ToTable("AIRecommendations");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.RecommendationType).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Active'::character varying");

            entity.HasOne(d => d.User).WithMany(p => p.Airecommendations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("AIRecommendations_UserId_fkey");
        });

        modelBuilder.Entity<Airequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("AIRequests_pkey");

            entity.ToTable("AIRequests");

            entity.Property(e => e.Aimodel)
                .HasMaxLength(100)
                .HasColumnName("AIModel");
            entity.Property(e => e.Aiprovider)
                .HasMaxLength(50)
                .HasColumnName("AIProvider");
            entity.Property(e => e.InputImageUrl).HasMaxLength(500);
            entity.Property(e => e.RequestType).HasMaxLength(50);
            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Airequests)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("AIRequests_UserId_fkey");
        });

        modelBuilder.Entity<Airesponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("AIResponses_pkey");

            entity.ToTable("AIResponses");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Request).WithMany(p => p.Airesponses)
                .HasForeignKey(d => d.RequestId)
                .HasConstraintName("AIResponses_RequestId_fkey");
        });

        modelBuilder.Entity<Allergy>(entity =>
        {
            entity.HasKey(e => e.AllergyId).HasName("Allergies_pkey");

            entity.Property(e => e.AllergyName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.User).WithMany(p => p.Allergies)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Allergies_UserId_fkey");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("Comments_pkey");

            entity.Property(e => e.Content).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("Comments_PostId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Comments_UserId_fkey");
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.ExerciseId).HasName("Exercises_pkey");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ExerciseName).HasMaxLength(150);
        });

        modelBuilder.Entity<ExerciseLog>(entity =>
        {
            entity.HasKey(e => e.ExerciseLogId).HasName("ExerciseLogs_pkey");

            entity.Property(e => e.LoggedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Exercise).WithMany(p => p.ExerciseLogs)
                .HasForeignKey(d => d.ExerciseId)
                .HasConstraintName("ExerciseLogs_ExerciseId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ExerciseLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("ExerciseLogs_UserId_fkey");
        });

        modelBuilder.Entity<FavoriteFood>(entity =>
        {
            entity.HasKey(e => e.FavoriteFoodId).HasName("FavoriteFoods_pkey");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Food).WithMany(p => p.FavoriteFoods)
                .HasForeignKey(d => d.FoodId)
                .HasConstraintName("FavoriteFoods_FoodId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.FavoriteFoods)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FavoriteFoods_UserId_fkey");
        });

        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasKey(e => e.FoodId).HasName("Foods_pkey");

            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.FoodType).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.ServingSize).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Active'::character varying");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.FoodCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("Foods_CreatedBy_fkey");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.FoodUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("Foods_UpdatedBy_fkey");
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.MealId).HasName("Meals_pkey");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.MealType).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(d => d.User).WithMany(p => p.Meals)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Meals_UserId_fkey");
        });

        modelBuilder.Entity<MealItem>(entity =>
        {
            entity.HasKey(e => e.MealItemId).HasName("MealItems_pkey");

            entity.HasOne(d => d.Food).WithMany(p => p.MealItems)
                .HasForeignKey(d => d.FoodId)
                .HasConstraintName("MealItems_FoodId_fkey");

            entity.HasOne(d => d.Meal).WithMany(p => p.MealItems)
                .HasForeignKey(d => d.MealId)
                .HasConstraintName("MealItems_MealId_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("Notifications_pkey");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Sent'::character varying");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Notifications_UserId_fkey");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("Posts_pkey");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LikesCount).HasDefaultValue(0);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Active'::character varying");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Posts_UserId_fkey");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.RecipeId).HasName("Recipes_pkey");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.RecipeName).HasMaxLength(200);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Pending'::character varying");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.User).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Recipes_UserId_fkey");
        });

        modelBuilder.Entity<RecipeItem>(entity =>
        {
            entity.HasKey(e => e.RecipeItemId).HasName("RecipeItems_pkey");

            entity.Property(e => e.Unit).HasMaxLength(50);

            entity.HasOne(d => d.Food).WithMany(p => p.RecipeItems)
                .HasForeignKey(d => d.FoodId)
                .HasConstraintName("RecipeItems_FoodId_fkey");

            entity.HasOne(d => d.Recipe).WithMany(p => p.RecipeItems)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("RecipeItems_RecipeId_fkey");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("Reports_pkey");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Pending'::character varying");
            entity.Property(e => e.TargetType).HasMaxLength(50);

            entity.HasOne(d => d.ReporterUser).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ReporterUserId)
                .HasConstraintName("Reports_ReporterUserId_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("Roles_pkey");

            entity.HasIndex(e => e.RoleName, "Roles_RoleName_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.HasIndex(e => e.Username, "Users_Username_key").IsUnique();

            entity.Property(e => e.ActivityLevel).HasMaxLength(50);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EmailVerified).HasDefaultValue(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.Goal).HasMaxLength(50);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Active'::character varying");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("Users_RoleId_fkey");
        });

        modelBuilder.Entity<UserHealthCondition>(entity =>
        {
            entity.HasKey(e => e.ConditionId).HasName("UserHealthConditions_pkey");

            entity.Property(e => e.ConditionName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.UserHealthConditions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("UserHealthConditions_UserId_fkey");
        });

        modelBuilder.Entity<WaterLog>(entity =>
        {
            entity.HasKey(e => e.WaterLogId).HasName("WaterLogs_pkey");

            entity.Property(e => e.AmountMl).HasColumnName("AmountML");
            entity.Property(e => e.LoggedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.User).WithMany(p => p.WaterLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("WaterLogs_UserId_fkey");
        });

        modelBuilder.Entity<WaterGoal>(entity =>
        {
            entity.HasKey(e => e.GoalId).HasName("WaterGoals_pkey");

            entity.Property(e => e.DailyTargetMl).HasColumnName("DailyTargetML");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.User).WithMany(p => p.WaterGoals)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("WaterGoals_UserId_fkey");
        });

        modelBuilder.Entity<WaterReminder>(entity =>
        {
            entity.HasKey(e => e.ReminderId).HasName("WaterReminders_pkey");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.User).WithMany(p => p.WaterReminders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("WaterReminders_UserId_fkey");
        });

        modelBuilder.Entity<WeightLog>(entity =>
        {
            entity.HasKey(e => e.WeightLogId).HasName("WeightLogs_pkey");

            entity.Property(e => e.LoggedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.User).WithMany(p => p.WeightLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("WeightLogs_UserId_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
