using System.Drawing;

namespace IssueTracker.Services.Interfaces
{
    public interface IITFileService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        public string ConvertByteArrayToFile(byte[] fileData, string extension);

        public string GetFileIcon(string file);

        public string FormatFileSize(long bytes); 
    }
}
