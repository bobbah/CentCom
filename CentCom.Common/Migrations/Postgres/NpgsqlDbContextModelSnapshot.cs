﻿// <auto-generated />
using System;
using CentCom.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CentCom.Common.Migrations.Postgres
{
    [DbContext(typeof(NpgsqlDbContext))]
    partial class NpgsqlDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.9")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("CentCom.Common.Models.Ban", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<int>("BanAttributes")
                        .HasColumnType("integer")
                        .HasColumnName("ban_attributes");

                    b.Property<string>("BanID")
                        .HasColumnType("text")
                        .HasColumnName("ban_id");

                    b.Property<long>("BanType")
                        .HasColumnType("bigint")
                        .HasColumnName("ban_type");

                    b.Property<string>("BannedBy")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("banned_by");

                    b.Property<DateTime>("BannedOn")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("banned_on");

                    b.Property<string>("CKey")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("c_key");

                    b.Property<DateTime?>("Expires")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("expires");

                    b.Property<string>("Reason")
                        .HasColumnType("text")
                        .HasColumnName("reason");

                    b.Property<int>("Source")
                        .HasColumnType("integer")
                        .HasColumnName("source");

                    b.Property<string>("UnbannedBy")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("unbanned_by");

                    b.HasKey("Id")
                        .HasName("pk_bans");

                    b.HasIndex("CKey")
                        .HasDatabaseName("ix_bans_c_key");

                    b.HasIndex("Source")
                        .HasDatabaseName("ix_bans_source");

                    b.ToTable("bans");
                });

            modelBuilder.Entity("CentCom.Common.Models.BanSource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("Display")
                        .HasColumnType("text")
                        .HasColumnName("display");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<long>("RoleplayLevel")
                        .HasColumnType("bigint")
                        .HasColumnName("roleplay_level");

                    b.HasKey("Id")
                        .HasName("pk_ban_sources");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_ban_sources_name");

                    b.ToTable("ban_sources");
                });

            modelBuilder.Entity("CentCom.Common.Models.CheckHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<int>("Added")
                        .HasColumnType("integer")
                        .HasColumnName("added");

                    b.Property<bool>("CompleteRefresh")
                        .HasColumnType("boolean")
                        .HasColumnName("complete_refresh");

                    b.Property<DateTimeOffset?>("CompletedDataFetch")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("completed_data_fetch");

                    b.Property<DateTimeOffset?>("CompletedUpload")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("completed_upload");

                    b.Property<int>("Deleted")
                        .HasColumnType("integer")
                        .HasColumnName("deleted");

                    b.Property<int>("Erroneous")
                        .HasColumnType("integer")
                        .HasColumnName("erroneous");

                    b.Property<string>("Exception")
                        .HasColumnType("text")
                        .HasColumnName("exception");

                    b.Property<string>("ExceptionDetailed")
                        .HasColumnType("text")
                        .HasColumnName("exception_detailed");

                    b.Property<DateTimeOffset?>("Failed")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("failed");

                    b.Property<string>("Parser")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("parser");

                    b.Property<string>("ResponseContent")
                        .HasColumnType("text")
                        .HasColumnName("response_content");

                    b.Property<DateTimeOffset>("Started")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("started");

                    b.Property<bool>("Success")
                        .HasColumnType("boolean")
                        .HasColumnName("success");

                    b.Property<int>("Updated")
                        .HasColumnType("integer")
                        .HasColumnName("updated");

                    b.HasKey("Id")
                        .HasName("pk_check_history");

                    b.HasIndex("Parser", "Started")
                        .HasDatabaseName("ix_check_history_parser_started");

                    b.ToTable("check_history");
                });

            modelBuilder.Entity("CentCom.Common.Models.FlatBansVersion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<DateTime>("PerformedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("performed_at");

                    b.Property<long>("Version")
                        .HasColumnType("bigint")
                        .HasColumnName("version");

                    b.HasKey("Id")
                        .HasName("pk_flat_bans_version");

                    b.HasIndex("Name", "Version")
                        .IsUnique()
                        .HasDatabaseName("ix_flat_bans_version_name_version");

                    b.ToTable("flat_bans_version");
                });

            modelBuilder.Entity("CentCom.Common.Models.JobBan", b =>
                {
                    b.Property<int>("BanId")
                        .HasColumnType("integer")
                        .HasColumnName("ban_id");

                    b.Property<string>("Job")
                        .HasColumnType("text")
                        .HasColumnName("job");

                    b.HasKey("BanId", "Job")
                        .HasName("pk_job_bans");

                    b.ToTable("job_bans");
                });

            modelBuilder.Entity("CentCom.Common.Models.NotifiedFailure", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<long>("CheckHistoryId")
                        .HasColumnType("bigint")
                        .HasColumnName("check_history_id");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("timestamp");

                    b.HasKey("Id")
                        .HasName("pk_notified_failures");

                    b.HasIndex("CheckHistoryId")
                        .IsUnique()
                        .HasDatabaseName("ix_notified_failures_check_history_id");

                    b.ToTable("notified_failures");
                });

            modelBuilder.Entity("CentCom.Common.Models.Ban", b =>
                {
                    b.HasOne("CentCom.Common.Models.BanSource", "SourceNavigation")
                        .WithMany("Bans")
                        .HasForeignKey("Source")
                        .HasConstraintName("fk_bans_ban_sources_source_navigation_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SourceNavigation");
                });

            modelBuilder.Entity("CentCom.Common.Models.JobBan", b =>
                {
                    b.HasOne("CentCom.Common.Models.Ban", "BanNavigation")
                        .WithMany("JobBans")
                        .HasForeignKey("BanId")
                        .HasConstraintName("fk_job_bans_bans_ban_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BanNavigation");
                });

            modelBuilder.Entity("CentCom.Common.Models.NotifiedFailure", b =>
                {
                    b.HasOne("CentCom.Common.Models.CheckHistory", "CheckHistory")
                        .WithOne("Notification")
                        .HasForeignKey("CentCom.Common.Models.NotifiedFailure", "CheckHistoryId")
                        .HasConstraintName("fk_notified_failures_check_history_check_history_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CheckHistory");
                });

            modelBuilder.Entity("CentCom.Common.Models.Ban", b =>
                {
                    b.Navigation("JobBans");
                });

            modelBuilder.Entity("CentCom.Common.Models.BanSource", b =>
                {
                    b.Navigation("Bans");
                });

            modelBuilder.Entity("CentCom.Common.Models.CheckHistory", b =>
                {
                    b.Navigation("Notification");
                });
#pragma warning restore 612, 618
        }
    }
}
