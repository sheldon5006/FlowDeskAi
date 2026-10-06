using Microsoft.EntityFrameworkCore;

namespace FlowDesk.AI.Infrastructure.Persistence;

public sealed class FlowDeskDbContext(DbContextOptions<FlowDeskDbContext> options) : DbContext(options)
{
    public DbSet<BusinessRecord> Businesses => Set<BusinessRecord>();

    public DbSet<KnowledgeDocumentRecord> KnowledgeDocuments => Set<KnowledgeDocumentRecord>();

    public DbSet<KnowledgeChunkRecord> KnowledgeChunks => Set<KnowledgeChunkRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<BusinessRecord>(entity =>
        {
            entity.ToTable("businesses");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Slug)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.Slug)
                .IsUnique();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });

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

            entity.HasIndex(x => x.BusinessId);

            entity.HasOne(x => x.Business)
                .WithMany(x => x.KnowledgeDocuments)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
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
