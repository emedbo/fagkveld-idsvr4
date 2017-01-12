using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HelligApp.Controllers
{
    public class HybridController : Controller
    {
        private string IdSvr = "http://localhost:5000/";
        private string UrlEncodedRedirectUri = "http%3A%2F%2Flocalhost%3A55458%2Fhybrid%2Fsignin-oidc";
        private string RedirectUri = "http://localhost:55458/hybrid/signin-oidc";
        private const string clientId = "externalmvc";
        private const string clientSecret = "secret";

        

        public IActionResult Index(string authCode, string idToken, string accesstoken, string expiresin, string refreshtoken)
        {
            var vm = new HybridViewModel
            {
                IdToken = idToken,
                AuthCode = authCode,
                AccessToken = accesstoken,
                RefreshToken = refreshtoken,
                ExpiresIn = expiresin
            };
            return View("Index", vm);
        }

        public IActionResult GetAuthCode()
        {
            var path =
                CreateUrl(
                    "connect/authorize" +
                    "?response_type=code+id_token" +
                    "&response_mode=form_post" +
                    "&client_id=externalmvc" +
                    "&client_secret=secret" +
                    "&scope=offline_access+openid" +
                    "&nonce=123" +
                    "&redirect_uri=" +
                    UrlEncodedRedirectUri);
            return Redirect(path);
        }

        [HttpPost("hybrid/signin-oidc")]
        public IActionResult AuthCodeReceived_Post()
        {
            var vm = new HybridViewModel
            {
                IdToken = Request.Form["id_token"],
                AuthCode = Request.Form["code"],
            };
            return RedirectToAction("Index", CreateViewModelForUriRedirect(vm));
        }

        private object CreateViewModelForUriRedirect(HybridViewModel vm)
        {
            return new
            {
                authCode = vm.AuthCode,
                idToken = vm.IdToken,
                accesstoken = vm.AccessToken,
                expiresin = vm.ExpiresIn,
                refreshtoken = vm.RefreshToken
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAccessToken(string authCode, string idToken )
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(IdSvr, UriKind.Absolute);
            var result = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", RedirectUri),
                new KeyValuePair<string, string>("code", authCode)
            }));
            var content = await result.Content.ReadAsStringAsync();
            var vm = ParseToAuthCodeViewModel(content, idToken);
            return RedirectToAction("Index", CreateViewModelForUriRedirect(vm));
        }

        [HttpGet]
        public async Task<IActionResult> GetRefreshToken(string refreshToken, string idToken)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(IdSvr, UriKind.Absolute);
            var postAsync = await client.PostAsync("/connect/token", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            }));

            var content = await postAsync.Content.ReadAsStringAsync();
            var vm = ParseToAuthCodeViewModel(content, idToken);
            return RedirectToAction("Index", CreateViewModelForUriRedirect(vm));
        }


        private HybridViewModel ParseToAuthCodeViewModel(string content, string idToken)
        {
            var obj = JsonConvert.DeserializeObject(content) as JObject;
            if (obj == null)
            {
                return null;
            }
            var vm = new HybridViewModel
            {
                AccessToken = obj["access_token"].Value<string>(),
                ExpiresIn = obj["expires_in"].Value<string>(),
                RefreshToken = obj["refresh_token"].Value<string>(),
                IdToken = obj["id_token"]?.Value<string>()
            };
            if (vm.IdToken == null)
            {
                vm.IdToken = idToken;
            }
            return vm;
        }

        private string CreateUrl(string path)
        {
            return IdSvr + path;
        }
    }

    public class HybridViewModel
    {
        public string AuthCode { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
        public string ExpiresIn { get; set; }
    }
}