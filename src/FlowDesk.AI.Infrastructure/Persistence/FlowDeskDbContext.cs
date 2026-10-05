using Microsoft.EntityFrameworkCore;

namespace FlowDesk.AI.Infrastructure.Persistence;

public sealed class FlowDeskDbContext(DbContextOptions<FlowDeskDbContext> options) : DbContext(options)
{
    public DbSet<KnowledgeDocumentRecord> KnowledgeDocuments => Set<KnowledgeDocumentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<KnowledgeDocumentRecord>(entity =>
        {
            entity.ToTable("knowledge_documents");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Source)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.Content)
                .IsRequired();

            entity.Property(x => x.Embedding)
                .HasColumnType("vector");

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });
    }
}
