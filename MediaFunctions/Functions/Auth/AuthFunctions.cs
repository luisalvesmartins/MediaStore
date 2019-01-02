using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Functions
{
    public static class AuthFunctions
    {
        public static async Task<string> GetAuthProviderParam(string iAuthMeURL,string iXZumoAUth,string iParamKey)
        {
            using (HttpClient pHCtClient = new HttpClient())
            {
                pHCtClient.DefaultRequestHeaders.Add("x-zumo-auth", iXZumoAUth);
                string pStrResponse = await pHCtClient.GetStringAsync(iAuthMeURL);
                JObject pJOtResponse = JObject.Parse(pStrResponse.Trim(new char[] { '[', ']' }));
                if (pJOtResponse[iParamKey] != null)
                {
                    return (pJOtResponse[iParamKey].Value<string>());
                }
                else
                {
                    throw new KeyNotFoundException(string.Format("A parameter with the key '{0}' was not found.", iParamKey));
                }
            }
        }
    }
}