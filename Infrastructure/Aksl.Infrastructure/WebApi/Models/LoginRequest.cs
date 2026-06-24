using System.ComponentModel.DataAnnotations;

namespace Aksl.Infrastructure;

public class LoginRequest 
{  
    #region Properties
    [Required]
    [StringLength(maximumLength: 16, MinimumLength = 1)]
    public string UserName { get; set; } 

    [Required]
    [StringLength(maximumLength: 16, MinimumLength = 8)]
    public string Password { get; set; }

    ////[EmailAddress]
    //public string Email { get; set; }

    ////[Phone]
    //public string PhoneNumber { get; set; }
    #endregion
}

public class LoginResponse : ApiResult
{
    #region Properties
    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
    #endregion
}

public class ResetLockoutRequest
{
    [Required]
    [StringLength(maximumLength: 16, MinimumLength = 1)]
    public string UserName { get; set; }
}

public class ResetLockoutResponse : ApiResult
{
}
