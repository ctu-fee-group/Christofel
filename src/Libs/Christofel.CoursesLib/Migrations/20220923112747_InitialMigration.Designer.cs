﻿// <auto-generated />
using Christofel.CoursesLib.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Christofel.CoursesLib.Migrations
{
    [DbContext(typeof(CoursesContext))]
    [Migration("20220923112747_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Courses")
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Christofel.CoursesLib.Database.CourseAssignment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<long>("ChannelId")
                        .HasColumnType("bigint");

                    b.Property<string>("CourseKey")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("CourseName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("DepartmentKey")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("CourseAssignments", "Courses");
                });

            modelBuilder.Entity("Christofel.CoursesLib.Database.DepartmentAssignment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<long>("CategoryId")
                        .HasColumnType("bigint");

                    b.Property<string>("DepartmentKey")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("DepartmentName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("DepartmentAssignments", "Courses");
                });
#pragma warning restore 612, 618
        }
    }
}
