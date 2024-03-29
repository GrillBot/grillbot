﻿// <auto-generated />
using System;
using GrillBot.Cache.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrillBot.Cache.Migrations
{
    [DbContext(typeof(GrillBotCacheContext))]
    [Migration("20220520081636_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GrillBot.Cache.Entity.DirectApiMessage", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<DateTime>("ExpireAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("JsonData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DirectApiMessages");
                });

            modelBuilder.Entity("GrillBot.Cache.Entity.MessageIndex", b =>
                {
                    b.Property<string>("MessageId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("AuthorId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("MessageId");

                    b.HasIndex(new[] { "AuthorId" }, "IX_MessageCache_AuthorId");

                    b.HasIndex(new[] { "ChannelId" }, "IX_MessageCache_ChannelId");

                    b.HasIndex(new[] { "GuildId" }, "IX_MessageCache_GuildId");

                    b.ToTable("MessageIndex");
                });
#pragma warning restore 612, 618
        }
    }
}
