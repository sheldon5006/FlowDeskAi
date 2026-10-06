using Microsoft.EntityFrameworkCore;

namespace FlowDesk.AI.Infrastructure.Persistence;

public sealed class DatabaseInitializer(FlowDeskDbContext dbContext)
{
    public static readonly Guid DefaultBusinessId =
        Guid.Parse("00000000-0000-0000-0000-000000000001");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        const string sql = """
            CREATE TABLE IF NOT EXISTS businesses
            (
                "Id" uuid NOT NULL PRIMARY KEY,
                "Name" character varying(200) NOT NULL,
                "Slug" character varying(100) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_businesses_Slug"
                ON businesses ("Slug");

            INSERT INTO businesses ("Id", "Name", "Slug", "CreatedAtUtc")
            VALUES (
                '00000000-0000-0000-0000-000000000001',
                'Demo Business',
                'demo-business',
                NOW()
            )
            ON CONFLICT ("Id") DO NOTHING;

            ALTER TABLE knowledge_documents
                ADD COLUMN IF NOT EXISTS "BusinessId" uuid;

            UPDATE knowledge_documents
            SET "BusinessId" = '00000000-0000-0000-0000-000000000001'
            WHERE "BusinessId" IS NULL;

            ALTER TABLE knowledge_documents
                ALTER COLUMN "BusinessId" SET NOT NULL;

            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'FK_knowledge_documents_businesses_BusinessId'
                ) THEN
                    ALTER TABLE knowledge_documents
                    ADD CONSTRAINT "FK_knowledge_documents_businesses_BusinessId"
                    FOREIGN KEY ("BusinessId")
                    REFERENCES businesses ("Id")
                    ON DELETE CASCADE;
                END IF;
            END
            $$;

            CREATE INDEX IF NOT EXISTS "IX_knowledge_documents_BusinessId"
                ON knowledge_documents ("BusinessId");
            """;

        await dbContext.Database.ExecuteSqlRawAsync(
            sql,
            cancellationToken);
    }
}
