using System;
using System.Net;

namespace Jellyfin.Plugin.SerializdSync.Api.Models;

/// <summary>
/// Exception thrown when Serializd API calls fail.
/// </summary>
public class SerializdApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializdApiException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public SerializdApiException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializdApiException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public SerializdApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializdApiException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    public SerializdApiException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the HTTP status code if available.
    /// </summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets a value indicating whether this is an authentication error.
    /// </summary>
    public bool IsAuthenticationError =>
        StatusCode == HttpStatusCode.Unauthorized ||
        StatusCode == HttpStatusCode.Forbidden;
}
