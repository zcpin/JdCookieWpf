using System.ComponentModel.DataAnnotations;

namespace JdCookieWpf.Models;

public class AccountItem
{
    [Key] public required int Id { get; set; }
    [MaxLength(32)] public required string Account { get; set; }
    [Required, MaxLength(100)] public required string Password { get; set; }
    [MaxLength(100)] public string? Remark { get; set; } = "";
    public DateTime CreateTime { get; set; } = DateTime.Now;
    public DateTime? UpdateTime { get; set; }
}