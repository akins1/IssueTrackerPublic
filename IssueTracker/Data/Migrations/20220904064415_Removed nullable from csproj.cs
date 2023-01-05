using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssueTracker.Data.Migrations
{
    public partial class Removednullablefromcsproj : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "FileData",
                table: "TicketAttachments",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea", //smallint[]
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "ImageFileData",
                table: "Projects",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea", //smallint[]
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "AvatarFileData",
                table: "AspNetUsers",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea", //smallint[]
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "FileData",
                table: "TicketAttachments",
                type: "bytea", //smallint[]
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "ImageFileData",
                table: "Projects",
                type: "bytea", //smallint[]
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "AvatarFileData",
                table: "AspNetUsers",
                type: "bytea", //smallint[]
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);
        }
    }
}
