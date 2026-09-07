using System.ComponentModel.DataAnnotations;

namespace EventParkingReservationSystem.API.Models.Core;

public class User
{
    public int Id { get; set; }

    // ============================================
    // BASIC USER DETAILS
    // ============================================

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;


    // ============================================
    // ROLE
    // ============================================

    public UserRole Role { get; set; }
        = UserRole.Customer;


    // ============================================
    // ACCOUNT STATUS
    // ============================================

    public bool IsActive { get; set; } = true;


    // ============================================
    // CREATED / UPDATED
    // ============================================

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }


    // ============================================
    // ORGANIZER PROFILE
    // ============================================

    public Organizer? Organizer { get; set; }


    // ============================================
    // CUSTOMER PROFILE
    // ============================================

    public Customer? Customer { get; set; }


    // ============================================
    // LOGIN OTP
    // ============================================

    public ICollection<LoginOtp> LoginOtps { get; set; }
        = new List<LoginOtp>();


    // ============================================
    // USER NOTIFICATIONS
    // ============================================

    public ICollection<UserNotification> Notifications { get; set; }
        = new List<UserNotification>();
}