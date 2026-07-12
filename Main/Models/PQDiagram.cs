using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    [Table("PQ-Diagrams")]
    public class PQDiagram
    {
        [Key, Column(Order = 0)]
        public int PQDiagramNumber { get; set; }

        [Key, Column(Order = 1)]
        public double ActivePower { get; set; }

        public double MaxReactivePower { get; set; }

        public double MinReactivePower { get; set; }
    }
}