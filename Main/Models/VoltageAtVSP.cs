using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    [Table("VoltageAtVSPs")]
    public class VoltageAtVSP
    {
        [Key, Column(Order = 0)]
        public int VCP_NodeNumber { get; set; }

        public double Voltage { get; set; }

        public int MinVoltage { get; set; }

        public int MaxVoltage { get; set; }

        [Key, Column(Order = 1)]
        public int Hour { get; set; }
    }
}