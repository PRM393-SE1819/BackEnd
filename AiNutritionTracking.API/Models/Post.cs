using System;
using System.Collections.Generic;

namespace AiNutritionTracking.API.Models;

public partial class Post
{
    public int PostId { get; set; }

    public int? UserId { get; set; }

    public string? Content { get; set; }

    public string? ImageUrl { get; set; }

    public int? LikesCount { get; set; }

    public string? Status { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual User? User { get; set; }
}
