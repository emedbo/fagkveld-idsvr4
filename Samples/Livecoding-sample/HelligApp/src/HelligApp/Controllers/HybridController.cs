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
            var vm = new HybridViewModel{IdToken = idToken,AuthCode = authCode,AccessToken = accesstoken,RefreshToken = refreshtoken,ExpiresIn = expiresin};
            return View("Index", vm);
        }

        //public IActionResult GetAuthCode()
        //{
            
        //}

        //[HttpPost("hybrid/signin-oidc")]
        //public IActionResult AuthCodeReceived_Post()
        //{
           
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetAccessToken(string authCode, string idToken )
        //{
        
        //}

        //[HttpGet]
        //public async Task<IActionResult> GetRefreshToken(string refreshToken, string idToken)
        //{
        
        //}


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