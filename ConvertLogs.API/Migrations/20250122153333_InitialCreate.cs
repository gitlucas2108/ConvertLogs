using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ConvertLogs.API.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogsConvertidos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Provider = table.Column<string>(nullable: true),
                    HttpMethod = table.Column<string>(nullable: true),
                    StatusCode = table.Column<int>(nullable: false),
                    UriPath = table.Column<string>(nullable: true),
                    TimeTaken = table.Column<int>(nullable: false),
                    ResponseSize = table.Column<int>(nullable: false),
                    CacheStatus = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsConvertidos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogsOrigem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ResponseSize = table.Column<int>(nullable: false),
                    StatusCode = table.Column<int>(nullable: false),
                    CacheStatus = table.Column<string>(nullable: true),
                    HttpMethod = table.Column<string>(nullable: true),
                    UriPath = table.Column<string>(nullable: true),
                    TimeTaken = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsOrigem", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogsConvertidos");

            migrationBuilder.DropTable(
                name: "LogsOrigem");
        }
    }
}
