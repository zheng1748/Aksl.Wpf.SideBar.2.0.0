using System.ComponentModel.DataAnnotations;

namespace Aksl.Infrastructure.Models;

public class LoginRequest
{
    [Required]
    [StringLength(16)]
    public string UserName { get; set; } 

    [Required]
    [StringLength(16, MinimumLength = 8)]
    public string Password { get; set; }

    public string Email { get; set; }
}

public class LoginResponse
{
    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
}
