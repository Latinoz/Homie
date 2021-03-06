﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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
        public string HoldPlay { get; set; }       
        public bool Archive { get; set; }
        
        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }        

    }
}
