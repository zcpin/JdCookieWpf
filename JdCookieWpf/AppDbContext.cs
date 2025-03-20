using System.IO;
using JdCookieWpf.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JdCookieWpf;

public class AppDbContext : DbContext
{
    public DbSet<AccountItem> Accounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        const string dir = nameof(JdCookieWpf);
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            dir, $"{dir}.db");

        if (!File.Exists(dbPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? string.Empty);
        }

        optionsBuilder.UseSqlite($"Data Source={dbPath}")
            .LogTo(message => System.Diagnostics.Debug.WriteLine(message),
                LogLevel.Information);
    }
}