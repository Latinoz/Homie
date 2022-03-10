using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Homie.Areas.Battletech.Models
{
    public class BTMechsModel
    {
        //public BTMechsModel()
        //{
        //    BTPilotsModel = new List<BTPilotsModel> ();
        //}


        public int Id { get; set; }
        public string Name { get; set; }
        public int Tonnage { get; set; }        
        public int Experience { get; set; }
        public int Bv { get; set; }        
        public int Sent { get; set; }
        public int Return { get; set; }
        public int State { get; set; }
        
        public string TypeMech { get; set; }

        [NotMapped]
        public List<SelectListItem> TypeMechList { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "ASS", Text = "Assault" },
            new SelectListItem { Value = "HEA", Text = "Heavy" },
            new SelectListItem { Value = "MED", Text = "Medium"  },
            new SelectListItem { Value = "LIG", Text = "Light"  },
            new SelectListItem { Value = "Empty", Text = "" }
        };

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }        

        public int GameType { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string ImgBT { get; set; }
        
        public byte[] Avatar { get; set; }

        public int? BTPilotsModelId { get; set; }

        public BTPilotsModel BTPilotsModel { get; set; }

    }
    
}
