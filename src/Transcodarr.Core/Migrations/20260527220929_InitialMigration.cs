using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transcodarr.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AutoApplyTranscode = table.Column<bool>(type: "INTEGER", nullable: false),
                    TranscodeTempDirectory = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                    TranscodeAudioCodec = table.Column<int>(type: "INTEGER", nullable: false),
                    TranscodeEncoderPreset = table.Column<int>(type: "INTEGER", nullable: false),
                    TranscodeVideoCodec = table.Column<int>(type: "INTEGER", nullable: false),
                    ConstantRateFactor = table.Column<int>(type: "INTEGER", nullable: false),
                    JobExpirationInMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileSystemPath = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                    DiscoveredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FileModifiedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaFiles_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaFileMetadataEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaFileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VideoCodec = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    BitRate = table.Column<long>(type: "INTEGER", nullable: false),
                    IsHdr = table.Column<bool>(type: "INTEGER", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    AudioStreams = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFileMetadataEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaFileMetadataEntity_MediaFiles_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TranscodeJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaFileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NodeId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    OutputPath = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                    ConstantRateFactor = table.Column<int>(type: "INTEGER", nullable: false),
                    AudioCodec = table.Column<int>(type: "INTEGER", nullable: false),
                    VideoCodec = table.Column<int>(type: "INTEGER", nullable: false),
                    EncoderPreset = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Progress = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LeaseExpiresAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranscodeJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranscodeJobs_MediaFiles_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TranscodeResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TranscodeJobId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EncoderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    VmafScore = table.Column<double>(type: "REAL", nullable: true),
                    ApprovalState = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranscodeResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranscodeResults_TranscodeJobs_TranscodeJobId",
                        column: x => x.TranscodeJobId,
                        principalTable: "TranscodeJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFileMetadataEntity_MediaFileId",
                table: "MediaFileMetadataEntity",
                column: "MediaFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_LibraryId",
                table: "MediaFiles",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Status",
                table: "MediaFiles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TranscodeJobs_MediaFileId",
                table: "TranscodeJobs",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_TranscodeJobs_Status_LeaseExpiresAt",
                table: "TranscodeJobs",
                columns: new[] { "Status", "LeaseExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TranscodeResults_TranscodeJobId",
                table: "TranscodeResults",
                column: "TranscodeJobId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigurations");

            migrationBuilder.DropTable(
                name: "MediaFileMetadataEntity");

            migrationBuilder.DropTable(
                name: "TranscodeResults");

            migrationBuilder.DropTable(
                name: "TranscodeJobs");

            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.DropTable(
                name: "Libraries");
        }
    }
}
