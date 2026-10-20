using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IT_Helpdesk_Platform.Migrations
{
    /// <inheritdoc />
    public partial class Test6M4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentId",
                table: "Attachments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_CommentId",
                table: "Attachments",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Comments_CommentId",
                table: "Attachments",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Comments_CommentId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_CommentId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "Attachments");
        }
    }
}
