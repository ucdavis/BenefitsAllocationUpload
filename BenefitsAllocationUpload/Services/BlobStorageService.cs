using BenefitsAllocationUpload.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;

namespace BenefitsAllocationUpload.Services
{
    public interface IBlobStorageService
    {
        void RenameBlob(string oldName, string newName);
        byte[] GetBlob(string filename);
        void PutBlob(string filepath);

        Uri PutBlob(Stream stream, string filepath);

        bool DeleteBlob(string filename);
        bool Exists(string filename);

        List<FileNames> ListBlobs();

        long GetBlobLength(string filename);
    }

    public class BlobStoargeService : IBlobStorageService
    {
        //private readonly IBlobStoargeService _storageService;
        private readonly CloudBlobContainer _container;

        public BlobStoargeService()
        {
            var storageConnectionString =
            string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                  CloudConfigurationManager.GetSetting("AzureStorageAccountName"),
                  CloudConfigurationManager.GetSetting("AzureStorageKey"));

            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();

            _container = blobClient.GetContainerReference(CloudConfigurationManager.GetSetting("AzureContainerName"));
            _container.CreateIfNotExists();
            _container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off });
        }

        public void RenameBlob(string oldName, string newName)
        {
            _container.Rename(oldName, newName);
        }

        public List<FileNames> ListBlobs()
        {
            var retval = new List<FileNames>();
            var blobs = _container.ListBlobs(null, false);
            var fileId = 0;
            foreach (var item in blobs)
            {
                if (item != null && item is CloudBlockBlob)
                {
                    var blob = (CloudBlockBlob)item;
                    retval.Add(new FileNames()
                    {
                        Length = blob.Properties.Length,
                        TimeStamp = blob.Properties.LastModified,
                        FileName = blob.Name,
                        FileId = fileId,
                        FilePath = blob.Uri.AbsoluteUri
                    });

                    //Console.WriteLine("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri);
                }
                else if (item != null && item is CloudPageBlob)
                {
                    var pageBlob = (CloudPageBlob)item;
                    retval.Add(new FileNames()
                    {
                        Length = pageBlob.Properties.Length,
                        TimeStamp = pageBlob.Properties.LastModified,
                        FileName = pageBlob.Name,
                        FileId = fileId,
                        FilePath = pageBlob.Uri.AbsoluteUri
                    });

                    //Console.WriteLine("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri);

                }
                else if (item != null && item is CloudBlobDirectory)
                {
                    var directory = (CloudBlobDirectory)item;

                    retval.Add(new FileNames()
                    {
                        FileId = fileId,
                        FilePath = directory.Uri.AbsoluteUri
                    });

                    //Console.WriteLine("Directory: {0}", directory.Uri);
                }

                fileId++;
            }

            return retval;
        }

        public byte[] GetBlob(string filename)
        {
            byte[] contents = null;
            try
            {
                //Get file from blob storage and populate the contents
                var blob = _container.GetBlockBlobReference(string.Format("{0}", filename));
                using (var stream = new MemoryStream())
                {
                    blob.DownloadToStream(stream);
                    //stream.Seek(0, SeekOrigin.Begin);
                    contents = stream.ToArray();

                    //using (var reader = new BinaryReader(stream))
                    //{
                    //    stream.Position = 0;
                    //    contents = reader.ReadBytes((int)stream.Length);
                    //}
                }
            }
            catch (Exception)
            {
                return contents;
            }

            return contents;
        }

        public void PutBlob(string filepath)
        {
            var filename = Path.GetFileName(filepath);
            var blob = _container.GetBlockBlobReference(string.Format("{0}", filename));

            using (var fileStream = System.IO.File.OpenRead(filepath))
            {
                blob.UploadFromStream(fileStream);
            }
        }

        public Uri PutBlob(Stream stream, string filepath)
        {
            var filename = Path.GetFileName(filepath);
            var blob = _container.GetBlockBlobReference(string.Format("{0}", filename));

            stream.Seek(0, SeekOrigin.Begin);
            blob.UploadFromStream(stream);

            return blob.Uri;
        }

        public bool Exists(string filename)
        {
            return _container.GetBlockBlobReference(string.Format("{0}", filename)).Exists();
        }

        public bool DeleteBlob(string filename)
        {
            var blob = _container.GetBlockBlobReference(string.Format("{0}", filename));
            return blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public long GetBlobLength(string filename)
        {
            var blob = _container.GetBlobReferenceFromServer(filename);
            return blob.Properties.Length;
        }
    }

    public static class BlobContainerExtensions
    {
        public static void Rename(this CloudBlobContainer container, string oldName, string newName)
        {
            var source = container.GetBlobReferenceFromServer(oldName);
            var target = container.GetBlockBlobReference(newName);
            target.StartCopyFromBlob(source.Uri);
            source.Delete();
        }
    }
}