using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkAuthorizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("074af2a4-ac53-4aa5-8c95-1bf4ac9dba5d"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("222d566b-d389-4873-9a89-81c321791d56"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("3e1afabf-873a-4655-8ae6-3713b8f7e3a3"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("41f175d7-a98f-4e78-825a-7410d97471c6"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("449dab03-00fc-41a1-b3be-f146a35981a3"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("4e35ee51-b266-4b38-a0c9-f217071ec1bb"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("98f8641e-e463-4f38-8895-f9f0d69558cc"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("aa82e888-93ce-4c56-93ae-2062e07ad462"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("bf0dc1df-1501-401f-a4ab-312d7ee7b675"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("fea499d4-2ea1-4e19-9bd9-55695310d27e"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("1f0184be-d701-4a35-bb10-cd6cddec4a7a"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("684c117c-fe36-4ffc-85da-01829786f7af"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("820e93c2-e175-47b8-8a52-a458bc9dd428"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("9939fd8c-c3da-4025-8c62-cb060baa3196"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("9f8c490e-77e9-4df2-9a66-8f556d6073ec"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("a87cdcf4-fc71-4b5a-beff-7867d87a5765"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c5b9cc7a-67d5-464d-94a8-e94343ecbb52"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("d3343dc5-d579-452a-8def-101fd2c8d7e5"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("d4662021-c019-45b6-a046-6a6bab3a533b"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("ebb99aed-f011-49d1-bab2-bc6d6fe7185b"));

            migrationBuilder.AddColumn<string>(
                name: "WorkAuthorizations",
                table: "PT_Candidates",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("20deeed5-badb-4291-bded-02ac9c5bbea4"), "Identidad", new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3003), null, null, null, "DNI / NIE / Cédula de identidad", false, true, "Documento de identidad", 2, null, null });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("2ea62b07-467b-47e6-b821-5a98d20bfa2f"), "Fiscal", new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3080), null, null, null, "Documento con número de afiliación a la seguridad social", false, "Nº Seguridad Social", 9, null, null },
                    { new Guid("383eb4c2-1cd4-478a-8361-e46070d38961"), "Formación", new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3073), null, null, null, "Título habilitante o certificación profesional", false, "Titulo / Certificación profesional", 8, null, null },
                    { new Guid("8073659c-2935-4e3a-8e9e-7b0294856d90"), "Salud", new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3052), null, null, null, "Tarjeta sanitaria europea (TSE) o seguro médico privado", false, "Tarjeta sanitaria", 6, null, null },
                    { new Guid("99f1269c-a7af-411b-9a5c-e8cd8b070993"), "Migratorio", new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3010), null, null, null, "Autorización de trabajo en el país de destino", false, "Permiso de trabajo", 3, null, null },
                    { new Guid("9fe9e562-6b05-4456-8e20-1e3b46b32900"), "Fiscal", new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3086), null, null, null, "Justificante de cuenta bancaria a nombre del candidato", false, "Cuenta bancaria (IBAN)", 10, null, null },
                    { new Guid("af9311e2-777f-40d2-88ce-b84d0f5eecb4"), "Habilitación", new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3039), null, null, null, "Permiso de conducir válido", false, "Licencia de conducir", 4, null, null },
                    { new Guid("d81fcc97-b62b-4a14-8360-768dc3fe4972"), "Identidad", new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(2984), null, null, null, "Pasaporte válido y en vigor", false, "Pasaporte", 1, null, null },
                    { new Guid("e70af1b3-927f-4ed4-8a08-68eda2468bea"), "Migratorio", new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3046), null, null, null, "Visado que habilita a trabajar legalmente", false, "Visado de trabajo", 5, null, null },
                    { new Guid("e83e630c-9563-45a3-a406-e96f1b77b139"), "Legal", new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3059), null, null, null, "Certificado de antecedentes penales apostillado", false, "Certificado de antecedentes penales", 7, null, null }
                });

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0355b955-a884-44aa-9b51-9f1731d5a8d1"), new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3918), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("4df34d99-3bf5-45a3-b8f0-180a4bc2f6e8"), new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3904), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("61e1169f-6c35-41bb-814a-22cedd7595bf"), new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3867), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("b05b8006-d298-40a6-b01e-1a899b2d79ee"), new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3924), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("c768dd61-712c-4a44-bd92-84af91e440ad"), new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3861), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("cc0e8924-e4e9-4ce5-8898-28d4655b1a2a"), new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3854), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("ccce338c-bd1d-4ee7-998c-ccd88008d2f2"), new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3834), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("e22a84c0-4479-49f6-a2a7-766fe351d4bc"), new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3936), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("ee3bd06e-e6c1-4c93-a48b-dd35abc65d4f"), new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3874), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("f17c79ae-562e-4fa8-b72b-a87ac3d738f7"), new DateTime(2026, 9, 4, 4, 24, 43, 950, DateTimeKind.Utc).AddTicks(3911), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("20deeed5-badb-4291-bded-02ac9c5bbea4"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("2ea62b07-467b-47e6-b821-5a98d20bfa2f"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("383eb4c2-1cd4-478a-8361-e46070d38961"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("8073659c-2935-4e3a-8e9e-7b0294856d90"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("99f1269c-a7af-411b-9a5c-e8cd8b070993"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("9fe9e562-6b05-4456-8e20-1e3b46b32900"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("af9311e2-777f-40d2-88ce-b84d0f5eecb4"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("d81fcc97-b62b-4a14-8360-768dc3fe4972"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("e70af1b3-927f-4ed4-8a08-68eda2468bea"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("e83e630c-9563-45a3-a406-e96f1b77b139"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("0355b955-a884-44aa-9b51-9f1731d5a8d1"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("4df34d99-3bf5-45a3-b8f0-180a4bc2f6e8"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("61e1169f-6c35-41bb-814a-22cedd7595bf"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("b05b8006-d298-40a6-b01e-1a899b2d79ee"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c768dd61-712c-4a44-bd92-84af91e440ad"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("cc0e8924-e4e9-4ce5-8898-28d4655b1a2a"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("ccce338c-bd1d-4ee7-998c-ccd88008d2f2"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("e22a84c0-4479-49f6-a2a7-766fe351d4bc"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("ee3bd06e-e6c1-4c93-a48b-dd35abc65d4f"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("f17c79ae-562e-4fa8-b72b-a87ac3d738f7"));

            migrationBuilder.DropColumn(
                name: "WorkAuthorizations",
                table: "PT_Candidates");

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("074af2a4-ac53-4aa5-8c95-1bf4ac9dba5d"), "Migratorio", new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8141), null, null, null, "Visado que habilita a trabajar legalmente", false, "Visado de trabajo", 5, null, null },
                    { new Guid("222d566b-d389-4873-9a89-81c321791d56"), "Fiscal", new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8171), null, null, null, "Documento con número de afiliación a la seguridad social", false, "Nº Seguridad Social", 9, null, null },
                    { new Guid("3e1afabf-873a-4655-8ae6-3713b8f7e3a3"), "Legal", new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8159), null, null, null, "Certificado de antecedentes penales apostillado", false, "Certificado de antecedentes penales", 7, null, null }
                });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("41f175d7-a98f-4e78-825a-7410d97471c6"), "Identidad", new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8098), null, null, null, "DNI / NIE / Cédula de identidad", false, true, "Documento de identidad", 2, null, null });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("449dab03-00fc-41a1-b3be-f146a35981a3"), "Formación", new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8165), null, null, null, "Título habilitante o certificación profesional", false, "Titulo / Certificación profesional", 8, null, null },
                    { new Guid("4e35ee51-b266-4b38-a0c9-f217071ec1bb"), "Fiscal", new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8177), null, null, null, "Justificante de cuenta bancaria a nombre del candidato", false, "Cuenta bancaria (IBAN)", 10, null, null },
                    { new Guid("98f8641e-e463-4f38-8895-f9f0d69558cc"), "Habilitación", new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8134), null, null, null, "Permiso de conducir válido", false, "Licencia de conducir", 4, null, null },
                    { new Guid("aa82e888-93ce-4c56-93ae-2062e07ad462"), "Migratorio", new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8128), null, null, null, "Autorización de trabajo en el país de destino", false, "Permiso de trabajo", 3, null, null },
                    { new Guid("bf0dc1df-1501-401f-a4ab-312d7ee7b675"), "Identidad", new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8075), null, null, null, "Pasaporte válido y en vigor", false, "Pasaporte", 1, null, null },
                    { new Guid("fea499d4-2ea1-4e19-9bd9-55695310d27e"), "Salud", new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8147), null, null, null, "Tarjeta sanitaria europea (TSE) o seguro médico privado", false, "Tarjeta sanitaria", 6, null, null }
                });

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("1f0184be-d701-4a35-bb10-cd6cddec4a7a"), new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8925), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("684c117c-fe36-4ffc-85da-01829786f7af"), new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8948), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("820e93c2-e175-47b8-8a52-a458bc9dd428"), new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8851), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("9939fd8c-c3da-4025-8c62-cb060baa3196"), new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8844), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("9f8c490e-77e9-4df2-9a66-8f556d6073ec"), new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8833), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("a87cdcf4-fc71-4b5a-beff-7867d87a5765"), new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8954), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("c5b9cc7a-67d5-464d-94a8-e94343ecbb52"), new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8857), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("d3343dc5-d579-452a-8def-101fd2c8d7e5"), new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8868), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("d4662021-c019-45b6-a046-6a6bab3a533b"), new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8938), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("ebb99aed-f011-49d1-bab2-bc6d6fe7185b"), new DateTime(2026, 9, 4, 3, 12, 53, 313, DateTimeKind.Utc).AddTicks(8932), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null }
                });
        }
    }
}
