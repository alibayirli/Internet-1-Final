using Internet_1.Models;

namespace Internet_1.ViewModels
{
    public class FileUploadViewModel : FileModal
    {
        public byte[] FileData { get; set; }

        public List<FileModal> SystemFiles { get; set; }
    }
}
