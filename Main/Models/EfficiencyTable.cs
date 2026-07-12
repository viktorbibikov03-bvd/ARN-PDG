using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    [Table("EfficiencyTable")]
    public class EfficiencyTable
    {
        [Key, Column(Order = 0)]
        public int VCP_NodeNumber { get; set; }

        [Key, Column(Order = 1)]
        public int PriorityRPCDNumber { get; set; }

        public int ControlledRPCD_NodeNumber { get; set; }

        public int ReactorNumber { get; set; }

        public int GeneratorNodeNumber { get; set; }
    }
}