using System;

namespace AiNutritionTracking.API.Models;

public partial class WaterReminder
{
    public int ReminderId { get; set; }
    public int? UserId { get; set; }
    public TimeOnly ReminderTime { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}