using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidateRecruitmentPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("25180b4f-184b-40ba-b8ed-a08ff084e672"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("2c86c03d-6eec-4482-9c53-e57cbededd54"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("326559c9-b543-449b-9bde-de6b446c32ac"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("43e9b14d-f3dc-4f4e-9750-26f67e0cda74"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("469cabe1-d7f8-481f-b3e3-aee576b72599"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("54b2c553-378a-4355-870d-29486ac3f8b1"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("949a8eaa-217d-451e-8f2d-ae6ead7b49d7"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("d40c69f0-526b-4582-9094-480e0aae409a"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("df4185ae-f1c8-45d5-86b6-0fc36acdea20"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("ec44d156-87da-4e22-b1e7-916f48eb628d"));

            migrationBuilder.CreateTable(
                name: "PT_CandidateRecruitmentPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CandidateRecruitmentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PreferredWorkShift = table.Column<int>(type: "int", nullable: true),
                    AcceptedContractType = table.Column<int>(type: "int", nullable: true),
                    AvailabilityToJoin = table.Column<int>(type: "int", nullable: true),
                    ExpectedSalary = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    AvailableDays = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvailableWeekends = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    AvailableHolidays = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    AvailableSchedule = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_PT_CandidateRecruitmentPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_CandidateRecruitmentPreferences_PT_CandidateRecruitments_~",
                        column: x => x.PT_CandidateRecruitmentId,
                        principalTable: "PT_CandidateRecruitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0dcde868-ff56-4f4a-b4e8-ff266e75d1a0"), new DateTime(2026, 9, 4, 0, 12, 59, 774, DateTimeKind.Utc).AddTicks(1834), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("0df381d0-d9eb-4461-8d8e-b1d85f47b38c"), new DateTime(2026, 9, 4, 0, 12, 59, 774, DateTimeKind.Utc).AddTicks(1844), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("151f6214-f52a-4c14-b0d1-ef6d78277af9"), new DateTime(2026, 9, 4, 0, 12, 59, 774, DateTimeKind.Utc).AddTicks(1831), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("2418b77e-5068-4ec7-9538-6afb4b6c6a41"), new DateTime(2026, 9, 4, 0, 12, 59, 774, DateTimeKind.Utc).AddTicks(1826), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("2c9781ff-e88b-4f22-80ee-ac3888766afb"), new DateTime(2026, 9, 4, 0, 12, 59, 774, DateTimeKind.Utc).AddTicks(1851), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("4b2795a9-e872-4442-8b4f-516ee756e398"), new DateTime(2026, 9, 4, 0, 12, 59, 774, DateTimeKind.Utc).AddTicks(1856), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("68a02876-e5a2-4af5-9a13-e18f34cc473d"), new DateTime(2026, 9, 4, 0, 12, 59, 774, DateTimeKind.Utc).AddTicks(1841), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("9ed63139-7ce2-4386-854e-eefc83b711e9"), new DateTime(2026, 9, 4, 0, 12, 59, 774, DateTimeKind.Utc).AddTicks(1859), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("b422f2aa-da01-4139-a1ad-f9de855027a4"), new DateTime(2026, 9, 4, 0, 12, 59, 774, DateTimeKind.Utc).AddTicks(1848), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("c21ff05d-a53e-4dbf-84fe-d2215e0ed69b"), new DateTime(2026, 9, 4, 0, 12, 59, 774, DateTimeKind.Utc).AddTicks(1815), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRecruitmentPreferences_IsCompleted_IsDeleted",
                table: "PT_CandidateRecruitmentPreferences",
                columns: new[] { "IsCompleted", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRecruitmentPreferences_PT_CandidateRecruitmentId",
                table: "PT_CandidateRecruitmentPreferences",
                column: "PT_CandidateRecruitmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRecruitmentPreferences_PT_CandidateRecruitmentId~",
                table: "PT_CandidateRecruitmentPreferences",
                columns: new[] { "PT_CandidateRecruitmentId", "IsDeleted" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_CandidateRecruitmentPreferences");

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("0dcde868-ff56-4f4a-b4e8-ff266e75d1a0"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("0df381d0-d9eb-4461-8d8e-b1d85f47b38c"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("151f6214-f52a-4c14-b0d1-ef6d78277af9"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("2418b77e-5068-4ec7-9538-6afb4b6c6a41"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("2c9781ff-e88b-4f22-80ee-ac3888766afb"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("4b2795a9-e872-4442-8b4f-516ee756e398"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("68a02876-e5a2-4af5-9a13-e18f34cc473d"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("9ed63139-7ce2-4386-854e-eefc83b711e9"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("b422f2aa-da01-4139-a1ad-f9de855027a4"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c21ff05d-a53e-4dbf-84fe-d2215e0ed69b"));

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("25180b4f-184b-40ba-b8ed-a08ff084e672"), new DateTime(2026, 8, 21, 4, 43, 40, 256, DateTimeKind.Utc).AddTicks(5379), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("2c86c03d-6eec-4482-9c53-e57cbededd54"), new DateTime(2026, 8, 21, 4, 43, 40, 256, DateTimeKind.Utc).AddTicks(5384), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("326559c9-b543-449b-9bde-de6b446c32ac"), new DateTime(2026, 8, 21, 4, 43, 40, 256, DateTimeKind.Utc).AddTicks(5411), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("43e9b14d-f3dc-4f4e-9750-26f67e0cda74"), new DateTime(2026, 8, 21, 4, 43, 40, 256, DateTimeKind.Utc).AddTicks(5413), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("469cabe1-d7f8-481f-b3e3-aee576b72599"), new DateTime(2026, 8, 21, 4, 43, 40, 256, DateTimeKind.Utc).AddTicks(5409), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("54b2c553-378a-4355-870d-29486ac3f8b1"), new DateTime(2026, 8, 21, 4, 43, 40, 256, DateTimeKind.Utc).AddTicks(5417), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("949a8eaa-217d-451e-8f2d-ae6ead7b49d7"), new DateTime(2026, 8, 21, 4, 43, 40, 256, DateTimeKind.Utc).AddTicks(5376), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("d40c69f0-526b-4582-9094-480e0aae409a"), new DateTime(2026, 8, 21, 4, 43, 40, 256, DateTimeKind.Utc).AddTicks(5382), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("df4185ae-f1c8-45d5-86b6-0fc36acdea20"), new DateTime(2026, 8, 21, 4, 43, 40, 256, DateTimeKind.Utc).AddTicks(5400), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("ec44d156-87da-4e22-b1e7-916f48eb628d"), new DateTime(2026, 8, 21, 4, 43, 40, 256, DateTimeKind.Utc).AddTicks(5363), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null }
                });
        }
    }
}
