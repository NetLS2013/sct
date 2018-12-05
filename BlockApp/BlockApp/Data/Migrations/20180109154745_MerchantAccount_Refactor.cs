using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BlockApp.Data.Migrations
{
    public partial class MerchantAccount_Refactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "MerchantAccounts");

            migrationBuilder.AddColumn<string>(
                name: "MerchantSecret",
                table: "MerchantAccounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MerchantSecret",
                table: "MerchantAccounts");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "MerchantAccounts",
                nullable: true);
        }
    }
}
