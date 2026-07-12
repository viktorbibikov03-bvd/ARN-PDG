using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    [Table("PQ-Diagrams")]
    public class PQDiagram
    {
        [Key, Column("PQDiagramNumber", Order = 0)]
        public int PQDiagramNumber { get; set; }

        [Key, Column("ActivePower", Order = 1)]
        public double ActivePower { get; set; }

        [Column("MaxReactivePower")]
        public double MaxReactivePower { get; set; }

        [Column("MinReactivePower")]
        public double MinReactivePower { get; set; }
    }
}