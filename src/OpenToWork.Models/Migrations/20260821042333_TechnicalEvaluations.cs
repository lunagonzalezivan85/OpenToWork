using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class TechnicalEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("02764e3c-cb33-40c4-bbf4-6f5df36936b9"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("0285e1fb-e821-4501-8726-72bbadf6ac1b"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("3afd48b6-8f39-41e9-be6b-f7f4a18b6756"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("7ced2b85-1c25-41c7-b59c-d4ec16c7c98d"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("7e9d304e-dc6f-4b93-9691-c6c6b5c159e8"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("950b6b03-4eee-4d20-be6b-8f6826000b4e"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("a84e2e54-deda-4bc7-a9f1-11f9f8cbda28"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("acd3b81d-7722-4c9d-983f-5ddc01ddba47"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("f2c4eeac-46d6-4241-821d-e11ed5dacbda"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("fbc84e9b-1a41-469c-bf98-089b9a3dd857"));

            migrationBuilder.CreateTable(
                name: "PT_TechnicalEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CandidateRecruitmentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EvaluationName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Score = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    EvidenceUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EvaluatedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_PT_TechnicalEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_TechnicalEvaluations_PT_CandidateRecruitments_PT_Candidat~",
                        column: x => x.PT_CandidateRecruitmentId,
                        principalTable: "PT_CandidateRecruitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_TechnicalEvaluations_SC_Users_EvaluatedByUserId",
                        column: x => x.EvaluatedByUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("1f00cfa0-9d1d-46bc-a43d-514535f03fbb"), new DateTime(2026, 8, 21, 4, 23, 32, 897, DateTimeKind.Utc).AddTicks(801), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("4c28b271-6c5c-47c4-b174-af6a26e08c41"), new DateTime(2026, 8, 21, 4, 23, 32, 897, DateTimeKind.Utc).AddTicks(843), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("53822315-4289-4cc7-a580-3ef720e2e8a5"), new DateTime(2026, 8, 21, 4, 23, 32, 897, DateTimeKind.Utc).AddTicks(834), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("571cf977-b8bf-45d6-a66a-863bf4c8f9f6"), new DateTime(2026, 8, 21, 4, 23, 32, 897, DateTimeKind.Utc).AddTicks(839), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("6a4abf6e-a36f-44c7-9116-a479408e3d1e"), new DateTime(2026, 8, 21, 4, 23, 32, 897, DateTimeKind.Utc).AddTicks(817), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("8f3131fd-23ef-4bfd-9bdd-dfa1333ace85"), new DateTime(2026, 8, 21, 4, 23, 32, 897, DateTimeKind.Utc).AddTicks(821), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("a746271d-da37-4ea1-a0cd-7e876a31b7db"), new DateTime(2026, 8, 21, 4, 23, 32, 897, DateTimeKind.Utc).AddTicks(831), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("c1c0f1d6-3db5-45d2-81c9-413dc888cd86"), new DateTime(2026, 8, 21, 4, 23, 32, 897, DateTimeKind.Utc).AddTicks(836), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("cd014bac-f600-4aab-9f41-a0cb55fb2df5"), new DateTime(2026, 8, 21, 4, 23, 32, 897, DateTimeKind.Utc).AddTicks(826), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("e73b1ee0-1d3f-4af7-9ed9-d30672f71eba"), new DateTime(2026, 8, 21, 4, 23, 32, 897, DateTimeKind.Utc).AddTicks(823), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PT_TechnicalEvaluations_EvaluatedByUserId_IsDeleted",
                table: "PT_TechnicalEvaluations",
                columns: new[] { "EvaluatedByUserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_TechnicalEvaluations_PT_CandidateRecruitmentId_IsDeleted",
                table: "PT_TechnicalEvaluations",
                columns: new[] { "PT_CandidateRecruitmentId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_TechnicalEvaluations");

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("1f00cfa0-9d1d-46bc-a43d-514535f03fbb"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("4c28b271-6c5c-47c4-b174-af6a26e08c41"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("53822315-4289-4cc7-a580-3ef720e2e8a5"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("571cf977-b8bf-45d6-a66a-863bf4c8f9f6"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("6a4abf6e-a36f-44c7-9116-a479408e3d1e"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("8f3131fd-23ef-4bfd-9bdd-dfa1333ace85"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("a746271d-da37-4ea1-a0cd-7e876a31b7db"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c1c0f1d6-3db5-45d2-81c9-413dc888cd86"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("cd014bac-f600-4aab-9f41-a0cb55fb2df5"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("e73b1ee0-1d3f-4af7-9ed9-d30672f71eba"));

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("02764e3c-cb33-40c4-bbf4-6f5df36936b9"), new DateTime(2026, 8, 21, 3, 53, 5, 943, DateTimeKind.Utc).AddTicks(5062), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("0285e1fb-e821-4501-8726-72bbadf6ac1b"), new DateTime(2026, 8, 21, 3, 53, 5, 943, DateTimeKind.Utc).AddTicks(5070), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("3afd48b6-8f39-41e9-be6b-f7f4a18b6756"), new DateTime(2026, 8, 21, 3, 53, 5, 943, DateTimeKind.Utc).AddTicks(5064), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("7ced2b85-1c25-41c7-b59c-d4ec16c7c98d"), new DateTime(2026, 8, 21, 3, 53, 5, 943, DateTimeKind.Utc).AddTicks(5043), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("7e9d304e-dc6f-4b93-9691-c6c6b5c159e8"), new DateTime(2026, 8, 21, 3, 53, 5, 943, DateTimeKind.Utc).AddTicks(5045), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("950b6b03-4eee-4d20-be6b-8f6826000b4e"), new DateTime(2026, 8, 21, 3, 53, 5, 943, DateTimeKind.Utc).AddTicks(5023), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("a84e2e54-deda-4bc7-a9f1-11f9f8cbda28"), new DateTime(2026, 8, 21, 3, 53, 5, 943, DateTimeKind.Utc).AddTicks(5040), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("acd3b81d-7722-4c9d-983f-5ddc01ddba47"), new DateTime(2026, 8, 21, 3, 53, 5, 943, DateTimeKind.Utc).AddTicks(5049), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("f2c4eeac-46d6-4241-821d-e11ed5dacbda"), new DateTime(2026, 8, 21, 3, 53, 5, 943, DateTimeKind.Utc).AddTicks(5067), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("fbc84e9b-1a41-469c-bf98-089b9a3dd857"), new DateTime(2026, 8, 21, 3, 53, 5, 943, DateTimeKind.Utc).AddTicks(5036), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null }
                });
        }
    }
}
