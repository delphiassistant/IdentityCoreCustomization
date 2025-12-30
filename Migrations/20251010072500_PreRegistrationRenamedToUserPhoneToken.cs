using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityCoreCustomization.Migrations
{
    /// <inheritdoc />
    public partial class PreRegistrationRenamedToUserPhoneToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreRegistrations");

            migrationBuilder.CreateTable(
                name: "UserPhoneTokens",
                columns: table => new
                {
                    UserPhoneTokenID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    ExpireTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuthenticationCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthenticationKey = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPhoneTokens", x => x.UserPhoneTokenID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPhoneTokens");

            migrationBuilder.CreateTable(
                name: "PreRegistrations",
                columns: table => new
                {
                    RegisterationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthenticationCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthenticationKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confirmed = table.Column<bool>(type: "bit", nullable: false),
                    ExpireTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreRegistrations", x => x.RegisterationID);
                });
        }
    }
}
