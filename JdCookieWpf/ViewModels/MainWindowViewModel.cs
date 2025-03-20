using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JdCookieWpf.Models;
using JdCookieWpf.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.WebView2.Wpf;

namespace JdCookieWpf.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private AppConfig _config = ConfigService.LoadConfig() ?? throw new InvalidOperationException();

    [ObservableProperty] private ObservableCollection<AccountItem> _accounts;

    [ObservableProperty] private AccountItem _accountItem;

    [ObservableProperty] private static bool _isEdit;


    public MainWindowViewModel()
    {
        _accountItem = new AccountItem
        {
            Account = "",
            Password = "",
            Remark = "",
            Id = 0
        };
        using var dbContext = new AppDbContext();
        Accounts = [];
        _ = GetAccounts();
    }


    // 保存配置
    [RelayCommand]
    private void SaveConfig()
    {
        _ = ConfigService.SaveConfig(Config);
        MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task GetAccounts()
    {
        await using var dbContext = new AppDbContext();
        await dbContext.Database.EnsureCreatedAsync();
        var accounts = await dbContext.Accounts.ToListAsync();

        Accounts.Clear();
        foreach (var account in accounts)
        {
            Accounts.Add(account);
        }
    }

    // 保存账号
    [RelayCommand]
    private async Task SaveAccount()
    {
        await using var dbContext = new AppDbContext();
        try
        {
            // 校验数据
            if (string.IsNullOrWhiteSpace(AccountItem.Account) || string.IsNullOrWhiteSpace(AccountItem.Password))
            {
                MessageBox.Show("账号或密码不能为空", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AccountItem.Account = AccountItem.Account.Trim();
            AccountItem.Password = AccountItem.Password.Trim();
            // 查询账号是否存在
            var accountItem = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Account == AccountItem.Account);

            if (IsEdit)
            {
                if (accountItem == null)
                {
                    MessageBox.Show("账号不存在", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                accountItem.Remark = AccountItem.Remark;
                accountItem.Password = AccountItem.Password;
                accountItem.UpdateTime = DateTime.Now;
                IsEdit = false;
            }
            else
            {
                if (accountItem != null)
                {
                    MessageBox.Show("账号已存在", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                AccountItem.CreateTime = DateTime.Now;
                await dbContext.Accounts.AddAsync(AccountItem);
            }

            await dbContext.SaveChangesAsync();
            await GetAccounts();
            // 清空账号
            ClearAccountFields();
            MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception e)
        {
            LogError(e, nameof(SaveAccount));
            MessageBox.Show("保存失败：" + e.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // 编辑账号
    [RelayCommand]
    private async Task EditAccount(string account)
    {
        IsEdit = true;
        await using var dbContext = new AppDbContext();
        var accountItem = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Account == account);
        if (accountItem == null)
        {
            MessageBox.Show("账号不存在", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // 表单赋值
        AccountItem = new AccountItem
        {
            Account = accountItem.Account,
            Password = accountItem.Password,
            Remark = accountItem.Remark,
            Id = accountItem.Id
        };
    }

    // 删除账号
    [RelayCommand]
    private async Task DeleteAccount(int id)
    {
        await using var dbContext = new AppDbContext();
        var accountItem = await dbContext.Accounts.FindAsync(id);
        if (accountItem != null)
        {
            dbContext.Accounts.Remove(accountItem);
            await dbContext.SaveChangesAsync();
            await GetAccounts();
            MessageBox.Show("删除成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("账号不存在", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        _isEdit = false;
        await GetAccounts();
        ClearAccountFields();
    }


    // 推送cookie
    [RelayCommand]
    private async Task PushCookie(WebView2 webView2)
    {
        try
        {
            var cookies = await CookieService.GetCookie(webView2);
            if (cookies == null)
            {
                MessageBox.Show("获取cookie失败", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var qingLongService = new QingLongService(Config);
            var result = await qingLongService.PushCookie(cookies);
            if (result)
            {
                MessageBox.Show("推送成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("推送失败", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception e)
        {
            MessageBox.Show("推送失败：" + e.Message + e.Source, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // 切换账号
    [RelayCommand]
    private void SwitchAccount(WebView2 webView2)
    {
        webView2.CoreWebView2.CookieManager.DeleteAllCookies();
        webView2.CoreWebView2.Reload();
    }

    // 清空账号字段方法
    private void ClearAccountFields()
    {
        AccountItem = new AccountItem
        {
            Account = "",
            Password = "",
            Remark = "",
            Id = 0
        };
    }

    // 日志记录方法
    private static void LogError(Exception ex, string methodName)
    {
        // 可以集成日志框架（如NLog、Serilog等）
        Console.WriteLine($"Error in {methodName}: {ex.Message}");
    }
}