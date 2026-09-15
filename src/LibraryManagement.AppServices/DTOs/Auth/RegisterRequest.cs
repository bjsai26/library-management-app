using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.AppServices.DTOs.Auth;

/// <summary>Sign-up payload. Registration always creates an account with the Member role.</summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Full name must be between 3 and 150 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).+$",
        ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, a digit and a special character.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required.")]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
