using System.ComponentModel.DataAnnotations;

namespace Aksl.Infrastructure;

public record RefreshTokenRequest 
{
    [Required]
    public string AccessToken { get; set; }

    [Required]
    public string RefreshToken { get; set; }
}

public class RefreshTokenResponse : ApiResult
{
    public bool Succeeded { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
