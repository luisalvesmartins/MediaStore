using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using ImageResizer;
using MetadataExtractor;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace PhotoUploader
{
    class Program
    {
        static StreamWriter logFile = null;
        static string notAllowedExtensions="";
        static void Main(string[] args)
        {
            notAllowedExtensions = CloudConfigurationManager.GetSetting("notallowedextensions").ToString();
            if (args.Length<1)
            {
                Console.WriteLine("Usage: PhotoUploader <path>");
                return;
            }
            Console.WriteLine("Upload tree starting at");
            Console.WriteLine(args[0]);
            string path = args[0];

            logFile = new StreamWriter(@"log.txt", true);

            string connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString").ToString();
            string blobContainerName = CloudConfigurationManager.GetSetting("Container.Media").ToString(); //"media";
            string blobContainerName1000 = CloudConfigurationManager.GetSetting("Container.Thumbnails1000").ToString(); //"thumbs1000";
            string blobContainerName320 = CloudConfigurationManager.GetSetting("Container.Thumbnails320").ToString(); //"thumbs320";
            string queueThumbnails = CloudConfigurationManager.GetSetting("Container.Thumbnails").ToString(); //"thumbnails";
            string blobContainerNameEXIF = CloudConfigurationManager.GetSetting("Container.EXIF").ToString(); //"EXIF";
            string tableNameMedia = CloudConfigurationManager.GetSetting("Table.Media").ToString(); //"media";
            string tableTag = CloudConfigurationManager.GetSetting("Table.Tag").ToString(); //"Tag";
            string tableTagReverse = CloudConfigurationManager.GetSetting("Table.TagReverse").ToString(); //"TagIndex";

            //public const string tableContainerName = "Tag";
            //public const string tableContainerNameReverse = "TagIndex";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer1 = blobClient.GetContainerReference(blobContainerName320);
            blobContainer1.CreateIfNotExists();
            CloudBlobContainer blobContainerEXIF = blobClient.GetContainerReference(blobContainerNameEXIF);
            blobContainerEXIF.CreateIfNotExists();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);
            blobContainer.CreateIfNotExists();
            CloudBlobContainer blobContainer1000 = blobClient.GetContainerReference(blobContainerName1000);
            blobContainer1000.CreateIfNotExists();

            var queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(queueThumbnails);

            CloudTable tagContainer = Tags.GetTableContainer(storageAccount,tableTag);
            CloudTable tagContainerReverse = Tags.GetTableContainer(storageAccount,tableTagReverse);
            CloudTable tableContainer = StorageFileInfo.GetTableContainer(storageAccount, tableNameMedia);

            DirectoryCopy(blobContainer, blobContainer1000, tableContainer, path, "", tagContainer, tagContainerReverse, queue, blobContainerEXIF);

            logFile.Close();
        }

        private static string CreateEXIF(CloudTable tableContainerSFI, string id, string file, string extension, CloudBlobContainer blobContainerEXIF)
        {
            string ok = "#Image Height#Image Width#Date/Time#Date/Time Original#";

            string sE = "";
            if (".MPG.MOV.MP4".IndexOf(extension.ToUpper() + ".")>=0)
            {
                sE = "{}";
            }
            else
            {
                //Console.WriteLine(n);

                CloudBlockBlob blob = blobContainerEXIF.GetBlockBlobReference(id);
                try
                {
                    IEnumerable<MetadataExtractor.Directory> dirs = ImageMetadataReader.ReadMetadata(file);

                    foreach (var directory in dirs)
                        foreach (var tag in directory.Tags)
                        {
                            if (ok.IndexOf("#" + tag.Name + "#") >= 0)
                            {
                                //Console.WriteLine("YES:" + tag.Name);
                                if (sE != "")
                                    sE += ",";
                                sE += "\"" + tag.Name + "\":\"" + tag.Description + "\"";
                            }
                            //Console.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
                        }
                    sE = "{" + sE + "}";

                    string s = Newtonsoft.Json.JsonConvert.SerializeObject(dirs);

                    blob.UploadText(s);
                }
                catch (Exception e)
                {
                    sE = "{Error:\"" + e.Message + "\"}";
                    blob.UploadText(sE);
                }

            }
            return sE;
        }

        public static void DirectoryCopy(CloudBlobContainer blobContainer, CloudBlobContainer blobContainer1000, CloudTable tableContainer, string sourceDirName, string destDirName, CloudTable tagContainer, CloudTable tagContainerReverse,CloudQueue queue, CloudBlobContainer blobContainerEXIF)
        {
            // Get the subdirectories for the specified directory.
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            System.IO.DirectoryInfo[] dirs = dir.GetDirectories();

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                //string temppath = Path.Combine(destDirName, file.Name);
                //file.CopyTo(temppath, false);
                StorageFileInfo sfi = new StorageFileInfo() {
                    CreationTime = file.CreationTime,
                    Directory = file.Directory.ToString().ToUpper(),
                    Extension = file.Extension.ToUpper(),
                    Name = file.Name.ToUpper(),
                    Length = file.Length,
                    Id = Guid.NewGuid().ToString()
                };

                //CHECK DUP
                if (StorageFileInfo.FindEqual(tableContainer, sfi.Name, sfi.Length))
                {
                    //FOUND SAME
                    Console.WriteLine("DUPLICATE FOUND:" + file.FullName);
                    logFile.WriteLine("DUPLICATE:" + file.FullName);
                }
                else
                {
                    if (file.Extension.Length<=4 && notAllowedExtensions.IndexOf(file.Extension.ToLower())<0)
                    {

                        sfi.EXIF=CreateEXIF(tableContainer, sfi.Id, file.FullName, sfi.Extension, blobContainerEXIF);

                        Console.WriteLine(destDirName + "/" + file.Name);
                        //Console.ReadLine();

                        //UPLOAD BLOB
                        CloudBlockBlob blob = blobContainer.GetBlockBlobReference(sfi.Id);
                        blob.UploadFromFile(file.FullName);

                        if (StorageFileInfo.canCreateThumbnail(file.Extension.ToUpper()))
                        {
                            //REDUCE FILE SIZE
                            var destination = new MemoryStream();
                            ImageBuilder.Current.Build(file.FullName, destination, new ResizeSettings { Width = 1000 });
                            destination.Position = 0;

                            CloudBlockBlob blob1 = blobContainer1000.GetBlockBlobReference(sfi.Id);
                            blob1.UploadFromStream(destination);

                            //QUEUE VISION
                            queue.AddMessage(new CloudQueueMessage("{\"id\":\"" + sfi.Id + "\"}"));
                        }

                        //WRITE TAG 
                        Tags T = new Tags() { Id=sfi.Id, TagType="EVENT", Tag= sfi.Directory };
                        T.Save(tagContainer, tagContainerReverse);

                        sfi.Save(tableContainer);
                        Console.WriteLine(file.FullName + " uploaded:" + sfi.Id);
                    }
                }

            }

            foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(blobContainer, blobContainer1000, tableContainer,  subdir.FullName, temppath, tagContainer, tagContainerReverse,queue, blobContainerEXIF);
                }
        }

    }

}
