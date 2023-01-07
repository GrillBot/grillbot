using System;

namespace GrillBot.Data.Models.API.OAuth2;

/// <summary>
/// Token from OAuth2 login lifecycle.
/// </summary>
public class OAuth2LoginToken
{
    /// <summary>
    /// Error message if login failed.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Access token if login was success.
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Datetime in UTC when login expires.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    public OAuth2LoginToken() { }

    public OAuth2LoginToken(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public OAuth2LoginToken(string accessToken, DateTimeOffset expiresAt)
    {
        AccessToken = accessToken;
        ExpiresAt = expiresAt;
    }
}
