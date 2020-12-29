using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

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

        public int TypeMech { get; set; }

        //public enum Type
        //{
        //    Assault = 1,
        //    Heavy   = 2,
        //    Medium  = 3,
        //    Light   = 4
        //}

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string PilotUid { get; set; }

        public int Game { get; set; }

    }
    
}
