using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolunteerPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddHelpPostsCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HelpPosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AddressApprox = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NeededVolunteers = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NeedsReviewReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpPosts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HelpPosts_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HelpPostApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HelpPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicantOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AvailabilityNote = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RejectReason = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpPostApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpPostApplications_HelpPosts_HelpPostId",
                        column: x => x.HelpPostId,
                        principalTable: "HelpPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HelpPostApplications_Organizations_ApplicantOrganizationId",
                        column: x => x.ApplicantOrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HelpPostApplications_Users_ApplicantUserId",
                        column: x => x.ApplicantUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HelpPostAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HelpPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpPostAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpPostAssignments_HelpPostApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "HelpPostApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HelpPostAssignments_HelpPosts_HelpPostId",
                        column: x => x.HelpPostId,
                        principalTable: "HelpPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HelpPostApplications_ApplicantOrganizationId",
                table: "HelpPostApplications",
                column: "ApplicantOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpPostApplications_ApplicantUserId",
                table: "HelpPostApplications",
                column: "ApplicantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpPostApplications_HelpPostId_ApplicantUserId",
                table: "HelpPostApplications",
                columns: new[] { "HelpPostId", "ApplicantUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HelpPostApplications_HelpPostId_Status",
                table: "HelpPostApplications",
                columns: new[] { "HelpPostId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_HelpPostAssignments_ApplicationId",
                table: "HelpPostAssignments",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HelpPostAssignments_HelpPostId",
                table: "HelpPostAssignments",
                column: "HelpPostId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpPosts_CreatedByUserId",
                table: "HelpPosts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpPosts_OrganizationId",
                table: "HelpPosts",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpPosts_Type_Status",
                table: "HelpPosts",
                columns: new[] { "Type", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HelpPostAssignments");

            migrationBuilder.DropTable(
                name: "HelpPostApplications");

            migrationBuilder.DropTable(
                name: "HelpPosts");
        }
    }
}
