using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class UserSportRole
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public SportRoleType RoleType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}