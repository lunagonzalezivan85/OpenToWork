using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class InvestigationTrackingAndReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("08132a46-e9b6-4175-a980-07f67716e4d1"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("3619dc90-8866-4edf-ad9f-7115fefed406"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("438e2667-6985-4766-b36c-926e1414a14d"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("6116dd93-665a-4aef-8979-e13094495182"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c6fa5b57-b342-41d5-b61a-b40d7c557387"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("ca2aee38-8040-4f93-af00-924eddd5c080"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("d650743d-6deb-44e1-aaaf-5b20853d3047"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("e76a070a-3b47-40c0-bb85-f5b4b0cb195e"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("eaf58b6f-f7f8-4b4b-9833-6f2b10ae3d37"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("eb1c6b4f-0bd1-4852-9e58-b9b11216c979"));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "PT_InvestigationChecklists",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PT_ReferenceChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_InvestigationChecklistId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CompanyName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactPhone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactEmail = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CalledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_PT_ReferenceChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_ReferenceChecks_PT_InvestigationChecklists_PT_Investigati~",
                        column: x => x.PT_InvestigationChecklistId,
                        principalTable: "PT_InvestigationChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("3675093c-0b60-4743-9432-7294e80f72fd"), new DateTime(2026, 8, 21, 3, 30, 46, 407, DateTimeKind.Utc).AddTicks(188), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("40a43747-d6b5-41df-94df-a8c6a0c872b3"), new DateTime(2026, 8, 21, 3, 30, 46, 407, DateTimeKind.Utc).AddTicks(160), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("40a74b54-3f7b-451d-9bd2-016fe09df0ea"), new DateTime(2026, 8, 21, 3, 30, 46, 407, DateTimeKind.Utc).AddTicks(191), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("5a92d4a9-30f3-477a-92ed-4cc9995f2d20"), new DateTime(2026, 8, 21, 3, 30, 46, 407, DateTimeKind.Utc).AddTicks(193), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("68b1dbf6-c999-4689-85db-08d51ac19017"), new DateTime(2026, 8, 21, 3, 30, 46, 407, DateTimeKind.Utc).AddTicks(157), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("9dc8598d-066f-4236-81b4-6caaf32953e8"), new DateTime(2026, 8, 21, 3, 30, 46, 407, DateTimeKind.Utc).AddTicks(173), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("b81506d7-98fa-4454-a8f1-11aff9c5bc91"), new DateTime(2026, 8, 21, 3, 30, 46, 407, DateTimeKind.Utc).AddTicks(148), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("cd8d5468-ad18-433d-b634-b7324f792216"), new DateTime(2026, 8, 21, 3, 30, 46, 407, DateTimeKind.Utc).AddTicks(176), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("d4ead945-2045-4d8d-b265-e40be485ebe2"), new DateTime(2026, 8, 21, 3, 30, 46, 407, DateTimeKind.Utc).AddTicks(179), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("f047b136-b288-44db-a8fd-238763836b0e"), new DateTime(2026, 8, 21, 3, 30, 46, 407, DateTimeKind.Utc).AddTicks(182), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PT_ReferenceChecks_PT_InvestigationChecklistId_IsDeleted",
                table: "PT_ReferenceChecks",
                columns: new[] { "PT_InvestigationChecklistId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_ReferenceChecks");

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("3675093c-0b60-4743-9432-7294e80f72fd"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("40a43747-d6b5-41df-94df-a8c6a0c872b3"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("40a74b54-3f7b-451d-9bd2-016fe09df0ea"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("5a92d4a9-30f3-477a-92ed-4cc9995f2d20"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("68b1dbf6-c999-4689-85db-08d51ac19017"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("9dc8598d-066f-4236-81b4-6caaf32953e8"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("b81506d7-98fa-4454-a8f1-11aff9c5bc91"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("cd8d5468-ad18-433d-b634-b7324f792216"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("d4ead945-2045-4d8d-b265-e40be485ebe2"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("f047b136-b288-44db-a8fd-238763836b0e"));

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "PT_InvestigationChecklists");

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("08132a46-e9b6-4175-a980-07f67716e4d1"), new DateTime(2026, 8, 21, 2, 57, 46, 944, DateTimeKind.Utc).AddTicks(5782), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("3619dc90-8866-4edf-ad9f-7115fefed406"), new DateTime(2026, 8, 21, 2, 57, 46, 944, DateTimeKind.Utc).AddTicks(5777), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("438e2667-6985-4766-b36c-926e1414a14d"), new DateTime(2026, 8, 21, 2, 57, 46, 944, DateTimeKind.Utc).AddTicks(5769), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("6116dd93-665a-4aef-8979-e13094495182"), new DateTime(2026, 8, 21, 2, 57, 46, 944, DateTimeKind.Utc).AddTicks(5784), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("c6fa5b57-b342-41d5-b61a-b40d7c557387"), new DateTime(2026, 8, 21, 2, 57, 46, 944, DateTimeKind.Utc).AddTicks(5772), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("ca2aee38-8040-4f93-af00-924eddd5c080"), new DateTime(2026, 8, 21, 2, 57, 46, 944, DateTimeKind.Utc).AddTicks(5792), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("d650743d-6deb-44e1-aaaf-5b20853d3047"), new DateTime(2026, 8, 21, 2, 57, 46, 944, DateTimeKind.Utc).AddTicks(5775), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("e76a070a-3b47-40c0-bb85-f5b4b0cb195e"), new DateTime(2026, 8, 21, 2, 57, 46, 944, DateTimeKind.Utc).AddTicks(5755), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("eaf58b6f-f7f8-4b4b-9833-6f2b10ae3d37"), new DateTime(2026, 8, 21, 2, 57, 46, 944, DateTimeKind.Utc).AddTicks(5788), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("eb1c6b4f-0bd1-4852-9e58-b9b11216c979"), new DateTime(2026, 8, 21, 2, 57, 46, 944, DateTimeKind.Utc).AddTicks(5786), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null }
                });
        }
    }
}
