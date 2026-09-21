using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskBridge.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActorIpAddressToAuditEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActorIpAddress",
                table: "AuditEntries",
                type: "TEXT",
                maxLength: 45,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActorIpAddress",
                table: "AuditEntries");
        }
    }
}
