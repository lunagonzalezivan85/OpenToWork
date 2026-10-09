using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class CandidateRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("0265ab2b-380a-4c7c-b627-4f7ee3092918"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("19af7b68-b44c-4cc9-9d56-7a134ecdce2a"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("3a47dcf0-b5ee-497b-b292-d2390f2277cd"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("83e1e972-7348-45f9-8286-109913ed8a4b"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("93b08ff6-c120-499f-9ef2-eac354e5ebda"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("b1360198-1d20-4298-8769-ebc849a382cf"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("bd6ce6b8-bfcc-496a-9079-bd0708809a01"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("d04b73ac-5ee0-4385-90f3-3bced2df54c7"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("d855c678-8bbf-411f-a9be-378cf9d9db3d"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("fe9e655a-cff4-40ef-9fd3-47ad81054fe8"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("3bdecb05-5e2d-4c66-8197-63374b9fc436"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("6213527d-790a-4721-996f-934ef2623b36"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("74fc4210-845c-4968-b0d7-d8b1439cff67"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("7f2abf4f-4a0d-416f-950a-5e362b4b2414"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("8a636679-dfe3-4bf0-9e32-f96535bf4822"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("93ccab10-d672-417f-ac0d-6914b0d70287"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("a1aa9c66-160b-462a-9af8-e3256895407b"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("a228de5a-2903-4953-93cf-bad0a824eb02"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("c52991fc-0d49-4b5e-86b4-b26d41d8d588"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("ebc9f773-da4e-4c22-8fcc-57555b3d5d13"));

            migrationBuilder.CreateTable(
                name: "PT_CandidateRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CompanyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RequestedByUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CandidateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_VacancyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReviewedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_PT_CandidateRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_CandidateRequests_PT_Candidates_PT_CandidateId",
                        column: x => x.PT_CandidateId,
                        principalTable: "PT_Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_CandidateRequests_PT_Companies_PT_CompanyId",
                        column: x => x.PT_CompanyId,
                        principalTable: "PT_Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_CandidateRequests_PT_Vacancies_PT_VacancyId",
                        column: x => x.PT_VacancyId,
                        principalTable: "PT_Vacancies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PT_CandidateRequests_SC_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6381));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6412));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6416));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b1111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6420));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b2222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6425));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b3333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6430));

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("159074f3-55b4-4c90-bb19-f0d34d3cceab"), "Migratorio", new DateTime(2026, 10, 9, 21, 31, 4, 134, DateTimeKind.Utc).AddTicks(5514), null, null, null, "Autorización de trabajo en el país de destino", false, "Permiso de trabajo", 3, null, null },
                    { new Guid("2892b646-5075-4fa4-8f34-aa4c4d65f454"), "Habilitación", new DateTime(2026, 10, 9, 21, 31, 4, 134, DateTimeKind.Utc).AddTicks(5527), null, null, null, "Permiso de conducir válido", false, "Licencia de conducir", 4, null, null },
                    { new Guid("32b1a3e3-5844-4c55-a1ad-d599d41788ed"), "Fiscal", new DateTime(2026, 10, 9, 21, 31, 4, 134, DateTimeKind.Utc).AddTicks(5551), null, null, null, "Justificante de cuenta bancaria a nombre del candidato", false, "Cuenta bancaria (IBAN)", 10, null, null },
                    { new Guid("5e8a639b-da4b-4e19-9e32-62dc45d32afa"), "Legal", new DateTime(2026, 10, 9, 21, 31, 4, 134, DateTimeKind.Utc).AddTicks(5542), null, null, null, "Certificado de antecedentes penales apostillado", false, "Certificado de antecedentes penales", 7, null, null },
                    { new Guid("7567f361-a43b-4070-87d2-405dbb3fac0e"), "Formación", new DateTime(2026, 10, 9, 21, 31, 4, 134, DateTimeKind.Utc).AddTicks(5547), null, null, null, "Título habilitante o certificación profesional", false, "Titulo / Certificación profesional", 8, null, null },
                    { new Guid("85adb332-410c-46c6-afed-99fb9f76cebb"), "Identidad", new DateTime(2026, 10, 9, 21, 31, 4, 134, DateTimeKind.Utc).AddTicks(5504), null, null, null, "Pasaporte válido y en vigor", false, "Pasaporte", 1, null, null },
                    { new Guid("90996e47-4509-45d4-9a5b-60a4b16edbd5"), "Salud", new DateTime(2026, 10, 9, 21, 31, 4, 134, DateTimeKind.Utc).AddTicks(5540), null, null, null, "Tarjeta sanitaria europea (TSE) o seguro médico privado", false, "Tarjeta sanitaria", 6, null, null }
                });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("bd375efd-e3d6-43ff-9cf5-a6b753f279c0"), "Identidad", new DateTime(2026, 10, 9, 21, 31, 4, 134, DateTimeKind.Utc).AddTicks(5512), null, null, null, "DNI / NIE / Cédula de identidad", false, true, "Documento de identidad", 2, null, null });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("c2f862e8-a8fd-4118-817d-cdce5db8793f"), "Fiscal", new DateTime(2026, 10, 9, 21, 31, 4, 134, DateTimeKind.Utc).AddTicks(5549), null, null, null, "Documento con número de afiliación a la seguridad social", false, "Nº Seguridad Social", 9, null, null },
                    { new Guid("dac655b4-0892-4403-97c7-96d67bc58c34"), "Migratorio", new DateTime(2026, 10, 9, 21, 31, 4, 134, DateTimeKind.Utc).AddTicks(5537), null, null, null, "Visado que habilita a trabajar legalmente", false, "Visado de trabajo", 5, null, null }
                });

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0bcf4edd-8456-47fe-bc2b-5aee1f6eda8b"), new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(5932), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("3b7db822-ab9f-487f-91dd-aa4c6d56bc8c"), new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6125), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("5d23de2a-52c3-42cb-a3c6-6df473800269"), new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6129), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("606547cd-5463-47f6-8f38-5f820c2066d4"), new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6000), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("9956deb0-7a10-4e24-961a-98c9f442fe74"), new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(5997), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("a024d4c6-995c-4488-934a-542f654f9c95"), new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6002), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("b6742fdb-b43b-44ce-bc84-5217997a8398"), new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6122), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("cf89e144-7704-42b1-b774-33b42fc09c15"), new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6083), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("d99461aa-7175-4fd4-a849-7311d2bd64a1"), new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(6127), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null },
                    { new Guid("f1a0bba8-5804-4084-bf40-63168db2f003"), new DateTime(2026, 10, 9, 21, 31, 4, 139, DateTimeKind.Utc).AddTicks(5893), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRequests_PT_CandidateId",
                table: "PT_CandidateRequests",
                column: "PT_CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRequests_PT_CompanyId",
                table: "PT_CandidateRequests",
                column: "PT_CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRequests_PT_VacancyId",
                table: "PT_CandidateRequests",
                column: "PT_VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_CandidateRequests_RequestedByUserId",
                table: "PT_CandidateRequests",
                column: "RequestedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_CandidateRequests");

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("159074f3-55b4-4c90-bb19-f0d34d3cceab"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("2892b646-5075-4fa4-8f34-aa4c4d65f454"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("32b1a3e3-5844-4c55-a1ad-d599d41788ed"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("5e8a639b-da4b-4e19-9e32-62dc45d32afa"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("7567f361-a43b-4070-87d2-405dbb3fac0e"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("85adb332-410c-46c6-afed-99fb9f76cebb"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("90996e47-4509-45d4-9a5b-60a4b16edbd5"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("bd375efd-e3d6-43ff-9cf5-a6b753f279c0"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("c2f862e8-a8fd-4118-817d-cdce5db8793f"));

            migrationBuilder.DeleteData(
                table: "SY_DocumentTypes",
                keyColumn: "Id",
                keyValue: new Guid("dac655b4-0892-4403-97c7-96d67bc58c34"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("0bcf4edd-8456-47fe-bc2b-5aee1f6eda8b"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("3b7db822-ab9f-487f-91dd-aa4c6d56bc8c"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("5d23de2a-52c3-42cb-a3c6-6df473800269"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("606547cd-5463-47f6-8f38-5f820c2066d4"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("9956deb0-7a10-4e24-961a-98c9f442fe74"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("a024d4c6-995c-4488-934a-542f654f9c95"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("b6742fdb-b43b-44ce-bc84-5217997a8398"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("cf89e144-7704-42b1-b774-33b42fc09c15"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("d99461aa-7175-4fd4-a849-7311d2bd64a1"));

            migrationBuilder.DeleteData(
                table: "SY_WizardSteps",
                keyColumn: "Id",
                keyValue: new Guid("f1a0bba8-5804-4084-bf40-63168db2f003"));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2578));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2608));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2613));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b1111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2616));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b2222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2620));

            migrationBuilder.UpdateData(
                table: "PT_Plans",
                keyColumn: "Id",
                keyValue: new Guid("b3333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2625));

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("0265ab2b-380a-4c7c-b627-4f7ee3092918"), "Formación", new DateTime(2026, 10, 9, 21, 2, 59, 765, DateTimeKind.Utc).AddTicks(8394), null, null, null, "Título habilitante o certificación profesional", false, "Titulo / Certificación profesional", 8, null, null },
                    { new Guid("19af7b68-b44c-4cc9-9d56-7a134ecdce2a"), "Migratorio", new DateTime(2026, 10, 9, 21, 2, 59, 765, DateTimeKind.Utc).AddTicks(8384), null, null, null, "Visado que habilita a trabajar legalmente", false, "Visado de trabajo", 5, null, null },
                    { new Guid("3a47dcf0-b5ee-497b-b292-d2390f2277cd"), "Migratorio", new DateTime(2026, 10, 9, 21, 2, 59, 765, DateTimeKind.Utc).AddTicks(8373), null, null, null, "Autorización de trabajo en el país de destino", false, "Permiso de trabajo", 3, null, null },
                    { new Guid("83e1e972-7348-45f9-8286-109913ed8a4b"), "Fiscal", new DateTime(2026, 10, 9, 21, 2, 59, 765, DateTimeKind.Utc).AddTicks(8401), null, null, null, "Documento con número de afiliación a la seguridad social", false, "Nº Seguridad Social", 9, null, null },
                    { new Guid("93b08ff6-c120-499f-9ef2-eac354e5ebda"), "Legal", new DateTime(2026, 10, 9, 21, 2, 59, 765, DateTimeKind.Utc).AddTicks(8391), null, null, null, "Certificado de antecedentes penales apostillado", false, "Certificado de antecedentes penales", 7, null, null },
                    { new Guid("b1360198-1d20-4298-8769-ebc849a382cf"), "Fiscal", new DateTime(2026, 10, 9, 21, 2, 59, 765, DateTimeKind.Utc).AddTicks(8404), null, null, null, "Justificante de cuenta bancaria a nombre del candidato", false, "Cuenta bancaria (IBAN)", 10, null, null },
                    { new Guid("bd6ce6b8-bfcc-496a-9079-bd0708809a01"), "Identidad", new DateTime(2026, 10, 9, 21, 2, 59, 765, DateTimeKind.Utc).AddTicks(8323), null, null, null, "Pasaporte válido y en vigor", false, "Pasaporte", 1, null, null },
                    { new Guid("d04b73ac-5ee0-4385-90f3-3bced2df54c7"), "Salud", new DateTime(2026, 10, 9, 21, 2, 59, 765, DateTimeKind.Utc).AddTicks(8388), null, null, null, "Tarjeta sanitaria europea (TSE) o seguro médico privado", false, "Tarjeta sanitaria", 6, null, null },
                    { new Guid("d855c678-8bbf-411f-a9be-378cf9d9db3d"), "Habilitación", new DateTime(2026, 10, 9, 21, 2, 59, 765, DateTimeKind.Utc).AddTicks(8376), null, null, null, "Permiso de conducir válido", false, "Licencia de conducir", 4, null, null }
                });

            migrationBuilder.InsertData(
                table: "SY_DocumentTypes",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Name", "SortOrder", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("fe9e655a-cff4-40ef-9fd3-47ad81054fe8"), "Identidad", new DateTime(2026, 10, 9, 21, 2, 59, 765, DateTimeKind.Utc).AddTicks(8367), null, null, null, "DNI / NIE / Cédula de identidad", false, true, "Documento de identidad", 2, null, null });

            migrationBuilder.InsertData(
                table: "SY_WizardSteps",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "IsRequired", "Order", "Phase", "StepName", "StepNumber", "StepTitle", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("3bdecb05-5e2d-4c66-8197-63374b9fc436"), new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2372), null, null, null, "Add your education", false, false, 8, 2, "Education", 8, "Education", null, null },
                    { new Guid("6213527d-790a-4721-996f-934ef2623b36"), new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2326), null, null, null, "Select your skills", false, false, 4, 1, "Skills", 4, "Skills", null, null },
                    { new Guid("74fc4210-845c-4968-b0d7-d8b1439cff67"), new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2362), null, null, null, "Verify your data is correct", false, true, 6, 1, "Confirmation", 6, "Review and Confirm", null, null },
                    { new Guid("7f2abf4f-4a0d-416f-950a-5e362b4b2414"), new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2379), null, null, null, "Upload your CV/resume", false, false, 10, 2, "UploadCV", 10, "Upload CV", null, null },
                    { new Guid("8a636679-dfe3-4bf0-9e32-f96535bf4822"), new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2322), null, null, null, "Your professional information", false, true, 3, 1, "ProfessionalProfile", 3, "Professional Profile", null, null },
                    { new Guid("93ccab10-d672-417f-ac0d-6914b0d70287"), new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2369), null, null, null, "Add your work experience", false, false, 7, 2, "WorkExperience", 7, "Work Experience", null, null },
                    { new Guid("a1aa9c66-160b-462a-9af8-e3256895407b"), new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2294), null, null, null, "Tell us about yourself", false, true, 1, 1, "PersonalData", 1, "Personal Data", null, null },
                    { new Guid("a228de5a-2903-4953-93cf-bad0a824eb02"), new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2330), null, null, null, "Choose your preference", false, true, 5, 1, "Preferences", 5, "What do you want to do?", null, null },
                    { new Guid("c52991fc-0d49-4b5e-86b4-b26d41d8d588"), new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2308), null, null, null, "Where are you located?", false, true, 2, 1, "Location", 2, "Location", null, null },
                    { new Guid("ebc9f773-da4e-4c22-8fcc-57555b3d5d13"), new DateTime(2026, 10, 9, 21, 2, 59, 777, DateTimeKind.Utc).AddTicks(2375), null, null, null, "Add your certifications", false, false, 9, 2, "Certifications", 9, "Certifications", null, null }
                });
        }
    }
}
