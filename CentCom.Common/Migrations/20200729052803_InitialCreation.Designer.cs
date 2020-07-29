﻿// <auto-generated />
using System;
using System.Net;
using CentCom.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CentCom.Common.Migrations
{
    [DbContext(typeof(NpgsqlDbContext))]
    [Migration("20200729052803_InitialCreation")]
    partial class InitialCreation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("CentCom.Common.Models.Ban", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("BanID")
                        .HasColumnType("text");

                    b.Property<long>("BanType")
                        .HasColumnType("bigint");

                    b.Property<string>("BannedBy")
                        .IsRequired()
                        .HasColumnType("character varying(32)")
                        .HasMaxLength(32);

                    b.Property<DateTime>("BannedOn")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long?>("CID")
                        .HasColumnType("bigint");

                    b.Property<string>("CKey")
                        .IsRequired()
                        .HasColumnType("character varying(32)")
                        .HasMaxLength(32);

                    b.Property<DateTime?>("Expires")
                        .HasColumnType("timestamp without time zone");

                    b.Property<IPAddress>("IP")
                        .HasColumnType("inet");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<int>("Source")
                        .HasColumnType("integer");

                    b.Property<string>("UnbannedBy")
                        .HasColumnType("character varying(32)")
                        .HasMaxLength(32);

                    b.HasKey("Id");

                    b.HasIndex("CKey");

                    b.HasIndex("Source");

                    b.ToTable("Bans");
                });

            modelBuilder.Entity("CentCom.Common.Models.BanSource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

                    b.Property<string>("Display")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<long>("RoleplayLevel")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("BanSources");
                });

            modelBuilder.Entity("CentCom.Common.Models.JobBan", b =>
                {
                    b.Property<int>("BanId")
                        .HasColumnType("integer");

                    b.Property<string>("Job")
                        .HasColumnType("text");

                    b.HasKey("BanId", "Job");

                    b.ToTable("JobBans");
                });

            modelBuilder.Entity("CentCom.Common.Models.Ban", b =>
                {
                    b.HasOne("CentCom.Common.Models.BanSource", "SourceNavigation")
                        .WithMany("Bans")
                        .HasForeignKey("Source")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("CentCom.Common.Models.JobBan", b =>
                {
                    b.HasOne("CentCom.Common.Models.Ban", "BanNavigation")
                        .WithMany("JobBans")
                        .HasForeignKey("BanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
