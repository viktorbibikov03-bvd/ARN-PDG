using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    [Table("Generators")]
    public class Generator
    {
        [Key, Column(Order = 0)]
        public int GeneratorNodeNumber { get; set; }

        public double BusVoltage { get; set; }

        public int PQDiagramNumber { get; set; }

        public string Name { get; set; }

        [Key, Column(Order = 1)]
        public int Hour { get; set; }

        [Column("On/Off")]
        public bool OnOff { get; set; }
    }
}