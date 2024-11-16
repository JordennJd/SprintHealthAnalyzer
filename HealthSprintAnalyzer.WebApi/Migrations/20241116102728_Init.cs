using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HealthSprintAnalyzer.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Datasets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LoadTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ParsingTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    From = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    To = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Teams = table.Column<List<string>>(type: "text[]", nullable: false),
                    SprintsIds = table.Column<List<long>>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Datasets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sprints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SprintName = table.Column<string>(type: "text", nullable: false),
                    SprintStatus = table.Column<string>(type: "text", nullable: false),
                    SprintStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SprintEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntityIds = table.Column<List<long>>(type: "bigint[]", nullable: false),
                    DatasetId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sprints_Datasets_DatasetId",
                        column: x => x.DatasetId,
                        principalTable: "Datasets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    EntityId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Area = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<string>(type: "text", nullable: true),
                    TicketNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    ParentTicketId = table.Column<long>(type: "bigint", nullable: true),
                    Assignee = table.Column<string>(type: "text", nullable: true),
                    Owner = table.Column<string>(type: "text", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Rank = table.Column<string>(type: "text", nullable: true),
                    Estimation = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Spent = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Workgroup = table.Column<string>(type: "text", nullable: true),
                    Resolution = table.Column<string>(type: "text", nullable: true),
                    SprintId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.EntityId);
                    table.ForeignKey(
                        name: "FK_Tickets_Sprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "Sprints",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TicketHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TicketId = table.Column<long>(type: "bigint", nullable: false),
                    HistoryPropertyName = table.Column<string>(type: "text", nullable: true),
                    HistoryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HistoryVersion = table.Column<int>(type: "integer", nullable: true),
                    HistoryChangeType = table.Column<string>(type: "text", nullable: true),
                    HistoryChange = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketHistories_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_DatasetId",
                table: "Sprints",
                column: "DatasetId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistories_TicketId",
                table: "TicketHistories",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SprintId",
                table: "Tickets",
                column: "SprintId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketHistories");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Sprints");

            migrationBuilder.DropTable(
                name: "Datasets");
        }
    }
}
