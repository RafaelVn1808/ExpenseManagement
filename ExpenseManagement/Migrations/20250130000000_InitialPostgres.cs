using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExpenseApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    ExpenseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Installments = table.Column<int>(type: "integer", nullable: false),
                    InstallmentAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Validity = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NoteImageUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ProofImageUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.ExpenseId);
                    table.ForeignKey(
                        name: "FK_Expenses_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CategoryId",
                table: "Expenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Status",
                table: "Expenses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId_StartDate",
                table: "Expenses",
                columns: new[] { "UserId", "StartDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
