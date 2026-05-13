using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowStack.LabelService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "checklists",
                columns: table => new
                {
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checklists", x => x.checklist_id);
                });

            migrationBuilder.CreateTable(
                name: "labels",
                columns: table => new
                {
                    label_id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_labels", x => x.label_id);
                });

            migrationBuilder.CreateTable(
                name: "checklist_items",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checklist_items", x => x.item_id);
                    table.ForeignKey(
                        name: "fk_checklist_items_checklists_checklist_id",
                        column: x => x.checklist_id,
                        principalTable: "checklists",
                        principalColumn: "checklist_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "card_labels",
                columns: table => new
                {
                    card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_card_labels", x => new { x.card_id, x.label_id });
                    table.ForeignKey(
                        name: "fk_card_labels_labels_label_id",
                        column: x => x.label_id,
                        principalTable: "labels",
                        principalColumn: "label_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_card_labels_card_id",
                table: "card_labels",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "ix_card_labels_label_id",
                table: "card_labels",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "ix_checklist_items_checklist_id",
                table: "checklist_items",
                column: "checklist_id");

            migrationBuilder.CreateIndex(
                name: "ix_checklists_card_id",
                table: "checklists",
                column: "card_id");

            migrationBuilder.CreateIndex(
                name: "ix_labels_board_id",
                table: "labels",
                column: "board_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_labels");

            migrationBuilder.DropTable(
                name: "checklist_items");

            migrationBuilder.DropTable(
                name: "labels");

            migrationBuilder.DropTable(
                name: "checklists");
        }
    }
}
