using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Battletech.Models
{
    public class BTMechModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Tonnage { get; set; }    
        
        // public enum Type { get; set; }     //Enum или Int ???
        public string Experience { get; set; }
        public int Bv { get; set; }        
        public int Sent { get; set; }
        public int Return { get; set; }
        public int State { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string PilotUid { get; set; }


    }

}
