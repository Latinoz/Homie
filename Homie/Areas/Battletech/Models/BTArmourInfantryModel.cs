using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Battletech.Models
{
    public class BTArmourInfantryModel
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

        public int TypeArmor { get; set; }

        //public enum Type
        //{
        //    BattleArmour = 1,
        //    Infantry = 2,           
        //}       

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }

        public int Game { get; set; }
    }

}
