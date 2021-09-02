using BiblioMit.Models.Entities;
using BiblioMit.Models.Entities.Ads;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public class FlowService : IFlow
    {
        private readonly FlowSettings _settings;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelper _urlHelper;
        public FlowService(IOptions<FlowSettings> settings,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelper urlHelper,
            IWebHostEnvironment environment)
        {
            _urlHelper = urlHelper;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _settings = settings?.Value;
        }
        public string FlowJs(string function, SortedDictionary<string, string> form)
        {
            var args = string.Join("&",
                form.Select(kvp => string.Format(CultureInfo.InvariantCulture, "{0}={1}", kvp.Key, kvp.Value)));
            var script = Path.Combine(_environment.ContentRootPath, "scripts", "flow.js");
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "C:\\Program Files\\nodejs\\node.exe",
                    Arguments = $"{script} {function} {_settings.SecretKey} {args}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            var s = string.Empty;
            var e = string.Empty;
            process.OutputDataReceived += (sender, data) => s += data.Data;
            process.ErrorDataReceived += (sender, data) => e += data.Data;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.Close();
            return s;
        }
        public SortedDictionary<string, string> Sign(SortedDictionary<string, string> ccForm)
        {
            if(ccForm != null)
            {
                var s = FlowJs("sign", ccForm);
                ccForm.Add("s", s);
            }
            return ccForm;
        }
        public Customer CustomerCreate(int id, string name, string email)
        {
            var ccForm = new SortedDictionary<string, string>
            {
                { "apiKey", _settings.ApiKey },
                { "name", name },
                { "email", email },
                { "externalId", id.ToString(CultureInfo.InvariantCulture) }
            };
            var text = FlowJs("customer/create", ccForm);
            var json = JsonConvert.DeserializeObject<Customer>(text);
            return json;
        }
        public Register CustomerRegister(string id, Uri returnUrl)
        {
            var ccForm = new SortedDictionary<string, string>
            {
                { "apiKey", _settings.ApiKey },
                { "customerId", id },
                { "url_return", returnUrl?.AbsolutePath }
            };
            var text = FlowJs("customer/register", ccForm);
            var json = JsonConvert.DeserializeObject<Register>(text);
            return json;
        }
        public Customer GetRegisterStatus(string token)
        {
            var ccForm = new SortedDictionary<string, string>
            {
                { "apiKey", _settings.ApiKey },
                { "token", token }
            };
            var text = FlowJs("customer/getRegisterStatus", ccForm);
            var json = JsonConvert.DeserializeObject<Customer>(text);
            return json;
        }
        public Customer CustomerCharge(int id, int amount, string subject, string order)
        {
            var ccForm = new SortedDictionary<string, string>
            {
                { "apiKey", _settings.ApiKey },
                { "customerId", id.ToString(CultureInfo.InvariantCulture) },
                { "amount", amount.ToString(CultureInfo.InvariantCulture) },
                { "subject", subject },
                { "commerceOrder", order },
                { "currency", "UF" }
            };
            var text = FlowJs("customer/charge", ccForm);
            var json = JsonConvert.DeserializeObject<Customer>(text);
            return json;
        }
        public string PaymentCreate(int id, string description, int ammount, string email)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var confirmUri = _urlHelper.Action("Index", "Payment", null, scheme);
            var returnUri = _urlHelper.Action("Index", "Payment", null, scheme); 
            var ccForm = new SortedDictionary<string, string>
            {
                { "apiKey", _settings.ApiKey },
                { "commerceOrder", id.ToString(CultureInfo.InvariantCulture) },
                { "subject", description },
                { "currency", "CLP" },
                { "amount", ammount.ToString(CultureInfo.InvariantCulture) },
                { "email", email },
                { "urlConfirmation", confirmUri.ToString() },
                { "urlReturn", returnUri.ToString() }
            };
            var text = FlowJs("payment/create", ccForm);
            var json = JsonConvert.DeserializeObject<Register>(text);
            return json.Url + "?token=" + json.Token;
        }
    }
    public class Register
    {
        public Uri Url { get; set; }
        public string Token { get; set; }
        public int FlowOrder { get; set; }
    }
    public class FlowSettings
    {
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string Currency { get; set; }
        public Uri EndPoint { get; set; }
    }
}
