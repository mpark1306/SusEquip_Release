using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using SusEquip.Data.Interfaces.Services;
using System.Threading.Tasks;

namespace SusEquip.Data.Services
{
    public class CookieService : ICookieService
    {
        private readonly IJSRuntime _jsRuntime;

        public CookieService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetCookieAsync(string key, string value, int? expireTime)
        {
            await _jsRuntime.InvokeVoidAsync("cookieHelper.setCookie", key, value, expireTime);
        }

        public async Task<string> GetCookieAsync(string key)
        {
            return await _jsRuntime.InvokeAsync<string>("cookieHelper.getCookie", key);
        }
    }
}
