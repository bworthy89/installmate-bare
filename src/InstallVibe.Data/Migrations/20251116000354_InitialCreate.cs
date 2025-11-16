using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstallVibe.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    FavoriteId = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    GuideId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.FavoriteId);
                });

            migrationBuilder.CreateTable(
                name: "Guides",
                columns: table => new
                {
                    GuideId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    RequiredLicense = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Published = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocalPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    SharePointPath = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CachedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Checksum = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    SyncStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "synced"),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: true),
                    StepCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guides", x => x.GuideId);
                });

            migrationBuilder.CreateTable(
                name: "MediaCache",
                columns: table => new
                {
                    MediaId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LocalPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    SharePointPath = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    Checksum = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CachedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastAccessed = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    AccessCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsShared = table.Column<bool>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaCache", x => x.MediaId);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    EncryptedValue = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "SyncMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastSyncDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ServerVersion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LocalVersion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    SyncStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "synced"),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncMetadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Progress",
                columns: table => new
                {
                    ProgressId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    GuideId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CurrentStepId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    StepProgress = table.Column<string>(type: "TEXT", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    PercentComplete = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Progress", x => x.ProgressId);
                    table.ForeignKey(
                        name: "FK_Progress_Guides_GuideId",
                        column: x => x.GuideId,
                        principalTable: "Guides",
                        principalColumn: "GuideId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    StepId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    GuideId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StepNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    MediaReferences = table.Column<string>(type: "TEXT", nullable: true),
                    LocalPath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Checksum = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    CachedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.StepId);
                    table.ForeignKey(
                        name: "FK_Steps_Guides_GuideId",
                        column: x => x.GuideId,
                        principalTable: "Guides",
                        principalColumn: "GuideId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId",
                table: "Favorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_GuideId",
                table: "Favorites",
                columns: new[] { "UserId", "GuideId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_UserId_SortOrder",
                table: "Favorites",
                columns: new[] { "UserId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Guides_Category",
                table: "Guides",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Guides_LastModified",
                table: "Guides",
                column: "LastModified");

            migrationBuilder.CreateIndex(
                name: "IX_Guides_SyncStatus",
                table: "Guides",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MediaCache_Category",
                table: "MediaCache",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_MediaCache_FileSize",
                table: "MediaCache",
                column: "FileSize");

            migrationBuilder.CreateIndex(
                name: "IX_MediaCache_LastAccessed",
                table: "MediaCache",
                column: "LastAccessed");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_GuideId",
                table: "Progress",
                column: "GuideId");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_LastUpdated",
                table: "Progress",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_UserId",
                table: "Progress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Category",
                table: "Settings",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_GuideId_StepNumber",
                table: "Steps",
                columns: new[] { "GuideId", "StepNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncMetadata_EntityType_EntityId",
                table: "SyncMetadata",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncMetadata_SyncStatus",
                table: "SyncMetadata",
                column: "SyncStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "MediaCache");

            migrationBuilder.DropTable(
                name: "Progress");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "SyncMetadata");

            migrationBuilder.DropTable(
                name: "Guides");
        }
    }
}
