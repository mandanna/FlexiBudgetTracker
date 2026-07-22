using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Paychecks_IsClosed",
                table: "Paychecks",
                column: "IsClosed");

            migrationBuilder.CreateIndex(
                name: "IX_Paychecks_ReceivedDate",
                table: "Paychecks",
                column: "ReceivedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseDate",
                table: "Expenses",
                column: "ExpenseDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Paychecks_IsClosed",
                table: "Paychecks");

            migrationBuilder.DropIndex(
                name: "IX_Paychecks_ReceivedDate",
                table: "Paychecks");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ExpenseDate",
                table: "Expenses");
        }
    }
}
