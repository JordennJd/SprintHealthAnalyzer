﻿// <auto-generated />
using System;
using System.Collections.Generic;
using HealthSprintAnalyzer.Storage.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HealthSprintAnalyzer.WebApi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("SprintHealthAnalyzer.Entities.Dataset", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<DateTime>("From")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("LoadTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("ParsingTime")
                        .HasColumnType("interval");

                    b.Property<List<long>>("SprintsIds")
                        .IsRequired()
                        .HasColumnType("bigint[]");

                    b.Property<List<string>>("Teams")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<DateTime>("To")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Datasets");
                });

            modelBuilder.Entity("SprintHealthAnalyzer.Entities.Sprint", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("DatasetId")
                        .HasColumnType("text");

                    b.Property<List<long>>("EntityIds")
                        .IsRequired()
                        .HasColumnType("bigint[]");

                    b.Property<DateTime>("SprintEndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SprintName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("SprintStartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("SprintStatus")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DatasetId");

                    b.ToTable("Sprints");
                });

            modelBuilder.Entity("SprintHealthAnalyzer.Entities.Ticket", b =>
                {
                    b.Property<long>("EntityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("EntityId"));

                    b.Property<string>("Area")
                        .HasColumnType("text");

                    b.Property<string>("Assignee")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DueDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan?>("Estimation")
                        .HasColumnType("interval");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Owner")
                        .HasColumnType("text");

                    b.Property<long?>("ParentTicketId")
                        .HasColumnType("bigint");

                    b.Property<string>("Priority")
                        .HasColumnType("text");

                    b.Property<string>("Rank")
                        .HasColumnType("text");

                    b.Property<string>("Resolution")
                        .HasColumnType("text");

                    b.Property<TimeSpan?>("Spent")
                        .HasColumnType("interval");

                    b.Property<long?>("SprintId")
                        .HasColumnType("bigint");

                    b.Property<string>("State")
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .HasColumnType("text");

                    b.Property<string>("TicketNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.Property<string>("Workgroup")
                        .HasColumnType("text");

                    b.HasKey("EntityId");

                    b.HasIndex("SprintId");

                    b.ToTable("Tickets");
                });

            modelBuilder.Entity("SprintHealthAnalyzer.Entities.TicketHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("HistoryChange")
                        .HasColumnType("text");

                    b.Property<string>("HistoryChangeType")
                        .HasColumnType("text");

                    b.Property<DateTime?>("HistoryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("HistoryPropertyName")
                        .HasColumnType("text");

                    b.Property<int?>("HistoryVersion")
                        .HasColumnType("integer");

                    b.Property<long>("TicketId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("TicketId");

                    b.ToTable("TicketHistories");
                });

            modelBuilder.Entity("SprintHealthAnalyzer.Entities.Sprint", b =>
                {
                    b.HasOne("SprintHealthAnalyzer.Entities.Dataset", null)
                        .WithMany("Sprints")
                        .HasForeignKey("DatasetId");
                });

            modelBuilder.Entity("SprintHealthAnalyzer.Entities.Ticket", b =>
                {
                    b.HasOne("SprintHealthAnalyzer.Entities.Sprint", null)
                        .WithMany("Tickets")
                        .HasForeignKey("SprintId");
                });

            modelBuilder.Entity("SprintHealthAnalyzer.Entities.TicketHistory", b =>
                {
                    b.HasOne("SprintHealthAnalyzer.Entities.Ticket", null)
                        .WithMany("History")
                        .HasForeignKey("TicketId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SprintHealthAnalyzer.Entities.Dataset", b =>
                {
                    b.Navigation("Sprints");
                });

            modelBuilder.Entity("SprintHealthAnalyzer.Entities.Sprint", b =>
                {
                    b.Navigation("Tickets");
                });

            modelBuilder.Entity("SprintHealthAnalyzer.Entities.Ticket", b =>
                {
                    b.Navigation("History");
                });
#pragma warning restore 612, 618
        }
    }
}
