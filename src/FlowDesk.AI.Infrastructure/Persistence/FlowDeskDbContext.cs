using Microsoft.EntityFrameworkCore;

namespace FlowDesk.AI.Infrastructure.Persistence;

public sealed class FlowDeskDbContext(DbContextOptions<FlowDeskDbContext> options) : DbContext(options)
{
    public DbSet<KnowledgeDocumentRecord> KnowledgeDocuments => Set<KnowledgeDocumentRecord>();

    public DbSet<KnowledgeChunkRecord> KnowledgeChunks => Set<KnowledgeChunkRecord>();

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

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<KnowledgeChunkRecord>(entity =>
        {
            entity.ToTable("knowledge_chunks");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Content)
                .IsRequired();

            entity.Property(x => x.Embedding)
                .HasColumnType("vector");

            entity.HasIndex(x => new { x.KnowledgeDocumentId, x.ChunkIndex })
                .IsUnique();

            entity.HasOne(x => x.KnowledgeDocument)
                .WithMany(x => x.Chunks)
                .HasForeignKey(x => x.KnowledgeDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
