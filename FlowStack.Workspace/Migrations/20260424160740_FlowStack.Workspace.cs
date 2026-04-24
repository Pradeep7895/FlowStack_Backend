using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowStack.Workspace.Migrations
{
    /// <inheritdoc />
    public partial class FlowStackWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workspaces",
                columns: table => new
                {
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    visibility = table.Column<int>(type: "integer", nullable: false),
                    logo_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspaces", x => x.workspace_id);
                });

            migrationBuilder.CreateTable(
                name: "workspace_members",
                columns: table => new
                {
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workspace_members", x => x.member_id);
                    table.ForeignKey(
                        name: "fk_workspace_members_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "workspace_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_workspace_members_workspace_id",
                table: "workspace_members",
                column: "workspace_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workspace_members");

            migrationBuilder.DropTable(
                name: "workspaces");
        }
    }
}
