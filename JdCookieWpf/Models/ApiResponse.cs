using System.Text.Json.Serialization;

namespace JdCookieWpf.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("data")] public T? Data { get; set; }
    [JsonPropertyName("msg")] public string? Msg { get; set; }
}

public class TokenInfo
{
    [JsonPropertyName("token")] public required string Token { get; set; }
    [JsonPropertyName("expiration")] public required long Expire { get; set; }
    [JsonPropertyName("token_type")] public required string TokenType { get; set; }
}

public class EnvInfo
{
    [JsonPropertyName("id")] public int? Id { get; set; }
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("value")] public required string Value { get; set; }
    [JsonPropertyName("remarks")] public string? Remarks { get; set; }
}