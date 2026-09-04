using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddHasTransport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("0d3783fa-0667-4eea-9fbe-87e7946e9bd0"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("0e2bbc0d-0127-4b2c-9145-8b0aa4256dae"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("0ea8a3d8-02a6-4224-bf31-e74f2adb30be"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("3402ca6a-8da1-4e7a-be98-40c3b744ef54"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("4d8f87ad-bb64-46a7-9e8a-3ffd5e273319"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("85b7ec75-18a3-40a1-a397-6eb9d097a0bb"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("927563cf-44a5-43fe-9fde-513ac20fde22"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("ac73637c-18b9-4956-91d2-b9fe847eee86"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("b773d6eb-37d1-4d8e-942f-cd2b11ede05e"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("bb942f7c-9b35-497a-aa79-82567793d345"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("10a8f0e3-7d9f-4c34-80b6-4138695988f7"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("1d6c9d3b-8cca-4c18-ad98-09dd90af2ef2"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("3df52b0a-e0f1-4fa5-a922-17fd80b4f2e4"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("3e69a59c-251d-441c-91ce-c8786f73e188"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("7cd7673d-c748-4f8e-950f-27c023529c2c"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("922edf5c-4182-4c01-a9b9-662566887d36"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("9fc2c42a-fe0f-4070-9087-e40d6df4e5da"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c6455f9d-0011-4021-b0ff-6cf8153dee8a"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("d36faf65-ca5e-4ee3-96f1-0513749dbb61"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("e610cd9d-28da-4b8c-a7e1-2209b9809e17"));

            migrationBuilder.AddColumn<bool>(
                name: "HasTransport",
                table: "PT_Candidates",
                type: "tinyint(1)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "HasTransport",
                table: "PT_Candidates");

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0d3783fa-0667-4eea-9fbe-87e7946e9bd0"), "Habilitación", new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(7725), null, null, null, "Permiso de conducir válido", false, "Licencia de conducir", 4, null, null },
                    { new Guid("0e2bbc0d-0127-4b2c-9145-8b0aa4256dae"), "Salud", new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(7735), null, null, null, "Tarjeta sanitaria europea (TSE) o seguro médico privado", false, "Tarjeta sanitaria", 6, null, null },
                    { new Guid("0ea8a3d8-02a6-4224-bf31-e74f2adb30be"), "Formación", new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(7763), null, null, null, "Título habilitante o certificación profesional", false, "Titulo / Certificación profesional", 8, null, null },
                    { new Guid("3402ca6a-8da1-4e7a-be98-40c3b744ef54"), "Legal", new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(7755), null, null, null, "Certificado de antecedentes penales apostillado", false, "Certificado de antecedentes penales", 7, null, null },
                    { new Guid("4d8f87ad-bb64-46a7-9e8a-3ffd5e273319"), "Migratorio", new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(7707), null, null, null, "Autorización de trabajo en el país de destino", false, "Permiso de trabajo", 3, null, null },
                    { new Guid("85b7ec75-18a3-40a1-a397-6eb9d097a0bb"), "Identidad", new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(7689), null, null, null, "Pasaporte válido y en vigor", false, "Pasaporte", 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("927563cf-44a5-43fe-9fde-513ac20fde22"), "Identidad", new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(7701), null, null, null, "DNI / NIE / Cédula de identidad", false, true, "Documento de identidad", 2, null, null });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("ac73637c-18b9-4956-91d2-b9fe847eee86"), "Migratorio", new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(7730), null, null, null, "Visado que habilita a trabajar legalmente", false, "Visado de trabajo", 5, null, null },
                    { new Guid("b773d6eb-37d1-4d8e-942f-cd2b11ede05e"), "Fiscal", new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(7772), null, null, null, "Justificante de cuenta bancaria a nombre del candidato", false, "Cuenta bancaria (IBAN)", 10, null, null },
                    { new Guid("bb942f7c-9b35-497a-aa79-82567793d345"), "Fiscal", new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(7768), null, null, null, "Documento con número de afiliación a la seguridad social", false, "Nº Seguridad Social", 9, null, null }
                });

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("10a8f0e3-7d9f-4c34-80b6-4138695988f7"), new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(8146), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("1d6c9d3b-8cca-4c18-ad98-09dd90af2ef2"), new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(8177), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("3df52b0a-e0f1-4fa5-a922-17fd80b4f2e4"), new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(8158), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("3e69a59c-251d-441c-91ce-c8786f73e188"), new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(8141), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("7cd7673d-c748-4f8e-950f-27c023529c2c"), new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(8150), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("922edf5c-4182-4c01-a9b9-662566887d36"), new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(8125), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("9fc2c42a-fe0f-4070-9087-e40d6df4e5da"), new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(8170), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("c6455f9d-0011-4021-b0ff-6cf8153dee8a"), new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(8162), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("d36faf65-ca5e-4ee3-96f1-0513749dbb61"), new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(8136), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("e610cd9d-28da-4b8c-a7e1-2209b9809e17"), new DateTime(2026, 9, 4, 2, 16, 17, 685, DateTimeKind.Utc).AddTicks(8166), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null }
                });
        }
    }
}
