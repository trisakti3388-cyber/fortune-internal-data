using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FortuneInternalData.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedRowsAndErrorMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "processed_rows",
                table: "import_batches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "error_message",
                table: "import_batches",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "processed_rows",
                table: "import_batches");

            migrationBuilder.DropColumn(
                name: "error_message",
                table: "import_batches");
        }
    }
}
