using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SurveyPortal.Api.Migrations
{
    /// <inheritdoc />
    public partial class RestructureSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Units_UnitId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "CorrectiveFeedback",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FavourableFeedback",
                table: "Submissions");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "Submissions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Answers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubmissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitId = table.Column<int>(type: "INTEGER", nullable: false),
                    FavourableFeedback = table.Column<string>(type: "TEXT", nullable: false),
                    CorrectiveFeedback = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedback_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedback_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_UnitId",
                table: "Answers",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_SubmissionId",
                table: "Feedback",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_UnitId",
                table: "Feedback",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Units_UnitId",
                table: "Answers",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Units_UnitId",
                table: "Submissions",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Units_UnitId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Units_UnitId",
                table: "Submissions");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Answers_UnitId",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Answers");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                table: "Submissions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectiveFeedback",
                table: "Submissions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FavourableFeedback",
                table: "Submissions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Units_UnitId",
                table: "Submissions",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
