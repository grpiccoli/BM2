using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public interface IPuppet
    {
        Task<Page> ForceGetPageAsync(Uri uri, ICollection<string> block = null);
        Task<Page> GetPageAsync(Uri uri, ICollection<string> block = null);
        Task<Page> GetPageAsync(string WebSocketEndpoint);
        //Task<string> GetCaptchaAsync(Page page, string selector, string folder = null);
    }
}
