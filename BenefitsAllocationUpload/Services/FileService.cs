using FileHelpers;
using BenefitsAllocationUpload.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BenefitsAllocationUpload.Services
{
    /// <summary>
    /// Methods for reading and writing scrubber files.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Given a transactions array, create a scrubber document, upload to the SFTP site and file storage location.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="devarOrgCode"></param>
        /// <param name="transactionsArray"></param>
        /// <returns></returns>
        bool WriteFile(string fileName, string devarOrgCode, FeederSystemFixedLengthRecord[] transactionsArray);

        /// <summary>
        /// Convert the byte array back into a transactions array.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        IList<FeederSystemFixedLengthRecord> ReadFile(byte[] bytes);

        /// <summary>
        /// Convert a memory stream back into a transactions array.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        IList<FeederSystemFixedLengthRecord> ReadFile(Stream stream);

        /// <summary>
        /// Get a file from blob storage
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        byte[] GetFile(string fileName);
    }

    /// <summary>
    /// 
    /// </summary>
    public class FileService : IFileService
    {
        private readonly ISftpService _sftpService;
        private readonly IBlobStorageService _blobStorageService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sftpService"></param>
        /// <param name="blobStorageService"></param>
        public FileService(ISftpService sftpService, IBlobStorageService blobStorageService)
        {
            _sftpService = sftpService;
            _blobStorageService = blobStorageService;
        }

        public bool WriteFile(string fileName, string devarOrgCode, FeederSystemFixedLengthRecord[] transactionsArray)
        {
            var success = false;
            using (var docStream = new MemoryStream())
            {
                using (var flatFileWriter = new StreamWriter(docStream, Encoding.ASCII))
                {
                    var fileWriterEngine = new FileHelperEngine(typeof(FeederSystemFixedLengthRecord));
                    fileWriterEngine.WriteStream(flatFileWriter, transactionsArray);

                    // Flush contents of fileWriterStream to underlying docStream:
                    flatFileWriter.Flush();

                    // Upload to SFTP server:
                    _sftpService.UploadStream(docStream, fileName, devarOrgCode); // Upload file to A&FS server.

                    // Save file to cloud:
                    _blobStorageService.PutBlob(docStream, fileName);
                    success = _blobStorageService.Exists(fileName);
                }

                return success;
            }
        }

        /// <summary>
        /// Get a file from blob storage
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public byte[] GetFile(string fileName)
        {
            return _blobStorageService.GetBlob(fileName);
        }

        public IList<FeederSystemFixedLengthRecord> ReadFile(byte[] bytes)
        {
            var engine = new FileHelperEngine<FeederSystemFixedLengthRecord>();
            var streamReader = new StreamReader(new MemoryStream(bytes));

            var result = engine.ReadStream(streamReader);
            return result.ToList();
        }

        public IList<FeederSystemFixedLengthRecord> ReadFile(Stream stream)
        {
            var engine = new FileHelperEngine<FeederSystemFixedLengthRecord>();
            var streamReader = new StreamReader(stream);

            var result = engine.ReadStream(streamReader);
            return result.ToList();
        }
    }
}