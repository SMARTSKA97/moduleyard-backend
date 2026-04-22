using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MFA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            // Assuming 'users' table already exists, so we skip it.
            /*
            migrationBuilder.CreateTable(
                name: "users",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    totp_secret = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                });
            */

            migrationBuilder.CreateTable(
                name: "fido_credentials",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descriptor_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    public_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    user_handle = table.Column<byte[]>(type: "bytea", nullable: false),
                    signature_counter = table.Column<long>(type: "bigint", nullable: false),
                    cred_type = table.Column<string>(type: "text", nullable: false),
                    reg_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    aa_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("fido_credentials_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fido_credentials_user_id_fkey",
                        column: x => x.user_id,
                        principalSchema: "core",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fido_credentials_user_id",
                schema: "core",
                table: "fido_credentials",
                column: "user_id");

            /*
            migrationBuilder.CreateIndex(
                name: "users_email_key",
                schema: "core",
                table: "users",
                column: "email",
                unique: true);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fido_credentials",
                schema: "core");

            // migrationBuilder.DropTable(
            //     name: "users",
            //     schema: "core");
        }
    }
}
