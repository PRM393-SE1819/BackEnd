using System;

namespace AiNutritionTracking.API.DTOs.Water
{
    public class WaterReminderDto
    {
        public int ReminderId { get; set; }
        public TimeOnly ReminderTime { get; set; }
        public bool IsEnabled { get; set; }
    }
}
