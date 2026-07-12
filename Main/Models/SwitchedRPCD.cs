using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    [Table("SwitchedRPCDs")]
    public class SwitchedRPCD
    {
        [Key, Column(Order = 0)]
        public int ReactorNumber { get; set; }

        public string Name { get; set; }

        public double Capacity { get; set; }

        [Column("On/Off")]
        public bool OnOff { get; set; }

        [Key, Column(Order = 1)]
        public int Hour { get; set; }
    }
}