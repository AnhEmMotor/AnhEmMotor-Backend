using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_PendingChanges_Late_23_04 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The column 'InterestedVehicle' is now added in the initial Lead table creation.
            /*
            migrationBuilder.AddColumn<string>(
                name: "InterestedVehicle",
                table: "Lead",
                type: "nvarchar(255)",
                nullable: true,
                defaultValue: "");
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterestedVehicle",
                table: "Lead");
        }
    }
}
