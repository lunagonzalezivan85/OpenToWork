using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddMigrationInfoAndDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<bool>(
                name: "HasPassport",
                table: "PT_Candidates",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "PT_Candidates",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "PT_Candidates",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SY_DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRequired = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SY_DocumentTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_RecruitmentDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_CandidateRecruitmentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SY_DocumentTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FileUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubmittedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    VerifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_PT_RecruitmentDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_RecruitmentDocuments_PT_CandidateRecruitments_PT_Candidat~",
                        column: x => x.PT_CandidateRecruitmentId,
                        principalTable: "PT_CandidateRecruitments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_RecruitmentDocuments_SC_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PT_RecruitmentDocuments_SY_DocumentTypes_SY_DocumentTypeId",
                        column: x => x.SY_DocumentTypeId,
                        principalTable: "SY_DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentDocuments_PT_CandidateRecruitmentId_IsDeleted",
                table: "PT_RecruitmentDocuments",
                columns: new[] { "PT_CandidateRecruitmentId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentDocuments_PT_CandidateRecruitmentId_SY_Documen~",
                table: "PT_RecruitmentDocuments",
                columns: new[] { "PT_CandidateRecruitmentId", "SY_DocumentTypeId", "IsDeleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentDocuments_Status_IsDeleted",
                table: "PT_RecruitmentDocuments",
                columns: new[] { "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentDocuments_SY_DocumentTypeId",
                table: "PT_RecruitmentDocuments",
                column: "SY_DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_RecruitmentDocuments_VerifiedByUserId",
                table: "PT_RecruitmentDocuments",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SY_DocumentTypes_Category_IsDeleted",
                table: "SY_DocumentTypes",
                columns: new[] { "Category", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_SY_DocumentTypes_Name_IsDeleted",
                table: "SY_DocumentTypes",
                columns: new[] { "Name", "IsDeleted" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_RecruitmentDocuments");

            migrationBuilder.DropTable(
                name: "SY_DocumentTypes");

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

            migrationBuilder.DropColumn(
                name: "HasPassport",
                table: "PT_Candidates");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "PT_Candidates");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "PT_Candidates");

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
        }
    }
}
