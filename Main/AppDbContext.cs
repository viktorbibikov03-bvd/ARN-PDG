using System.Data.Entity;
using Model.Models;

namespace Model
{
    [DbConfigurationType(typeof(SQLiteConfiguration))]
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("DefaultConnection")
        {
            Database.SetInitializer<AppDbContext>(null);
        }

        public DbSet<AdjustableRPCD> AdjustableRPCDs { get; set; }
        public DbSet<EfficiencyTable> EfficiencyTables { get; set; }
        public DbSet<Generator> Generators { get; set; }
        public DbSet<PQDiagram> PQDiagrams { get; set; }
        public DbSet<SwitchedRPCD> SwitchedRPCDs { get; set; }
        public DbSet<VoltageAtVSP> VoltageAtVSPs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AdjustableRPCD>().ToTable("Adjustable RPCDs");
            modelBuilder.Entity<EfficiencyTable>().ToTable("EfficiencyTable");
            modelBuilder.Entity<Generator>().ToTable("Generators");
            modelBuilder.Entity<PQDiagram>().ToTable("PQ-Diagrams");
            modelBuilder.Entity<SwitchedRPCD>().ToTable("SwitchedRPCDs");
            modelBuilder.Entity<VoltageAtVSP>().ToTable("VoltageAtVSPs");
        }
    }
}