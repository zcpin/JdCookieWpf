using JdCookieWpf.Models;
using Microsoft.Web.WebView2.Wpf;

namespace JdCookieWpf.Services;

public static class CookieService
{
    public static async Task<Dictionary<string, string>?> GetCookie(WebView2? webView2)
    {
        if (webView2?.CoreWebView2 == null || webView2.Source == null)
        {
            throw new ArgumentNullException(nameof(webView2), "WebView2 instance or its properties are null.");
        }

        var cookieManager = webView2.CoreWebView2.CookieManager;
        var cookies = await cookieManager.GetCookiesAsync(webView2.Source.ToString());

        if (cookies == null)
        {
            throw new InvalidOperationException("Cookies collection is null.");
        }

        string? ptKey = null, ptPin = null;

        foreach (var cookie in cookies)
        {
            switch (cookie.Name)
            {
                case "pt_key":
                    ptKey = cookie.Value;
                    break;
                case "pt_pin":
                    ptPin = cookie.Value;
                    break;
            }
        }

        if (ptKey == null || ptPin == null)
        {
            return null;
        }

        return new Dictionary<string, string>
        {
            { "pt_key", ptKey },
            { "pt_pin", ptPin }
        };
    }

}