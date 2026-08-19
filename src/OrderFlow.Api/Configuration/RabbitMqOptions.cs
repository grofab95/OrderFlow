using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Api.Configuration;

public class RabbitMqOptions
{
    public const string SectionKey = "RabbitMq";
    
    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; }

    [Required]
    public string Username { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}