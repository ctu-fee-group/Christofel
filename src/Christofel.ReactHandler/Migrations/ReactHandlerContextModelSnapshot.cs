﻿// <auto-generated />
using Christofel.ReactHandler.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Christofel.ReactHandler.Migrations
{
    [DbContext(typeof(ReactHandlerContext))]
    partial class ReactHandlerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.9");

            modelBuilder.Entity("Christofel.ReactHandler.Database.Models.HandleReact", b =>
                {
                    b.Property<int>("HandleReactId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<long>("ChannelId")
                        .HasColumnType("bigint");

                    b.Property<string>("Emoji")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<long>("EntityId")
                        .HasColumnType("bigint");

                    b.Property<long>("MessageId")
                        .HasColumnType("bigint");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("HandleReactId");

                    b.ToTable("HandleReacts");
                });
#pragma warning restore 612, 618
        }
    }
}
