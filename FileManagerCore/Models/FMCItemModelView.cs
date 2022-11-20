namespace FileManagerCore.Models
{
    public class FMCItemModelView
    {
        public string Path { get; set; }
        public bool IsFolder { get; set; }
        public bool IsImage { get; set; }
        public string MimeType { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string ImageThumbs { get; set; }
        public string FullImage { get; set; }
        
    }
}
