using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class CulturalInterviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<int>(
                name: "Recommendation",
                table: "PT_TechnicalEvaluations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "PT_TechnicalEvaluations",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Recommendation",
                table: "PT_TechnicalEvaluations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "PT_TechnicalEvaluations");

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
        }
    }
}
