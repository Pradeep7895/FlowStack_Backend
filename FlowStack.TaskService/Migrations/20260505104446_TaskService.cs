using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowStack.TaskService.Migrations
{
    /// <inheritdoc />
    public partial class TaskService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cards",
                columns: table => new
                {
                    card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    list_id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    cover_color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cards", x => x.card_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cards_assignee_id",
                table: "cards",
                column: "assignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_cards_board_id",
                table: "cards",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_cards_due_date",
                table: "cards",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "ix_cards_list_id",
                table: "cards",
                column: "list_id");

            migrationBuilder.CreateIndex(
                name: "ix_cards_list_id_position",
                table: "cards",
                columns: new[] { "list_id", "position" });

            migrationBuilder.CreateIndex(
                name: "ix_cards_priority",
                table: "cards",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "ix_cards_status",
                table: "cards",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cards");
        }
    }
}
