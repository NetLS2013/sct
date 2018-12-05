using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BlockApp.Data.Migrations
{
    public partial class TransactionHex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerConfirmedDepositTx",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerConfirmedWithdrawTx",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnconfirmedDepositTx",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnconfirmedWithdrawTx",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerConfirmedDepositTx",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BuyerConfirmedWithdrawTx",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UnconfirmedDepositTx",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UnconfirmedWithdrawTx",
                table: "Transactions");
        }
    }
}
