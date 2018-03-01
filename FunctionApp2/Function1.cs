using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace FunctionApp2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = @"HttpTriggersCSharp/name/{sv}/{ss}/{srt}/{sp}/{st}/{se}/{sig}/{sAccName}/{containername}")]HttpRequestMessage req, string sv,
            string ss, string srt, string sp, string st, string se, string sig, string sAccName, string containername,
             TraceWriter log)

        {
            log.Info("Setting up variables...");

            // FORMATTING OF SAS - SharedAccessSignature=sv=2017-04-17/ss=*stuff*/srt=*stuff*/sp=*stuff*/st=*stuff*/se=*stuff*/sig=*stuff*/*put the name of the storage account here*
            // this will be added to the main url in the format shown above
            string sas = (sv + "&" + ss + "&" + srt + "&" + sp + "&" + st + "&" + se + "&" + sig + ";BlobEndpoint=https://" + sAccName + ".blob.core.windows.net/;");
            sas = sas.Replace("++", "%");

            //variables and initial set up

            string request = req.Content.Headers.ToString();
            foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                log.Info($"{drive.Name}: {drive.TotalFreeSpace / 1024 / 1024 / 1024} GB");
            }
            log.Info("C# HTTP trigger function processed a request.");

            string zipFileName = ("blobzipper last part.zip");
            //string containername = "recordings";
            CloudStorageAccount storageAccount = null;
            //int zipTransitionCount = 0;
            //int amountOfParts = 0;

            System.IO.Directory.CreateDirectory("D:\\local\\Zipper");
            System.IO.Directory.CreateDirectory("D:\\local\\Zip");
            log.Info("succeeded in making dirs");
            //MemoryStream outputstream = new MemoryStream();
            //ZipOutputStream zipper = new ZipOutputStream(outputstream);
            //zipper.SetLevel(1);

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

            // list variables

            var list = container.ListBlobs(useFlatBlobListing: true);
            var listOfBlobNames = new List<string>();

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
                using (MemoryStream stream = new MemoryStream())
                {
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
                        string blobname2 = blobname;
                        blobname2 = blobname2.Replace(@":", "");
                        blobname2 = blobname2.Replace("/", "\\");
                        string fullblobpath = ("D:\\local\\Zipper\\" + blobname2);
                        string input = fullblobpath;
                        int index = input.LastIndexOf("\\");
                        if (index > 0)
                            input = input.Substring(0, index);
                        log.Info(input);
                        System.IO.Directory.CreateDirectory(input);
                        log.Info(fullblobpath);
                        var BlobFile = File.Create(fullblobpath);
                        stream.Position = 0;
                        stream.CopyTo(BlobFile);
                        stream.Dispose();
                        stream.Close();
                        GC.Collect();
                        BlobFile.Close();

                        foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                        {
                            log.Info($"{drive.Name}: {drive.TotalFreeSpace / 1024 / 1024 / 1024} GB");

                            //if(drive.Name.Contains("D"))
                            //{
                            //    if (drive.TotalFreeSpace <= 4294967296)
                            //    {
                            //        System.Threading.Thread.Sleep(2000);
                            //        log.Info("zipping part");
                            //        string tempzipPath = (@"D:\local\Zip\Recordings from " + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".zip");
                            //        zipFileName = ("Recordings from " + DateTime.Now.ToString("yyyy - dd - M--HH - mm - ss") + ".zip");
                            //        System.IO.Compression.ZipFile.CreateFromDirectory(@"D:\local\Zipper", tempzipPath, CompressionLevel.Optimal, true);
                            //        container = blobClient.GetContainerReference("uploads");
                            //        container.CreateIfNotExists();
                            //        CloudBlockBlob tempblob = container.GetBlockBlobReference(zipFileName);
                            //        tempblob.UploadFromFile(tempzipPath);
                            //        System.IO.File.Delete(tempzipPath);
                            //        log.Info("Part zip complete");

                            //    }
                            //}
                        }

                        //++zipTransitionCount;
                        //log.Info("Streaming " + blobname + " to zip " + zipFileName);
                        //ZipEntry newEntry = new ZipEntry(blobname);
                        //zipper.PutNextEntry(newEntry);
                        //stream.Position = 0;
                        //stream.CopyTo(zipper);
                        //zipper.CloseEntry();
                        //stream.Close();

                        // this if statement clears memory by writing the currect amount files to a zip.
                        //if (zipTransitionCount == 100)
                        //{
                        //    log.Info("Writing zip file to temp drive");
                        //    ++amountOfParts;
                        //    zipper.IsStreamOwner = false;
                        //    zipper.Close();
                        //    var FileSaver = File.Create("C:\\Zipper\\BlobZip Part " + amountOfParts + ".zip");
                        //    outputstream.Position = 0;
                        //    outputstream.CopyTo(FileSaver);
                        //    outputstream.Dispose();
                        //    outputstream.Close();

                        //    outputstream = new MemoryStream();
                        //    zipper = new ZipOutputStream(outputstream);
                        //    zipTransitionCount = 0;

                        //}
                    }
                }
            }
            System.Threading.Thread.Sleep(3000);
            //zipper.IsStreamOwner = false;
            //zipper.Close();
            //outputstream.Position = 0;
            GC.Collect();
            log.Info("Beginning zipping process");

            string zipPath = (@"D:\local\Zip\Recordings from " + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".zip");
            zipFileName = ("Recordings from " + DateTime.Now.ToString("yyyy - dd - M--HH - mm - ss") + ".zip");
            System.IO.Compression.ZipFile.CreateFromDirectory(@"D:\local\Zipper", zipPath, CompressionLevel.Optimal, true);
            log.Info("zipping complete");

            container = blobClient.GetContainerReference("uploads");
            container.CreateIfNotExists();

            CloudBlockBlob uploadblob = container.GetBlockBlobReference(zipFileName);

            uploadblob.UploadFromFile(zipPath);

            System.IO.Directory.Delete(@"D:\local\Zipper", true);
            System.IO.Directory.Delete(@"D:\local\Zip", true);

            uploadblob.Uri.AbsoluteUri.ToString();
            //outputstream.Close();

            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK, "Task has completed successfully. URI to uploaded blob: " + uploadblob.Uri.AbsoluteUri.ToString());
        }
    }
}