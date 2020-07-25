using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homie.Areas.Battletech.Models
{
    public class BTVehiclesModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Tonnage { get; set; }
        public List<BTVehicleType> Types { get; set; }
        public int Experience { get; set; }
        public int Bv { get; set; }
        public int Sent { get; set; }
        public int Return { get; set; }
        public List<BTVehicleState> States { get; set; }       

        [Column(TypeName = "varchar(255)")]
        public string UserUid { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string PilotUid { get; set; }
    }

    public class BTVehicleType
    {
        public int Id { get; set; }
        public string Content { get; set; }
        //public int BTVehicleModelId { get; set; }
        //public BTVehiclesModel BTVehiclesModel { get; set; }
    }

    public class BTVehicleState
    {
        public int Id { get; set; }
        public string Content { get; set; }
        //public int BTVehicleModelId { get; set; }
        //public BTVehiclesModel BTVehiclesModel { get; set; }
    }
}
