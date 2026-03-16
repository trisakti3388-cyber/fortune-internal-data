using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FortuneInternalData.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCanExportAndRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create role_permissions table if it doesn't exist (handles fresh installs)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `role_permissions` (
                    `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
                    `role_id` varchar(450) NOT NULL,
                    `module` varchar(50) NOT NULL,
                    `can_view` tinyint(1) NOT NULL DEFAULT 0,
                    `can_edit` tinyint(1) NOT NULL DEFAULT 0,
                    `can_export` tinyint(1) NOT NULL DEFAULT 0,
                    `created_at` datetime(6) NOT NULL,
                    `updated_at` datetime(6) NOT NULL,
                    PRIMARY KEY (`Id`),
                    UNIQUE KEY `IX_role_permissions_role_id_module` (`role_id`, `module`)
                ) CHARACTER SET utf8mb4;
            ");

            // Add can_export column to existing tables that don't have it yet
            migrationBuilder.Sql(@"
                SET @dbname = DATABASE();
                SET @tablename = 'role_permissions';
                SET @columnname = 'can_export';
                SET @preparedStatement = (SELECT IF(
                    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_SCHEMA = @dbname
                        AND TABLE_NAME = @tablename
                        AND COLUMN_NAME = @columnname) > 0,
                    'SELECT 1',
                    'ALTER TABLE `role_permissions` ADD COLUMN `can_export` tinyint(1) NOT NULL DEFAULT 0'
                ));
                PREPARE alterIfNotExists FROM @preparedStatement;
                EXECUTE alterIfNotExists;
                DEALLOCATE PREPARE alterIfNotExists;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "can_export",
                table: "role_permissions");
        }
    }
}
