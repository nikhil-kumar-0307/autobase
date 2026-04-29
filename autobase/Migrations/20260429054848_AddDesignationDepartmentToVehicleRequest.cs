using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace autobase.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignationDepartmentToVehicleRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "VehicleRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Designation",
                table: "VehicleRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "VehicleRequests");

            migrationBuilder.DropColumn(
                name: "Designation",
                table: "VehicleRequests");
        }
    }
}
