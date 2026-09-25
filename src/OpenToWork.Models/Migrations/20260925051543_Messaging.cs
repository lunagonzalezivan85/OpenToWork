using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    /// <remarks>Mensajeria real usuario del portal &lt;-&gt; Trato Directo (PT_Conversations, PT_Messages),
    /// reemplaza los datos de ejemplo fijos de MessagesController. Recortada a mano del ruido de seed
    /// de SY_DocumentTypes/SY_WizardSteps/PT_Plans (ver Bitacora 21-Sep).</remarks>
    public partial class Messaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PT_Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SCUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_VacancyId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Subject = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignedStaffId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastMessageAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastMessagePreview = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnreadForUser = table.Column<int>(type: "int", nullable: false),
                    UnreadForStaff = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PT_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_Conversations_PT_Vacancies_PT_VacancyId",
                        column: x => x.PT_VacancyId,
                        principalTable: "PT_Vacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PT_Conversations_SC_Users_AssignedStaffId",
                        column: x => x.AssignedStaffId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PT_Conversations_SC_Users_SCUserId",
                        column: x => x.SCUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PT_Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PT_ConversationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SenderUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsFromStaff = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Content = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    table.PrimaryKey("PK_PT_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_Messages_PT_Conversations_PT_ConversationId",
                        column: x => x.PT_ConversationId,
                        principalTable: "PT_Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_Messages_SC_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "SC_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Conversations_AssignedStaffId",
                table: "PT_Conversations",
                column: "AssignedStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Conversations_PT_VacancyId",
                table: "PT_Conversations",
                column: "PT_VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Conversations_SCUserId_LastMessageAt",
                table: "PT_Conversations",
                columns: new[] { "SCUserId", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_Conversations_UnreadForStaff_LastMessageAt",
                table: "PT_Conversations",
                columns: new[] { "UnreadForStaff", "LastMessageAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_Messages_PT_ConversationId_CreatedAt",
                table: "PT_Messages",
                columns: new[] { "PT_ConversationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_Messages_SenderUserId",
                table: "PT_Messages",
                column: "SenderUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_Messages");

            migrationBuilder.DropTable(
                name: "PT_Conversations");
        }
    }
}
