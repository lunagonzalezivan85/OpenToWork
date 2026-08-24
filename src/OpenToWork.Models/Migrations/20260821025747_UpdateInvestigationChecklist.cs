using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvestigationChecklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<bool>(
                name: "IsCustom",
                table: "PT_InvestigationChecklists",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "PT_InvestigationChecklists",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "IsCustom",
                table: "PT_InvestigationChecklists");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "PT_InvestigationChecklists");

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
        }
    }
}
