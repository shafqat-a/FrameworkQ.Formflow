using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormDesigner.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "form_definitions",
                columns: table => new
                {
                    form_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    dsl_json = table.Column<string>(type: "jsonb", nullable: false),
                    is_committed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_definitions", x => x.form_id);
                });

            migrationBuilder.CreateTable(
                name: "form_instances",
                columns: table => new
                {
                    instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    form_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "draft"),
                    data_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_instances", x => x.instance_id);
                    table.ForeignKey(
                        name: "FK_form_instances_form_definitions_form_id",
                        column: x => x.form_id,
                        principalTable: "form_definitions",
                        principalColumn: "form_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "temporary_states",
                columns: table => new
                {
                    state_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_json = table.Column<string>(type: "jsonb", nullable: false),
                    saved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    user_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temporary_states", x => x.state_id);
                    table.ForeignKey(
                        name: "FK_temporary_states_form_instances_instance_id",
                        column: x => x.instance_id,
                        principalTable: "form_instances",
                        principalColumn: "instance_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_form_definitions_created_at",
                table: "form_definitions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_form_definitions_dsl_json",
                table: "form_definitions",
                column: "dsl_json")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_form_definitions_is_active",
                table: "form_definitions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_form_definitions_is_committed",
                table: "form_definitions",
                column: "is_committed");

            migrationBuilder.CreateIndex(
                name: "ix_form_instances_created_at",
                table: "form_instances",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_form_instances_data_json",
                table: "form_instances",
                column: "data_json")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_form_instances_form_id",
                table: "form_instances",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "ix_form_instances_status",
                table: "form_instances",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_form_instances_user_id",
                table: "form_instances",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_temporary_states_instance_id",
                table: "temporary_states",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "ix_temporary_states_saved_at",
                table: "temporary_states",
                column: "saved_at");

            migrationBuilder.CreateIndex(
                name: "ix_temporary_states_user_id",
                table: "temporary_states",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "temporary_states");

            migrationBuilder.DropTable(
                name: "form_instances");

            migrationBuilder.DropTable(
                name: "form_definitions");
        }
    }
}
