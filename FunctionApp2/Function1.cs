using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
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
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = @"HttpTriggersCSharp/name/{sv}/{ss}/{srt}/{sp}/{st}/{se}/{sig}/{sAccName}")]HttpRequestMessage req, string sv,
            string ss, string srt, string sp, string st, string se, string sig, string sAccName,
             TraceWriter log)

        {
            // parse query parameter

            // Get request body
            // Set name to query string or body data

            string sas = (sv + "&" + ss + "&" + srt + "&" + sp + "&" + st + "&" + se + "&" + sig + ";BlobEndpoint=https://" + sAccName + ".blob.core.windows.net/;");
            sas = sas.Replace("++", "%");

            string request = req.Content.Headers.ToString();
            log.Info("f");

            foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                log.Info($"{drive.Name}: {drive.TotalFreeSpace / 1024 / 1024 / 1024} GB");
            }


            log.Info("C# HTTP trigger function processed a request.");

            string containername = "recordings";
            CloudStorageAccount storageAccount = null;

            try
            {
                storageAccount = CloudStorageAccount.Parse(sas);
            }
            catch
            {
                log.Error("Cannot parse sas");
                Environment.Exit(80085);
            }
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containername);
            container.CreateIfNotExists();

            var list = container.ListBlobs(useFlatBlobListing: true);
            var listOfBlobNames = new List<string>();

            MemoryStream outputstream = new MemoryStream();
            ZipOutputStream zipper = new ZipOutputStream(outputstream);
            zipper.SetLevel(1);

            string zipFileName = ("zup " + DateTime.Now.Millisecond + ".zip");

            CloudBlockBlob uploadblob = container.GetBlockBlobReference(zipFileName);

            // this loop constucts a list of blobs that can be downloaded

            foreach (var blob in list)
            {
                var Blobfilename = blob.Uri.LocalPath;
                string correctfilename = Blobfilename.Replace("%20", " ");
                correctfilename = correctfilename.Replace(("/" + containername + "/"), "");

                listOfBlobNames.Add(correctfilename);
            }

            // this loop adds files into entries in a zip file
            foreach (var blobname in listOfBlobNames)
            {
                int fourofour = 0;
                MemoryStream stream = new MemoryStream();

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobname);
                log.Info("Blob Size - " + blockBlob.StreamMinimumReadSizeInBytes);
                try { blockBlob.DownloadToStream(stream); }
                catch
                {
                    fourofour = 1;
                    log.Info("skipping: 404 not found - " + blobname);
                }

                if (fourofour == 0)
                {
                    log.Info("Streaming " + blobname + " to zip " + zipFileName);
                    ZipEntry newEntry = new ZipEntry(blobname);
                    zipper.PutNextEntry(newEntry);
                    stream.Position = 0;
                    stream.CopyTo(zipper);
                    zipper.CloseEntry();
                    stream.Close();
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