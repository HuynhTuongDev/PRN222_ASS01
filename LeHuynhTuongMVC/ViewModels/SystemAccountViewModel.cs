using System.ComponentModel.DataAnnotations;

namespace LeHuynhTuongMVC.ViewModels;

public class SystemAccountViewModel
{
    public short AccountId { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string AccountName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(70, ErrorMessage = "Email cannot exceed 70 characters")]
    public string AccountEmail { get; set; } = null!;

    [StringLength(70, ErrorMessage = "Password cannot exceed 70 characters")]
    [DataType(DataType.Password)]
    public string? AccountPassword { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public int AccountRole { get; set; }

    // For display purposes
    public string RoleName => AccountRole switch
    {
        1 => "Staff",
        2 => "Lecturer",
        3 => "Admin",
        _ => "Unknown"
    };
}

public class ProfileUpdateViewModel
{
    public short AccountId { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string AccountName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(70, ErrorMessage = "Email cannot exceed 70 characters")]
    public string AccountEmail { get; set; } = null!;

    [StringLength(70, ErrorMessage = "Password cannot exceed 70 characters")]
    [DataType(DataType.Password)]
    public string? AccountPassword { get; set; }

    public int AccountRole { get; set; }

    // For display purposes
    public string RoleName => AccountRole switch
    {
        1 => "Staff",
        2 => "Lecturer",
        3 => "Admin",
        _ => "Unknown"
    };
} 