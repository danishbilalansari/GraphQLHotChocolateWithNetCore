﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphQL.Demo.API.Migrations
{
    /// <inheritdoc />
    public partial class CourseCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Courses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Courses");
        }
    }
}
