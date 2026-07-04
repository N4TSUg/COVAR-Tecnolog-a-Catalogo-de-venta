using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace COVAR_Tecnologia.Migrations
{
    /// <inheritdoc />
    public partial class AsociacionProductoTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ÚNICAMENTE AGREGAR LA NUEVA COLUMNA Y SU RELACIÓN
            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "TicketsSoporte",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketsSoporte_ProductoId",
                table: "TicketsSoporte",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketsSoporte_Productos_ProductoId",
                table: "TicketsSoporte",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // REVERTIR SÓLO ESE CAMBIO
            migrationBuilder.DropForeignKey(
                name: "FK_TicketsSoporte_Productos_ProductoId",
                table: "TicketsSoporte");

            migrationBuilder.DropIndex(
                name: "IX_TicketsSoporte_ProductoId",
                table: "TicketsSoporte");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "TicketsSoporte");
        }
    }
}