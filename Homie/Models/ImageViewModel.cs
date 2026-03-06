using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Homie.Models
{
    public class ImageViewModel
    {
        public string NameImgVM { get; set; }
        public IFormFile AvatarFile { get; set; }
    }
}
