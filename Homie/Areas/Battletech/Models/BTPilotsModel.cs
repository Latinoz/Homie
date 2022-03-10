using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Battletech.Models
{
    public class BTPilotsModel
    {
        //public BTPilotsModel()
        //{
        //    BTMechsModel = new List<BTMechsModel>();
        //}

        public int Id { get; set; }
        public string Name { get; set; }        
        public int Experience { get; set; }        
        public int Sent { get; set; }
        public int Return { get; set; }
        public string Stats { get; set; }
        public string Raiting { get; set; } 
        public int Hits { get; set; }        

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }
        
        public int MechId { get; set; }

        public int GameType { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string ImgBT { get; set; }

        public byte[] Avatar { get; set; }        

        public BTMechsModel BTMechsModel { get; set; } 
    }
}
