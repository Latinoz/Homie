using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Homie.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Homie.Areas.Battletech.Models
{
    public class BTMechsModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Tonnage { get; set; }        
        public int Experience { get; set; }
        public int Bv { get; set; }        
        public int Sent { get; set; }
        public int Return { get; set; }
        public int State { get; set; }

        //public enum State
        //{
        //    Normal = 1,
        //    Damaged = 2,
        //    Destroyed = 3
        //}        

        //public enum TypeMechEnum
        //{
        //    Assault = 1,
        //    Heavy   = 2,
        //    Medium  = 3,
        //    Light   = 4
        //}

        public string TypeMech { get; set; }

        [NotMapped]
        public List<SelectListItem> TypeMechList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "ASS", Text = "Assault" },
            new SelectListItem { Value = "HEA", Text = "Heavy" },
            new SelectListItem { Value = "MED", Text = "Medium"  },
            new SelectListItem { Value = "LIG", Text = "Light"  },
        };

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string PilotUid { get; set; }

        public int GameType { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string ImgBT { get; set; }

        public Image ImgVMid { get; set; }
        
        public byte[] Avatar { get; set; }
    }
    
}
