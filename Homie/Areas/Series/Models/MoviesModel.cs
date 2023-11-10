using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Series.Models
{
    public class MoviesModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Category { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        
        [BindProperty, DataType(DataType.Time)] 
        public DateTime? HoldPlay { get; set; }       
        public bool Archive { get; set; }
        public bool Watching { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string ImgBT { get; set; }

        public byte[] Avatar { get; set; }

    }
}
