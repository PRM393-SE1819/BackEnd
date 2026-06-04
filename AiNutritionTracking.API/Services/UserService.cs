using System;
using System.Linq;
using System.Threading.Tasks;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.Health;
using AiNutritionTracking.API.Helpers;
using AiNutritionTracking.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AiNutritionTracking.API.Services
{
    public class UserService : IUserService
    {
        private readonly AinutritiontrackingContext _context;

        public UserService(AinutritiontrackingContext context)
        {
            _context = context;
        }

        // Other auth/user core methods can be implemented here...
    }
}
