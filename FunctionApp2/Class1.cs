using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Max.RecordingStreamer
{
    public static class Httpfunction
    {

        [FunctionName("HttpTriggerCSharp")]
        //public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
        public static HttpResponseMessage Run(
         HttpRequestMessage req, TraceWriter log)
        {
        //    var data = await req.Content.ReadAsStringAsync();
        //    var formValues = data.Split('&')
        //        .Select(value => value.Split('='))
        //        .ToDictionary(pair => Uri.UnescapeDataString(pair[0]).Replace("+", " "),
        //pair => Uri.UnescapeDataString(pair[1]).Replace("+", " "));
            
            log.Info(String.Format("{0} Sup {1}!", req, "gary"));
            Console.WriteLine("hello");
            //return Task.FromResult<object>(true);

            return new HttpResponseMessage
            {
                Content = new StringContent("Hello", Encoding.UTF8, "application/text")
            };
        }
    }
}
