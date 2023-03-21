namespace Graduate_Project_BackEnd.ViewModel
{
    public class ImageConverter
    {
        private IFormFile? file;
        private string? url;
        public ImageConverter(IFormFile file)
        {
            this.file = file;
        }
        public ImageConverter(string url)
        {
            this.url = url;
        }
        public string Converter()
        {
            MemoryStream ms = new();
            if (file != null) file.CopyTo(ms);
            else if (url != null)
            {
                byte[] bytes = File.ReadAllBytes(url);
                ms = new(bytes);
            }
            string img = "data:image/png;base64, " + Convert.ToBase64String(ms.ToArray());
            ms.Close();
            return img;
        }
    }
}
