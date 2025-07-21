using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2025Tpi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear tabla Customer (Clientes)
            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    // Identificador único (PK)
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    // Email obligatorio, max 100 caracteres
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    // Nombre obligatorio, max 60 caracteres
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    // Teléfono opcional, max 15 caracteres
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    // Definir clave primaria en Id
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            // Crear tabla Product (Productos)
            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    // Id único (PK)
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    // SKU único, max 15 caracteres, obligatorio
                    Sku = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    // Código interno, max 30 caracteres, obligatorio
                    InternalCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    // Nombre, max 60 caracteres, obligatorio
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    // Descripción opcional, max 500 caracteres
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    // Precio actual con precisión 15,2, obligatorio
                    CurrentPrice = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    // Cantidad en stock, obligatorio
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    // Indica si el producto está activo, por defecto true
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    // PK en Id
                    table.PrimaryKey("PK_Product", x => x.Id);
                });

            // Crear tabla Order (Pedidos)
            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    // Id único (PK)
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    // Fecha del pedido, obligatorio
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    // Dirección de envío, max 200 caracteres, obligatorio
                    ShippingAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    // Dirección de facturación, max 200 caracteres, obligatorio
                    BillingAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    // Notas opcionales, max 500 caracteres
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    // Monto total del pedido, decimal con precisión, obligatorio
                    TotalAmount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    // Estado del pedido, almacenado como int (enum), obligatorio
                    Status = table.Column<int>(type: "int", nullable: false),
                    // Id del cliente asociado, nullable (puede ser null)
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    // PK en Id
                    table.PrimaryKey("PK_Order", x => x.Id);
                    // FK a Customer con acción SetNull al borrar cliente
                    table.ForeignKey(
                        name: "FK_Order_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // Crear tabla OrderItem (Detalle de pedidos)
            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    // Id único (PK)
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    // Cantidad solicitada, obligatorio
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    // Precio unitario, decimal, obligatorio
                    UnitPrice = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    // Subtotal (Quantity * UnitPrice), decimal, obligatorio
                    Subtotal = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    // FK a Order, obligatorio
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    // FK a Product, obligatorio
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    // PK en Id
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                    // FK a Order con borrado en cascada (borrar pedido elimina items)
                    table.ForeignKey(
                        name: "FK_OrderItem_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    // FK a Product con restricción (no permite borrar producto con items asociados)
                    table.ForeignKey(
                        name: "FK_OrderItem_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Índice para optimizar consultas por CustomerId en Order
            migrationBuilder.CreateIndex(
                name: "IX_Order_CustomerId",
                table: "Order",
                column: "CustomerId");

            // Índices para optimizar consultas en OrderItem por OrderId y ProductId
            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ProductId",
                table: "OrderItem",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Borrar tablas en orden inverso para evitar conflictos de FK
            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Customer");
        }
    }
}
