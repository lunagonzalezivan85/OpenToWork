using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <summary>
    /// Retos de hosteleria ("Demuestra tus habilidades", 3-Oct): competencias, retos, versiones, intentos,
    /// respuestas y revisiones + descripcion en PT_JobTypes. Se quito el ruido de seeds que EF regenera.
    /// </summary>
    public partial class HospitalityChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PT_JobTypes",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_Challenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Slug = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DraftJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasDraftChanges = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LatestVersionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LatestVersionNumber = table.Column<int>(type: "int", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PublishedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ArchivedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_Challenges", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_Competencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Slug = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_Competencies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_ChallengeJobTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_ChallengeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_JobTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_ChallengeJobTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_ChallengeJobTypes_PT_Challenges_PT_ChallengeId",
                        column: x => x.PT_ChallengeId,
                        principalTable: "PT_Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PT_ChallengeJobTypes_PT_JobTypes_PT_JobTypeId",
                        column: x => x.PT_JobTypeId,
                        principalTable: "PT_JobTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_ChallengeVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_ChallengeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    ContentJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PublishedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_ChallengeVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_ChallengeVersions_PT_Challenges_PT_ChallengeId",
                        column: x => x.PT_ChallengeId,
                        principalTable: "PT_Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_JobTypeCompetencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_JobTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CompetencyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_JobTypeCompetencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_JobTypeCompetencies_PT_Competencies_PT_CompetencyId",
                        column: x => x.PT_CompetencyId,
                        principalTable: "PT_Competencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PT_JobTypeCompetencies_PT_JobTypes_PT_JobTypeId",
                        column: x => x.PT_JobTypeId,
                        principalTable: "PT_JobTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_ChallengeAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CandidateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_ChallengeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_ChallengeVersionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewStatus = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    ConditionsAcceptedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResultJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RowVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_ChallengeAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_ChallengeAttempts_PT_Candidates_PT_CandidateId",
                        column: x => x.PT_CandidateId,
                        principalTable: "PT_Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PT_ChallengeAttempts_PT_ChallengeVersions_PT_ChallengeVersio~",
                        column: x => x.PT_ChallengeVersionId,
                        principalTable: "PT_ChallengeVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PT_ChallengeAttempts_PT_Challenges_PT_ChallengeId",
                        column: x => x.PT_ChallengeId,
                        principalTable: "PT_Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_ChallengeAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_ChallengeAttemptId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ActivityKey = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AutoScore = table.Column<decimal>(type: "decimal(6,4)", nullable: true),
                    SavedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SaveCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_ChallengeAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_ChallengeAnswers_PT_ChallengeAttempts_PT_ChallengeAttempt~",
                        column: x => x.PT_ChallengeAttemptId,
                        principalTable: "PT_ChallengeAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_ChallengeReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_ChallengeAttemptId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReviewerUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ScoresJson = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeneralComment = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_ChallengeReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_ChallengeReviews_PT_ChallengeAttempts_PT_ChallengeAttempt~",
                        column: x => x.PT_ChallengeAttemptId,
                        principalTable: "PT_ChallengeAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_ChallengeAnswers_PT_ChallengeAttemptId_ActivityKey",
                table: "PT_ChallengeAnswers",
                columns: new[] { "PT_ChallengeAttemptId", "ActivityKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_ChallengeAttempts_PT_CandidateId_PT_ChallengeId_Mode_Stat~",
                table: "PT_ChallengeAttempts",
                columns: new[] { "PT_CandidateId", "PT_ChallengeId", "Mode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_ChallengeAttempts_PT_ChallengeId",
                table: "PT_ChallengeAttempts",
                column: "PT_ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_ChallengeAttempts_PT_ChallengeVersionId",
                table: "PT_ChallengeAttempts",
                column: "PT_ChallengeVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_ChallengeAttempts_ReviewStatus",
                table: "PT_ChallengeAttempts",
                column: "ReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PT_ChallengeJobTypes_PT_ChallengeId_PT_JobTypeId_IsDeleted",
                table: "PT_ChallengeJobTypes",
                columns: new[] { "PT_ChallengeId", "PT_JobTypeId", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_ChallengeJobTypes_PT_JobTypeId",
                table: "PT_ChallengeJobTypes",
                column: "PT_JobTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_ChallengeReviews_PT_ChallengeAttemptId",
                table: "PT_ChallengeReviews",
                column: "PT_ChallengeAttemptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_Challenges_Slug",
                table: "PT_Challenges",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_Challenges_Status",
                table: "PT_Challenges",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PT_ChallengeVersions_PT_ChallengeId_VersionNumber",
                table: "PT_ChallengeVersions",
                columns: new[] { "PT_ChallengeId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_Competencies_Slug",
                table: "PT_Competencies",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_JobTypeCompetencies_PT_CompetencyId",
                table: "PT_JobTypeCompetencies",
                column: "PT_CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_JobTypeCompetencies_PT_JobTypeId_PT_CompetencyId_IsDeleted",
                table: "PT_JobTypeCompetencies",
                columns: new[] { "PT_JobTypeId", "PT_CompetencyId", "IsDeleted" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_ChallengeAnswers");

            migrationBuilder.DropTable(
                name: "PT_ChallengeJobTypes");

            migrationBuilder.DropTable(
                name: "PT_ChallengeReviews");

            migrationBuilder.DropTable(
                name: "PT_JobTypeCompetencies");

            migrationBuilder.DropTable(
                name: "PT_ChallengeAttempts");

            migrationBuilder.DropTable(
                name: "PT_Competencies");

            migrationBuilder.DropTable(
                name: "PT_ChallengeVersions");

            migrationBuilder.DropTable(
                name: "PT_Challenges");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PT_JobTypes");
        }
    }
}
