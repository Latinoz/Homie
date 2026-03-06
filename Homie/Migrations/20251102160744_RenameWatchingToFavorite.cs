using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homie.Migrations
{
    /// <inheritdoc />
    public partial class RenameWatchingToFavorite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Проверяем существование колонки и переименовываем только если она существует
            migrationBuilder.Sql(@"
                SET @col_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'MoviesEF' 
                    AND COLUMN_NAME = 'Watching'
                );
                
                SET @sql = IF(@col_exists > 0, 
                    'ALTER TABLE `MoviesEF` CHANGE `Watching` `Favorite` tinyint(1) NOT NULL',
                    'SELECT ''Column Watching does not exist, skipping rename'' AS message'
                );
                
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Обратное переименование
            migrationBuilder.Sql(@"
                SET @col_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = 'MoviesEF' 
                    AND COLUMN_NAME = 'Favorite'
                );
                
                SET @sql = IF(@col_exists > 0, 
                    'ALTER TABLE `MoviesEF` CHANGE `Favorite` `Watching` tinyint(1) NOT NULL',
                    'SELECT ''Column Favorite does not exist, skipping rename'' AS message'
                );
                
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }
    }
}
