using System.Net.Http;
using System.Text;
using System.Text.Json;
using JdCookieWpf.Models;

namespace JdCookieWpf.Services;

public class QingLongService
{
    private readonly HttpClient _httpClient;
    private readonly AppConfig _config;

    public QingLongService(AppConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(config.Url);
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    private async Task SetToken()
    {
        var token = await GetToken();
        if (token == null)
        {
            throw new Exception("获取token失败");
        }

        // 判断是否设置过Authorization请求头，且以Bearer 开头
        if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            var authorization = _httpClient.DefaultRequestHeaders.Authorization?.ToString();
            if (authorization != null && authorization.StartsWith("Bearer "))
            {
                return;
            }
        }

        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
    }

    private async Task<string?> GetToken()
    {
        var tokenInfo = TokenService.Get<Dictionary<string, string>>("token");
        if (tokenInfo != null && tokenInfo.TryGetValue("token", out var token) &&
            tokenInfo.TryGetValue("expire", out var value))
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var result = long.TryParse(value, out var expire);
            if (result && expire > now)
            {
                return token;
            }
        }

        // 发送http请求获取token
        var queryParms = new Dictionary<string, string>()
        {
            { "client_id", _config.Cid },
            { "client_secret", _config.Secret }
        };
        var queryString = string.Join("&", queryParms.Select(param => $"{param.Key}={param.Value}"));
        var response = await _httpClient.GetAsync($"/open/auth/token?{queryString}");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();

        try
        {
            var apiResp = JsonSerializer.Deserialize<ApiResponse<TokenInfo>>(responseContent);
            if (apiResp?.Code != 200) return null;
            if (apiResp.Data is not { } data) return null;

            var newTokenInfo = new Dictionary<string, string>
            {
                { "token", data.Token },
                { "expire", data.Expire.ToString() }
            };
            TokenService.Set("token", newTokenInfo, TimeSpan.FromHours(1));
            return data.Token;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<List<EnvInfo>> GetEnvs()
    {
        await SetToken();
        // 获取当前时间戳
        var queryParams = new Dictionary<string, string>()
        {
            { "searchValue", "JD_COOKIE" },
            { "t", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }
        };
        var queryString = string.Join("&", queryParams.Select(param => $"{param.Key}={param.Value}"));
        var response = await _httpClient.GetAsync($"/open/envs?{queryString}");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();

        var apiResp = JsonSerializer.Deserialize<ApiResponse<List<EnvInfo>>>(responseContent);
        if (apiResp?.Code != 200) return null!;
        return apiResp.Data ?? null!;
    }

    private async Task<bool> AddEnv(EnvInfo envInfo)
    {
        await SetToken();
        var data = JsonSerializer.Serialize(envInfo);
        var response = await _httpClient.PostAsync("/open/envs", new StringContent(data));
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();

        var apiResp = JsonSerializer.Deserialize<ApiResponse<EnvInfo>>(responseContent);
        return apiResp?.Code == 200;
    }

    private async Task<bool> UpdateEnv(EnvInfo envInfo)
    {
        await SetToken();
        var data = JsonSerializer.Serialize(envInfo);
        var response =
            await _httpClient.PutAsync("/open/envs", new StringContent(data, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResp = JsonSerializer.Deserialize<ApiResponse<EnvInfo>>(responseContent);
        return apiResp?.Code == 200;
    }

    public async Task<bool> PushCookie(Dictionary<string, string>? cookies)
    {
        if (cookies == null)
        {
            return false;
        }

        if (!cookies.TryGetValue("pt_key", out _) || !cookies.TryGetValue("pt_pin", out var ptPin))
        {
            return false;
        }

        var envInfos = await GetEnvs();
        var envId = GetEnvId(envInfos, ptPin);

        var value = string.Join(";", cookies.Select(x => $"{x.Key}={x.Value}"));
        value += ";";
        bool result;
        if (envId == 0)
        {
            // 新增
            var envInfo = new EnvInfo
            {
                Name = "JD_COOKIE",
                Value = value,
                Remarks = ptPin,
            };
            result = await AddEnv(envInfo);
        }
        else
        {
            var envInfo = new EnvInfo
            {
                Id = envId,
                Name = "JD_COOKIE",
                Value = value,
                Remarks = ptPin,
            };
            result = await UpdateEnv(envInfo);
        }

        return result;
    }

    private static int GetEnvId(List<EnvInfo> envInfos, string pin)
    {
        var env = envInfos.FirstOrDefault(e => e.Value.Contains(pin));
        return env?.Id ?? 0;
    }
}