using System;

namespace Homie.Areas.Series.Models
{
    public class MovieListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Category { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public DateTime? HoldPlay { get; set; }
        public bool Archive { get; set; }
        public bool Watching { get; set; }
        public string UserUid { get; set; }
        public string ImgBT { get; set; }
    }
}