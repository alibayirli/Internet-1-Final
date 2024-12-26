using System.ComponentModel.DataAnnotations;

namespace Internet_1.Models
{
    public class FileModal
    {
        public int Id { get; set; }

        [Display(Name = "File Name")]
        public string FileName { get; set; }

        [Display(Name = "File Type")]
        public string FileType { get; set; }

        [Display(Name = "File Extension")]
        public string Extension { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "File Path")]
        public string FilePath { get; set; }

        [Display(Name = "Uploaded By")]
        public string UploadedBy { get; set; }

        [Display(Name = "Uploaded On")]
        public DateTime UploadOn { get; set; }

    }
}