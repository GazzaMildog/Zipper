using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace FunctionApp2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "HttpTriggerCSharp/name/{name}/{sas}/{blobname}")]HttpRequestMessage req, string name,
            string sas, TraceWriter log)
        {
            sas = "";
            log.Info("C# HTTP trigger function processed a request.");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(sas);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("testtest");
            container.CreateIfNotExists();

            var list = container.ListBlobs();
            var listOfBlobNames = new List<string>();

            MemoryStream outputstream = new MemoryStream();
            ZipOutputStream zipper = new ZipOutputStream(outputstream);
            zipper.SetLevel(3);
            CloudAppendBlob uploadblob = container.GetAppendBlobReference("zup.zip");
            foreach (var blob in list)
            {
                var Blobfilename = blob.Uri.Segments.Last();
                listOfBlobNames.Add(Blobfilename);
            }
            foreach (var blobname in listOfBlobNames)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobname);
                    try { blockBlob.DownloadToStream(stream); }
                    catch { }

                    ZipEntry newEntry = new ZipEntry(blobname);
                    zipper.PutNextEntry(newEntry);
                    stream.Position = 0;
                    StreamUtils.Copy(stream, zipper, new byte[4096]);
                    zipper.CloseEntry();
                    stream.Close();
                    outputstream.Position = 0;
                    uploadblob.UploadFromStream(outputstream);
                }
            }

            zipper.IsStreamOwner = false;
            zipper.Close();
            outputstream.Position = 0;

            uploadblob.UploadFromStream(outputstream);

            outputstream.Close();

            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK, "Hello ");
        }
    }
}