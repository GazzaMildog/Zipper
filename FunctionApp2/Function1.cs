using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using System;
using System.Collections.Generic;

namespace FunctionApp2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "HttpTriggerCSharp/name/{name}/{sas}/{blobname}")]HttpRequestMessage req, string name,
            string sas, TraceWriter log
            )
        {
            sas = "SharedAccessSignature=sv=2017-04-17&ss=bfqt&srt=sco&sp=rwdl&st=2018-02-22T15%3A52%3A00Z&se=2019-02-25T15%3A52%3A00Z&sig=G4eBQmUgE7TfgQclAEpa82DYFjaFZArF%2BuraClQIJC8%3D;BlobEndpoint=https://maxstoretest.blob.core.windows.net/;FileEndpoint=https://maxstoretest.file.core.windows.net/;QueueEndpoint=https://maxstoretest.queue.core.windows.net/;TableEndpoint=https://maxstoretest.table.core.windows.net/;";
            log.Info("C# HTTP trigger function processed a request.");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(sas);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("testtest");
            container.CreateIfNotExists();

            var list = container.ListBlobs();
            var listOfBlobNames = new List<string>();

            CloudBlockBlob uploadblob = container.GetBlockBlobReference("zup.zip");
            MemoryStream outputstream = new MemoryStream();
            ZipOutputStream zipper = new ZipOutputStream(outputstream);
            zipper.SetLevel(3);
            int fourOfour = 0;
            foreach (var blob in list)
            {
                var Blobfilename = blob.Uri.Segments.Last();
                listOfBlobNames.Add(Blobfilename);

            }
            foreach(var blobname in listOfBlobNames)
            {
                fourOfour = 0;
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobname);

                using (MemoryStream stream = new MemoryStream()) {

                    try { blockBlob.DownloadToStream(zipper); }
                    catch
                    {
                        fourOfour = 1;
                    }
                
                    if(fourOfour == 0)
                    {
                        ZipEntry newEntry = new ZipEntry(blobname);
                        zipper.PutNextEntry(newEntry);
                        stream.Position = 0;
                        StreamUtils.Copy(stream, zipper, new byte[512]);
                        zipper.CloseEntry();
                        stream.Close();
                        //  outputstream.Position = 0;
                        // uploadblob.UploadFromStream(zipper);
                    }

                    
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
