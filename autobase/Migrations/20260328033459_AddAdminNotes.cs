using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace autobase.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "VehicleRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "VehicleRequests");
        }
    }
}
