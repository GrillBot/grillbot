﻿// <auto-generated />
using System;
using System.Collections.Generic;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrillBot.Database.Migrations
{
    [DbContext(typeof(GrillBotContext))]
    [Migration("20240202092511_GuildUser_IsInGuild")]
    partial class GuildUser_IsInGuild
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GrillBot.Database.Entity.ApiClient", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<List<string>>("AllowedMethods")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<bool>("Disabled")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastUse")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int>("UseCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ApiClients");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.AutoReplyItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<long>("Flags")
                        .HasColumnType("bigint");

                    b.Property<string>("Reply")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Template")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("AutoReplies");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.EmoteStatisticItem", b =>
                {
                    b.Property<string>("EmoteId")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("UserId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<DateTime>("FirstOccurence")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsEmoteSupported")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastOccurence")
                        .HasColumnType("timestamp without time zone");

                    b.Property<long>("UseCount")
                        .HasColumnType("bigint");

                    b.HasKey("EmoteId", "UserId", "GuildId");

                    b.HasIndex("GuildId", "UserId");

                    b.ToTable("Emotes");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.EmoteSuggestion", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<bool?>("ApprovedForVote")
                        .HasColumnType("boolean");

                    b.Property<bool>("CommunityApproved")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<int>("DownVotes")
                        .HasColumnType("integer");

                    b.Property<string>("EmoteName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Filename")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("FromUserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<byte[]>("ImageData")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("SuggestionMessageId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("UpVotes")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("VoteEndsAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("VoteFinished")
                        .HasColumnType("boolean");

                    b.Property<string>("VoteMessageId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Id");

                    b.HasIndex("GuildId", "FromUserId");

                    b.ToTable("EmoteSuggestions");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Guild", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("AdminChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("BoosterRoleId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("BotRoomChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("EmoteSuggestionChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<DateTime?>("EmoteSuggestionsFrom")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("EmoteSuggestionsTo")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("MuteRoleId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("VoteChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildChannel", b =>
                {
                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("ChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("ChannelType")
                        .HasColumnType("integer");

                    b.Property<long>("Flags")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("ParentChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("PinCount")
                        .HasColumnType("integer");

                    b.Property<int>("RolePermissionsCount")
                        .HasColumnType("integer");

                    b.Property<int>("UserPermissionsCount")
                        .HasColumnType("integer");

                    b.HasKey("GuildId", "ChannelId");

                    b.HasIndex("GuildId", "ParentChannelId");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildUser", b =>
                {
                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("UserId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<long>("GivenReactions")
                        .HasColumnType("bigint");

                    b.Property<bool>("IsInGuild")
                        .HasColumnType("boolean");

                    b.Property<string>("Nickname")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<long>("ObtainedReactions")
                        .HasColumnType("bigint");

                    b.Property<string>("UsedInviteCode")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("GuildId", "UserId");

                    b.HasIndex("UsedInviteCode");

                    b.HasIndex("UserId");

                    b.ToTable("GuildUsers");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildUserChannel", b =>
                {
                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("ChannelId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("UserId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<long>("Count")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("FirstMessageAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("LastMessageAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("GuildId", "ChannelId", "UserId");

                    b.HasIndex("UserId");

                    b.HasIndex("GuildId", "UserId");

                    b.ToTable("UserChannels");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Invite", b =>
                {
                    b.Property<string>("Code")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("CreatorId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Code");

                    b.HasIndex("GuildId", "CreatorId");

                    b.ToTable("Invites");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Nickname", b =>
                {
                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("UserId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("NicknameValue")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.HasKey("GuildId", "UserId", "Id");

                    b.ToTable("Nicknames");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.RemindMessage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("At")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("FromUserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OriginalMessageId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("Postpone")
                        .HasColumnType("integer");

                    b.Property<string>("RemindMessageId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("ToUserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Id");

                    b.HasIndex("FromUserId");

                    b.HasIndex("ToUserId");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.SearchItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("MessageContent")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("GuildId", "ChannelId");

                    b.ToTable("SearchItems");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.SelfunverifyKeepable", b =>
                {
                    b.Property<string>("GroupName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("GroupName", "Name");

                    b.ToTable("SelfunverifyKeepables");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Unverify", b =>
                {
                    b.Property<string>("GuildId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("UserId")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<List<GuildChannelOverride>>("Channels")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("EndAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<List<string>>("Roles")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<long>("SetOperationId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("StartAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("GuildId", "UserId");

                    b.HasIndex("SetOperationId")
                        .IsUnique();

                    b.ToTable("Unverifies");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.UnverifyLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FromUserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("Operation")
                        .HasColumnType("integer");

                    b.Property<string>("ToUserId")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("Id");

                    b.HasIndex("GuildId", "FromUserId");

                    b.HasIndex("GuildId", "ToUserId");

                    b.ToTable("UnverifyLogs");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.User", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("AvatarUrl")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Flags")
                        .HasColumnType("integer");

                    b.Property<string>("GlobalAlias")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<string>("Language")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("SelfUnverifyMinimalTime")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.EmoteStatisticItem", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("EmoteStatistics")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "User")
                        .WithMany("EmoteStatistics")
                        .HasForeignKey("GuildId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.EmoteSuggestion", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "FromUser")
                        .WithMany()
                        .HasForeignKey("GuildId", "FromUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FromUser");

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildChannel", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("Channels")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildChannel", "ParentChannel")
                        .WithMany()
                        .HasForeignKey("GuildId", "ParentChannelId");

                    b.Navigation("Guild");

                    b.Navigation("ParentChannel");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildUser", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("Users")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.Invite", "UsedInvite")
                        .WithMany("UsedUsers")
                        .HasForeignKey("UsedInviteCode");

                    b.HasOne("GrillBot.Database.Entity.User", "User")
                        .WithMany("Guilds")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("UsedInvite");

                    b.Navigation("User");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildUserChannel", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.User", null)
                        .WithMany("Channels")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildChannel", "Channel")
                        .WithMany("Users")
                        .HasForeignKey("GuildId", "ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "User")
                        .WithMany("Channels")
                        .HasForeignKey("GuildId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Channel");

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Invite", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("Invites")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "Creator")
                        .WithMany("CreatedInvites")
                        .HasForeignKey("GuildId", "CreatorId");

                    b.Navigation("Creator");

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Nickname", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.GuildUser", "User")
                        .WithMany("Nicknames")
                        .HasForeignKey("GuildId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.RemindMessage", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.User", "FromUser")
                        .WithMany("OutgoingReminders")
                        .HasForeignKey("FromUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.User", "ToUser")
                        .WithMany("IncomingReminders")
                        .HasForeignKey("ToUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FromUser");

                    b.Navigation("ToUser");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.SearchItem", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("Searches")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.User", "User")
                        .WithMany("SearchItems")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildChannel", "Channel")
                        .WithMany("SearchItems")
                        .HasForeignKey("GuildId", "ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Channel");

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Unverify", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("Unverifies")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.UnverifyLog", "UnverifyLog")
                        .WithOne("Unverify")
                        .HasForeignKey("GrillBot.Database.Entity.Unverify", "SetOperationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "GuildUser")
                        .WithOne("Unverify")
                        .HasForeignKey("GrillBot.Database.Entity.Unverify", "GuildId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("GuildUser");

                    b.Navigation("UnverifyLog");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.UnverifyLog", b =>
                {
                    b.HasOne("GrillBot.Database.Entity.Guild", "Guild")
                        .WithMany("UnverifyLogs")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "FromUser")
                        .WithMany()
                        .HasForeignKey("GuildId", "FromUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GrillBot.Database.Entity.GuildUser", "ToUser")
                        .WithMany()
                        .HasForeignKey("GuildId", "ToUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FromUser");

                    b.Navigation("Guild");

                    b.Navigation("ToUser");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Guild", b =>
                {
                    b.Navigation("Channels");

                    b.Navigation("EmoteStatistics");

                    b.Navigation("Invites");

                    b.Navigation("Searches");

                    b.Navigation("Unverifies");

                    b.Navigation("UnverifyLogs");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildChannel", b =>
                {
                    b.Navigation("SearchItems");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.GuildUser", b =>
                {
                    b.Navigation("Channels");

                    b.Navigation("CreatedInvites");

                    b.Navigation("EmoteStatistics");

                    b.Navigation("Nicknames");

                    b.Navigation("Unverify");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.Invite", b =>
                {
                    b.Navigation("UsedUsers");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.UnverifyLog", b =>
                {
                    b.Navigation("Unverify");
                });

            modelBuilder.Entity("GrillBot.Database.Entity.User", b =>
                {
                    b.Navigation("Channels");

                    b.Navigation("Guilds");

                    b.Navigation("IncomingReminders");

                    b.Navigation("OutgoingReminders");

                    b.Navigation("SearchItems");
                });
#pragma warning restore 612, 618
        }
    }
}
