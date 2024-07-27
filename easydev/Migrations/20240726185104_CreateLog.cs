using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace easydev.Migrations
{
    /// <inheritdoc />
    public partial class CreateLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'aal_level') THEN
                    CREATE TYPE aal_level AS ENUM ('level1', 'level2', 'level3');
                END IF;
            END
            $$;
        ");
            // Otras operaciones de migración si es necesario
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TYPE IF EXISTS aal_level;");
            // Otras operaciones para revertir la migración si es necesario
        }
    }
}
