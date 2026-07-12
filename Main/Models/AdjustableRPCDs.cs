using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    [Table("Adjustable RPCDs")]
    public class AdjustableRPCD
    {
        [Key, Column(Order = 0)]
        public int ControlledRPCD_NodeNumber { get; set; }

        public double ReactivePower { get; set; }

        public string Name { get; set; }

        public double MinReactivePower { get; set; }

        public double MaxReactivePower { get; set; }

        [Column("On/Off")]
        public bool OnOff { get; set; }

        [Key, Column(Order = 1)]
        public int Hour { get; set; }

        public double VoltageAtNode { get; set; }
    }
}