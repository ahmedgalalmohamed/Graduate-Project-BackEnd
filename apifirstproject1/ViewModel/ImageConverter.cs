namespace Graduate_Project_BackEnd.ViewModel
{
    public class ImageConverter
    {
        public static string Converter(string url)
        {
            byte[] bytes = File.ReadAllBytes(url);
            MemoryStream ms = new(bytes);
            string img = "data:image/png;base64, " + Convert.ToBase64String(ms.ToArray());
            ms.Close();
            return img;
        }
    }
}
