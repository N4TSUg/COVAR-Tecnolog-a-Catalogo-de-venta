using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COVAR_Tecnologia.Migrations
{
    /// <inheritdoc />
    public partial class EditTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mensajes_TicketsSoporte_TicketSoporteId",
                table: "Mensajes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Mensajes",
                table: "Mensajes");

            migrationBuilder.RenameTable(
                name: "Mensajes",
                newName: "MensajesSoporte");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "MensajesSoporte",
                newName: "FechaEnvio");

            migrationBuilder.RenameColumn(
                name: "Contenido",
                table: "MensajesSoporte",
                newName: "Texto");

            migrationBuilder.RenameIndex(
                name: "IX_Mensajes_TicketSoporteId",
                table: "MensajesSoporte",
                newName: "IX_MensajesSoporte_TicketSoporteId");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "TicketsSoporte",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "EsRespuestaAdmin",
                table: "MensajesSoporte",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MensajesSoporte",
                table: "MensajesSoporte",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MensajesSoporte_TicketsSoporte_TicketSoporteId",
                table: "MensajesSoporte",
                column: "TicketSoporteId",
                principalTable: "TicketsSoporte",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MensajesSoporte_TicketsSoporte_TicketSoporteId",
                table: "MensajesSoporte");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MensajesSoporte",
                table: "MensajesSoporte");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "TicketsSoporte");

            migrationBuilder.DropColumn(
                name: "EsRespuestaAdmin",
                table: "MensajesSoporte");

            migrationBuilder.RenameTable(
                name: "MensajesSoporte",
                newName: "Mensajes");

            migrationBuilder.RenameColumn(
                name: "Texto",
                table: "Mensajes",
                newName: "Contenido");

            migrationBuilder.RenameColumn(
                name: "FechaEnvio",
                table: "Mensajes",
                newName: "Fecha");

            migrationBuilder.RenameIndex(
                name: "IX_MensajesSoporte_TicketSoporteId",
                table: "Mensajes",
                newName: "IX_Mensajes_TicketSoporteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Mensajes",
                table: "Mensajes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Mensajes_TicketsSoporte_TicketSoporteId",
                table: "Mensajes",
                column: "TicketSoporteId",
                principalTable: "TicketsSoporte",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
