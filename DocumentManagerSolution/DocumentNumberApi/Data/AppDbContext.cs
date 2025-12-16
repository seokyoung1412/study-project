// DocumentNumberApi/Data/AppDbContext.cs
using DocumentNumberApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentNumberApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<DocumentNumberSequence> DocumentSequences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 복합 키 설정
            modelBuilder.Entity<DocumentNumberSequence>()
                .HasKey(s => new { s.DocumentType, s.DepartmentCode });
            // 테이블 이름 설정
            modelBuilder.Entity<DocumentNumberSequence>().ToTable("DocumentSequence");
        }
    }
}