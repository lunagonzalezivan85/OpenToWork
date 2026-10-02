using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <summary>
    /// Registro de candidato (1-Oct): tipo de documento, consentimientos del registro (privacidad y
    /// comunicaciones comerciales con fecha, IP y version) y codigo de verificacion del correo.
    /// Se quito el ruido de seeds (Delete/Insert/UpdateData con GUIDs y fechas nuevas) que EF regenera.
    /// </summary>
    public partial class RegistrationConsentAndEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsentIp",
                table: "SC_Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EmailVerificationAttempts",
                table: "SC_Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationCodeHash",
                table: "SC_Users",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationExpiresAt",
                table: "SC_Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MarketingConsent",
                table: "SC_Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "MarketingConsentAt",
                table: "SC_Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyAcceptedAt",
                table: "SC_Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyPolicyVersion",
                table: "SC_Users",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DocumentType",
                table: "PT_Candidates",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ConsentIp", table: "SC_Users");
            migrationBuilder.DropColumn(name: "EmailVerificationAttempts", table: "SC_Users");
            migrationBuilder.DropColumn(name: "EmailVerificationCodeHash", table: "SC_Users");
            migrationBuilder.DropColumn(name: "EmailVerificationExpiresAt", table: "SC_Users");
            migrationBuilder.DropColumn(name: "MarketingConsent", table: "SC_Users");
            migrationBuilder.DropColumn(name: "MarketingConsentAt", table: "SC_Users");
            migrationBuilder.DropColumn(name: "PrivacyAcceptedAt", table: "SC_Users");
            migrationBuilder.DropColumn(name: "PrivacyPolicyVersion", table: "SC_Users");
            migrationBuilder.DropColumn(name: "DocumentType", table: "PT_Candidates");
        }
    }
}
