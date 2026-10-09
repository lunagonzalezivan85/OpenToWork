using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class VacancyReferenceCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("3261041c-7843-47cc-a730-72bb21b65802"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("39f1405f-0aad-4601-879c-654936dc91cc"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("6b4c276c-7e63-4cb1-8ff7-255d434b8e25"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("72e803aa-ca84-4186-9e06-e7b124b98e83"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("7e89ab06-d4c9-4db8-a325-08193efc7997"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("a3f6b0b3-fa55-4b03-8ea7-d1afe25f1441"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("aa87f26d-009a-41d5-9708-4dc5c6f156b2"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("b6c8f71b-822c-47de-9746-78f36a9a622e"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("ce86d023-c34b-4533-b48b-9034872fa935"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("d430fd72-180c-4e19-b876-32e4aa034a58"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("0343e51a-5b77-4925-9f26-3552013ec2fe"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("11a07ac6-bd8d-41b0-ac26-64c1fc745286"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("4c748082-07ad-49bd-a6a5-8b81c92c5a20"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("5d766ce4-7cb6-490a-8c00-3e56ec974ed8"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("7b7db369-f824-4d50-b586-38cf3c3f9e30"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("96a91616-c67a-4c07-bdd5-db92d98acc43"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("aaf2de82-4a3f-466a-ac7e-118efad5da3a"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c13f0835-adfc-4355-b3eb-d76ed15acd95"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c37e1c3c-372c-4b4d-adcf-8fe2cf465ff1"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c499d128-4c5f-4634-8656-2b9b30219ae4"));

            migrationBuilder.AddColumn<string>(
                name: "ReferenceCode",
                table: "PT_Vacancies",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(2028));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(2064));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(2072));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b1111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(2083));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b2222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(2090));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b3333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(2100));

            // Los seeds de HasData regeneran los GUIDs en cada scaffold; si la base tiene
            // filas sembradas por una migracion distinta, los DeleteData de arriba no las
            // tocan y los InsertData de abajo chocan con el indice unico por nombre.
            // Son tablas catalogo: se vacian y se reinserta el set completo.
            migrationBuilder.Sql("DELETE FROM SY_DocumentTypes");
            migrationBuilder.Sql("DELETE FROM SY_WizardSteps");

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("33b59a4e-f1e4-4a57-864e-824947a6faba"), "Legal", new DateTime(2026, 10, 5, 4, 28, 38, 150, DateTimeKind.Utc).AddTicks(8013), null, null, null, "Certificado de antecedentes penales apostillado", false, "Certificado de antecedentes penales", 7, null, null },
                    { new Guid("602fb1ce-123a-4bde-b519-fa1223d30e75"), "Migratorio", new DateTime(2026, 10, 5, 4, 28, 38, 150, DateTimeKind.Utc).AddTicks(8002), null, null, null, "Visado que habilita a trabajar legalmente", false, "Visado de trabajo", 5, null, null },
                    { new Guid("6d5c560b-2ac8-4c2b-9b5d-2662b7d6e159"), "Migratorio", new DateTime(2026, 10, 5, 4, 28, 38, 150, DateTimeKind.Utc).AddTicks(7994), null, null, null, "Autorización de trabajo en el país de destino", false, "Permiso de trabajo", 3, null, null },
                    { new Guid("802cad7a-3a71-40f4-81db-5bfb8cf3582a"), "Fiscal", new DateTime(2026, 10, 5, 4, 28, 38, 150, DateTimeKind.Utc).AddTicks(8025), null, null, null, "Justificante de cuenta bancaria a nombre del candidato", false, "Cuenta bancaria (IBAN)", 10, null, null },
                    { new Guid("85a1591f-1183-4fbc-994d-18a86870468a"), "Fiscal", new DateTime(2026, 10, 5, 4, 28, 38, 150, DateTimeKind.Utc).AddTicks(8021), null, null, null, "Documento con número de afiliación a la seguridad social", false, "Nº Seguridad Social", 9, null, null },
                    { new Guid("98367b41-1e12-4f58-b059-ce87a4e48b1e"), "Formación", new DateTime(2026, 10, 5, 4, 28, 38, 150, DateTimeKind.Utc).AddTicks(8017), null, null, null, "Título habilitante o certificación profesional", false, "Titulo / Certificación profesional", 8, null, null },
                    { new Guid("c7d6234b-f8eb-4bd9-a22f-9489c916771e"), "Habilitación", new DateTime(2026, 10, 5, 4, 28, 38, 150, DateTimeKind.Utc).AddTicks(7998), null, null, null, "Permiso de conducir válido", false, "Licencia de conducir", 4, null, null },
                    { new Guid("db954e18-c7ce-4a73-84d2-33787d1e8e36"), "Salud", new DateTime(2026, 10, 5, 4, 28, 38, 150, DateTimeKind.Utc).AddTicks(8005), null, null, null, "Tarjeta sanitaria europea (TSE) o seguro médico privado", false, "Tarjeta sanitaria", 6, null, null }
                });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("f08fb093-7ded-472d-8c1f-de3dc0a797d1"), "Identidad", new DateTime(2026, 10, 5, 4, 28, 38, 150, DateTimeKind.Utc).AddTicks(7974), null, null, null, "DNI / NIE / Cédula de identidad", false, true, "Documento de identidad", 2, null, null });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("fd58b1da-ee9a-4e63-9a08-72cea85966d3"), "Identidad", new DateTime(2026, 10, 5, 4, 28, 38, 150, DateTimeKind.Utc).AddTicks(7931), null, null, null, "Pasaporte válido y en vigor", false, "Pasaporte", 1, null, null });

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0beb3eb6-b4ce-4099-859b-416a1833ad11"), new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(1755), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("109f745b-5e0a-48dd-a2dc-5d355dce22fb"), new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(1650), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("233d66a7-6569-4296-bb12-705dca73f8ef"), new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(1750), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("36a44f79-0054-4dad-982c-f7dad7216d90"), new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(1675), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("3e3a9c60-6eaf-42a5-8f4e-a8faa13a805b"), new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(1771), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("4b14d717-77c1-44f2-9814-c6b32814a087"), new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(1671), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("7d75caa6-bb99-4858-b7cc-852b41a11a92"), new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(1664), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("a84c60d6-bf9f-4615-952e-25aa032a8260"), new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(1760), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("aef2c74b-7183-4733-ae71-a3d6ac8fe097"), new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(1720), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("c6847690-cf5d-4143-8a32-f74610470251"), new DateTime(2026, 10, 5, 4, 28, 38, 161, DateTimeKind.Utc).AddTicks(1776), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null }
                });

            // Backfill antes del indice unico: el default "" duplicado en filas existentes
            // violaria la unicidad. UUID() genera un valor distinto por fila.
            migrationBuilder.Sql(
                "UPDATE PT_Vacancies SET ReferenceCode = CONCAT('TD-', UPPER(SUBSTRING(REPLACE(UUID(), '-', ''), 1, 8))) WHERE ReferenceCode = ''");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Vacancies_ReferenceCode",
                table: "PT_Vacancies",
                column: "ReferenceCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PT_Vacancies_ReferenceCode",
                table: "PT_Vacancies");

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("33b59a4e-f1e4-4a57-864e-824947a6faba"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("602fb1ce-123a-4bde-b519-fa1223d30e75"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("6d5c560b-2ac8-4c2b-9b5d-2662b7d6e159"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("802cad7a-3a71-40f4-81db-5bfb8cf3582a"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("85a1591f-1183-4fbc-994d-18a86870468a"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("98367b41-1e12-4f58-b059-ce87a4e48b1e"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("c7d6234b-f8eb-4bd9-a22f-9489c916771e"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("db954e18-c7ce-4a73-84d2-33787d1e8e36"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("f08fb093-7ded-472d-8c1f-de3dc0a797d1"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("fd58b1da-ee9a-4e63-9a08-72cea85966d3"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("0beb3eb6-b4ce-4099-859b-416a1833ad11"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("109f745b-5e0a-48dd-a2dc-5d355dce22fb"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("233d66a7-6569-4296-bb12-705dca73f8ef"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("36a44f79-0054-4dad-982c-f7dad7216d90"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("3e3a9c60-6eaf-42a5-8f4e-a8faa13a805b"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("4b14d717-77c1-44f2-9814-c6b32814a087"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("7d75caa6-bb99-4858-b7cc-852b41a11a92"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("a84c60d6-bf9f-4615-952e-25aa032a8260"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("aef2c74b-7183-4733-ae71-a3d6ac8fe097"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c6847690-cf5d-4143-8a32-f74610470251"));

            migrationBuilder.DropColumn(
                name: "ReferenceCode",
                table: "PT_Vacancies");

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(6316));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(6369));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(6378));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b1111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(6384));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b2222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(6390));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b3333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(6397));

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("3261041c-7843-47cc-a730-72bb21b65802"), "Fiscal", new DateTime(2026, 10, 4, 19, 15, 10, 348, DateTimeKind.Utc).AddTicks(9977), null, null, null, "Justificante de cuenta bancaria a nombre del candidato", false, "Cuenta bancaria (IBAN)", 10, null, null });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("39f1405f-0aad-4601-879c-654936dc91cc"), "Identidad", new DateTime(2026, 10, 4, 19, 15, 10, 348, DateTimeKind.Utc).AddTicks(9858), null, null, null, "DNI / NIE / Cédula de identidad", false, true, "Documento de identidad", 2, null, null });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("6b4c276c-7e63-4cb1-8ff7-255d434b8e25"), "Habilitación", new DateTime(2026, 10, 4, 19, 15, 10, 348, DateTimeKind.Utc).AddTicks(9870), null, null, null, "Permiso de conducir válido", false, "Licencia de conducir", 4, null, null },
                    { new Guid("72e803aa-ca84-4186-9e06-e7b124b98e83"), "Legal", new DateTime(2026, 10, 4, 19, 15, 10, 348, DateTimeKind.Utc).AddTicks(9907), null, null, null, "Certificado de antecedentes penales apostillado", false, "Certificado de antecedentes penales", 7, null, null },
                    { new Guid("7e89ab06-d4c9-4db8-a325-08193efc7997"), "Salud", new DateTime(2026, 10, 4, 19, 15, 10, 348, DateTimeKind.Utc).AddTicks(9902), null, null, null, "Tarjeta sanitaria europea (TSE) o seguro médico privado", false, "Tarjeta sanitaria", 6, null, null },
                    { new Guid("a3f6b0b3-fa55-4b03-8ea7-d1afe25f1441"), "Formación", new DateTime(2026, 10, 4, 19, 15, 10, 348, DateTimeKind.Utc).AddTicks(9960), null, null, null, "Título habilitante o certificación profesional", false, "Titulo / Certificación profesional", 8, null, null },
                    { new Guid("aa87f26d-009a-41d5-9708-4dc5c6f156b2"), "Identidad", new DateTime(2026, 10, 4, 19, 15, 10, 348, DateTimeKind.Utc).AddTicks(9846), null, null, null, "Pasaporte válido y en vigor", false, "Pasaporte", 1, null, null },
                    { new Guid("b6c8f71b-822c-47de-9746-78f36a9a622e"), "Fiscal", new DateTime(2026, 10, 4, 19, 15, 10, 348, DateTimeKind.Utc).AddTicks(9972), null, null, null, "Documento con número de afiliación a la seguridad social", false, "Nº Seguridad Social", 9, null, null },
                    { new Guid("ce86d023-c34b-4533-b48b-9034872fa935"), "Migratorio", new DateTime(2026, 10, 4, 19, 15, 10, 348, DateTimeKind.Utc).AddTicks(9865), null, null, null, "Autorización de trabajo en el país de destino", false, "Permiso de trabajo", 3, null, null },
                    { new Guid("d430fd72-180c-4e19-b876-32e4aa034a58"), "Migratorio", new DateTime(2026, 10, 4, 19, 15, 10, 348, DateTimeKind.Utc).AddTicks(9897), null, null, null, "Visado que habilita a trabajar legalmente", false, "Visado de trabajo", 5, null, null }
                });

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0343e51a-5b77-4925-9f26-3552013ec2fe"), new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(5837), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("11a07ac6-bd8d-41b0-ac26-64c1fc745286"), new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(5888), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("4c748082-07ad-49bd-a6a5-8b81c92c5a20"), new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(5916), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("5d766ce4-7cb6-490a-8c00-3e56ec974ed8"), new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(5894), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("7b7db369-f824-4d50-b586-38cf3c3f9e30"), new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(5856), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("96a91616-c67a-4c07-bdd5-db92d98acc43"), new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(5882), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("aaf2de82-4a3f-466a-ac7e-118efad5da3a"), new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(5921), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("c13f0835-adfc-4355-b3eb-d76ed15acd95"), new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(5899), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("c37e1c3c-372c-4b4d-adcf-8fe2cf465ff1"), new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(5910), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("c499d128-4c5f-4634-8656-2b9b30219ae4"), new DateTime(2026, 10, 4, 19, 15, 10, 372, DateTimeKind.Utc).AddTicks(5927), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null }
                });
        }
    }
}
