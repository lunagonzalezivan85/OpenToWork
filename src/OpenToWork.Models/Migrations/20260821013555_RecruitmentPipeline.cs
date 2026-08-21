using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class RecruitmentPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("1ecde287-d0b3-48d0-8083-dca24bf5b68f"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("20df1f1a-381e-4e7a-92ec-49ffa4f343c9"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("8042dfc4-c69a-41e6-ab7f-bb3096f578c9"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("909facd3-124c-4f85-8def-5db0d8d7e4a6"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("a46cba54-3d26-49b9-889b-f41e967175a2"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("abfb92bb-6e9e-4c31-be9d-45b464121bbe"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c64c4bf4-675f-4092-9ea0-a1a7f90aa9f5"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("def9e009-3812-4deb-89e4-9de11393eab1"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("ea939395-9f9c-416a-a063-7664b1d34684"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("f5585533-9b9b-473a-be10-321bde41306f"));

            migrationBuilder.CreateTable(
                name: "PT_CandidateRecruitments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SCUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_VacancyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CurrentStage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    AssignedToUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AssignedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StageEnteredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
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
                    table.PrimaryKey("PK_PT_CandidateRecruitments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_CandidateRecruitments_PT_Vacancies_PT_VacancyId",
                        column: x => x.PT_VacancyId,
                        principalTable: "PT_Vacancies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PT_CandidateRecruitments_SC_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PT_CandidateRecruitments_SC_Users_SCUserId",
                        column: x => x.SCUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_InvestigationChecklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CandidateRecruitmentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Step = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EvidenceUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
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
                    table.PrimaryKey("PK_PT_InvestigationChecklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_InvestigationChecklists_PT_CandidateRecruitments_PT_Candi~",
                        column: x => x.PT_CandidateRecruitmentId,
                        principalTable: "PT_CandidateRecruitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_InvestigationChecklists_SC_Users_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_RecruitmentDismissals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CandidateRecruitmentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DismissedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
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
                    table.PrimaryKey("PK_PT_RecruitmentDismissals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_RecruitmentDismissals_PT_CandidateRecruitments_PT_Candida~",
                        column: x => x.PT_CandidateRecruitmentId,
                        principalTable: "PT_CandidateRecruitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_RecruitmentDismissals_SC_Users_DismissedByUserId",
                        column: x => x.DismissedByUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_RecruitmentStageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CandidateRecruitmentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FromStage = table.Column<int>(type: "int", nullable: false),
                    ToStage = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
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
                    table.PrimaryKey("PK_PT_RecruitmentStageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_RecruitmentStageLogs_PT_CandidateRecruitments_PT_Candidat~",
                        column: x => x.PT_CandidateRecruitmentId,
                        principalTable: "PT_CandidateRecruitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_RecruitmentStageLogs_SC_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
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
                    { new Guid("59bdd93c-0312-4c81-a719-98581578cab6"), new DateTime(2026, 8, 21, 1, 35, 55, 225, DateTimeKind.Utc).AddTicks(344), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("76453615-c21e-41e5-9c45-70251d1cfd60"), new DateTime(2026, 8, 21, 1, 35, 55, 225, DateTimeKind.Utc).AddTicks(332), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("7c53b291-24aa-4174-be3c-d5e05be88c1f"), new DateTime(2026, 8, 21, 1, 35, 55, 225, DateTimeKind.Utc).AddTicks(327), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("82547b43-0a0a-44d1-befd-4d4970c2f143"), new DateTime(2026, 8, 21, 1, 35, 55, 225, DateTimeKind.Utc).AddTicks(330), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("969d86fc-73e9-4bba-948e-99a49eb1f5e0"), new DateTime(2026, 8, 21, 1, 35, 55, 225, DateTimeKind.Utc).AddTicks(342), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("9c628313-9849-4c22-84dc-6d1dd6365a94"), new DateTime(2026, 8, 21, 1, 35, 55, 225, DateTimeKind.Utc).AddTicks(339), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("b6b78dca-4e08-4b20-822d-72e3c781f319"), new DateTime(2026, 8, 21, 1, 35, 55, 225, DateTimeKind.Utc).AddTicks(310), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("f12968e7-7a57-44ff-bfd0-879c0b52cdad"), new DateTime(2026, 8, 21, 1, 35, 55, 225, DateTimeKind.Utc).AddTicks(324), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("fa073ff0-064a-4e35-a2ce-0fac33fa1ee6"), new DateTime(2026, 8, 21, 1, 35, 55, 225, DateTimeKind.Utc).AddTicks(337), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("fddf973f-fd90-4546-989f-8ca3c7f469da"), new DateTime(2026, 8, 21, 1, 35, 55, 225, DateTimeKind.Utc).AddTicks(348), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRecruitments_AssignedToUserId_IsDeleted",
                table: "PT_CandidateRecruitments",
                columns: new[] { "AssignedToUserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRecruitments_CurrentStage_IsDeleted",
                table: "PT_CandidateRecruitments",
                columns: new[] { "CurrentStage", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRecruitments_PT_VacancyId",
                table: "PT_CandidateRecruitments",
                column: "PT_VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRecruitments_SCUserId_IsDeleted",
                table: "PT_CandidateRecruitments",
                columns: new[] { "SCUserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRecruitments_SCUserId_PT_VacancyId_IsDeleted",
                table: "PT_CandidateRecruitments",
                columns: new[] { "SCUserId", "PT_VacancyId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_InvestigationChecklists_CompletedByUserId",
                table: "PT_InvestigationChecklists",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_InvestigationChecklists_IsCompleted_IsDeleted",
                table: "PT_InvestigationChecklists",
                columns: new[] { "IsCompleted", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_InvestigationChecklists_PT_CandidateRecruitmentId_Step_Is~",
                table: "PT_InvestigationChecklists",
                columns: new[] { "PT_CandidateRecruitmentId", "Step", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentDismissals_DismissedByUserId",
                table: "PT_RecruitmentDismissals",
                column: "DismissedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentDismissals_PT_CandidateRecruitmentId",
                table: "PT_RecruitmentDismissals",
                column: "PT_CandidateRecruitmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentDismissals_PT_CandidateRecruitmentId_IsDeleted",
                table: "PT_RecruitmentDismissals",
                columns: new[] { "PT_CandidateRecruitmentId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentStageLogs_ChangedByUserId",
                table: "PT_RecruitmentStageLogs",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentStageLogs_CreatedAt_IsDeleted",
                table: "PT_RecruitmentStageLogs",
                columns: new[] { "CreatedAt", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentStageLogs_PT_CandidateRecruitmentId_IsDeleted",
                table: "PT_RecruitmentStageLogs",
                columns: new[] { "PT_CandidateRecruitmentId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_InvestigationChecklists");

            migrationBuilder.DropTable(
                name: "PT_RecruitmentDismissals");

            migrationBuilder.DropTable(
                name: "PT_RecruitmentStageLogs");

            migrationBuilder.DropTable(
                name: "PT_CandidateRecruitments");

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("59bdd93c-0312-4c81-a719-98581578cab6"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("76453615-c21e-41e5-9c45-70251d1cfd60"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("7c53b291-24aa-4174-be3c-d5e05be88c1f"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("82547b43-0a0a-44d1-befd-4d4970c2f143"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("969d86fc-73e9-4bba-948e-99a49eb1f5e0"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("9c628313-9849-4c22-84dc-6d1dd6365a94"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("b6b78dca-4e08-4b20-822d-72e3c781f319"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("f12968e7-7a57-44ff-bfd0-879c0b52cdad"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("fa073ff0-064a-4e35-a2ce-0fac33fa1ee6"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("fddf973f-fd90-4546-989f-8ca3c7f469da"));

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("1ecde287-d0b3-48d0-8083-dca24bf5b68f"), new DateTime(2026, 8, 12, 21, 42, 5, 383, DateTimeKind.Utc).AddTicks(5932), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("20df1f1a-381e-4e7a-92ec-49ffa4f343c9"), new DateTime(2026, 8, 12, 21, 42, 5, 383, DateTimeKind.Utc).AddTicks(5934), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("8042dfc4-c69a-41e6-ab7f-bb3096f578c9"), new DateTime(2026, 8, 12, 21, 42, 5, 383, DateTimeKind.Utc).AddTicks(5917), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("909facd3-124c-4f85-8def-5db0d8d7e4a6"), new DateTime(2026, 8, 12, 21, 42, 5, 383, DateTimeKind.Utc).AddTicks(5920), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("a46cba54-3d26-49b9-889b-f41e967175a2"), new DateTime(2026, 8, 12, 21, 42, 5, 383, DateTimeKind.Utc).AddTicks(5922), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("abfb92bb-6e9e-4c31-be9d-45b464121bbe"), new DateTime(2026, 8, 12, 21, 42, 5, 383, DateTimeKind.Utc).AddTicks(5924), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("c64c4bf4-675f-4092-9ea0-a1a7f90aa9f5"), new DateTime(2026, 8, 12, 21, 42, 5, 383, DateTimeKind.Utc).AddTicks(5929), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("def9e009-3812-4deb-89e4-9de11393eab1"), new DateTime(2026, 8, 12, 21, 42, 5, 383, DateTimeKind.Utc).AddTicks(5907), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("ea939395-9f9c-416a-a063-7664b1d34684"), new DateTime(2026, 8, 12, 21, 42, 5, 383, DateTimeKind.Utc).AddTicks(5936), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("f5585533-9b9b-473a-be10-321bde41306f"), new DateTime(2026, 8, 12, 21, 42, 5, 383, DateTimeKind.Utc).AddTicks(5900), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null }
                });
        }
    }
}
