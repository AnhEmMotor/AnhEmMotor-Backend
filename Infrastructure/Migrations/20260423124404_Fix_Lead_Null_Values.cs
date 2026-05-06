using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Lead_Null_Values : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.Sql("UPDATE [Lead] SET [FullName] = ISNULL([FullName], '')");
            migrationBuilder.Sql("UPDATE [Lead] SET [Email] = ISNULL([Email], '')");
            migrationBuilder.Sql("UPDATE [Lead] SET [PhoneNumber] = ISNULL([PhoneNumber], '')");
            migrationBuilder.Sql("UPDATE [Lead] SET [Status] = ISNULL([Status], 'New')");
            migrationBuilder.Sql("UPDATE [Lead] SET [Source] = ISNULL([Source], 'WebStore')");
            migrationBuilder.Sql("UPDATE [Lead] SET [InterestedVehicle] = ISNULL([InterestedVehicle], '')");
            migrationBuilder.Sql("UPDATE [Lead] SET [Address] = ISNULL([Address], '')");
            migrationBuilder.Sql("UPDATE [Lead] SET [AddressDetail] = ISNULL([AddressDetail], '')");
            migrationBuilder.Sql("UPDATE [Lead] SET [Ward] = ISNULL([Ward], '')");
            migrationBuilder.Sql("UPDATE [Lead] SET [District] = ISNULL([District], 'Biên Hòa')");
            migrationBuilder.Sql("UPDATE [Lead] SET [Province] = ISNULL([Province], 'Đồng Nai')");
            migrationBuilder.Sql("UPDATE [Lead] SET [Gender] = ISNULL([Gender], '')");
            migrationBuilder.Sql("UPDATE [Lead] SET [IdentificationNumber] = ISNULL([IdentificationNumber], '')");
            migrationBuilder.Sql("UPDATE [Lead] SET [Tier] = ISNULL([Tier], 'Thành viên mới')");
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
