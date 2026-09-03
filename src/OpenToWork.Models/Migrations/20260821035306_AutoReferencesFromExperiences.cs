using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class AutoReferencesFromExperiences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
