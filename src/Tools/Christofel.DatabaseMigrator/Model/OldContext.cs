using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Christofel.DatabaseMigrator.Model
{
    public partial class OldContext : DbContext
    {
        public OldContext()
        {
        }

        public OldContext(DbContextOptions<OldContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Command> Commands { get; set; }
        public virtual DbSet<CommandPermission> CommandPermissions { get; set; }
        public virtual DbSet<KarmaConfig> KarmaConfigs { get; set; }
        public virtual DbSet<MessageChannel> MessageChannels { get; set; }
        public virtual DbSet<MessageKarmaReport> MessageKarmaReports { get; set; }
        public virtual DbSet<MessageRole> MessageRoles { get; set; }
        public virtual DbSet<ProgrammeRole> ProgrammeRoles { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<YearRole> YearRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Command>(entity =>
            {
                entity.ToTable("commands");

                entity.HasIndex(e => e.DeletedAt, "idx_commands_deleted_at");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Autoremove)
                    .HasColumnType("numeric")
                    .HasColumnName("autoremove");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.Public)
                    .HasColumnType("numeric")
                    .HasColumnName("public");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<CommandPermission>(entity =>
            {
                entity.ToTable("command_permissions");

                entity.HasIndex(e => e.DeletedAt, "idx_command_permissions_deleted_at");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CommandId)
                    .HasColumnType("integer")
                    .HasColumnName("command_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.RoleId)
                    .HasColumnType("text")
                    .HasColumnName("role_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Command)
                    .WithMany(p => p.CommandPermissions)
                    .HasForeignKey(d => d.CommandId);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.CommandPermissions)
                    .HasPrincipalKey(p => p.DiscordId)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<KarmaConfig>(entity =>
            {
                entity.ToTable("karma_configs");

                entity.HasIndex(e => e.DeletedAt, "idx_karma_configs_deleted_at");

                entity.HasIndex(e => e.EmojiId, "idx_karma_configs_emoji_id");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.Effect)
                    .HasColumnType("integer")
                    .HasColumnName("effect");

                entity.Property(e => e.EmojiId)
                    .HasColumnType("text")
                    .HasColumnName("emoji_id");

                entity.Property(e => e.Trigger)
                    .HasColumnType("integer")
                    .HasColumnName("trigger");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<MessageChannel>(entity =>
            {
                entity.ToTable("message_channels");

                entity.HasIndex(e => e.DeletedAt, "idx_message_channels_deleted_at");

                entity.HasIndex(e => e.EmojiId, "idx_message_channels_emoji_id");

                entity.HasIndex(e => e.MessageId, "idx_message_channels_message_id");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ChannelId)
                    .HasColumnType("text")
                    .HasColumnName("channel_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.EmojiId)
                    .HasColumnType("text")
                    .HasColumnName("emoji_id");

                entity.Property(e => e.MessageId)
                    .HasColumnType("text")
                    .HasColumnName("message_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<MessageKarmaReport>(entity =>
            {
                entity.ToTable("message_karma_reports");

                entity.HasIndex(e => e.DeletedAt, "idx_message_karma_reports_deleted_at");

                entity.HasIndex(e => e.KarmaConfigId, "idx_message_karma_reports_karma_config_id");

                entity.HasIndex(e => e.MessageId, "idx_message_karma_reports_message_id");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.KarmaConfigId)
                    .HasColumnType("integer")
                    .HasColumnName("karma_config_id");

                entity.Property(e => e.MessageId)
                    .HasColumnType("text")
                    .HasColumnName("message_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.KarmaConfig)
                    .WithMany(p => p.MessageKarmaReports)
                    .HasForeignKey(d => d.KarmaConfigId);
            });

            modelBuilder.Entity<MessageRole>(entity =>
            {
                entity.ToTable("message_roles");

                entity.HasIndex(e => e.DeletedAt, "idx_message_roles_deleted_at");

                entity.HasIndex(e => e.EmojiId, "idx_message_roles_emoji_id");

                entity.HasIndex(e => e.MessageId, "idx_message_roles_message_id");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.EmojiId)
                    .HasColumnType("text")
                    .HasColumnName("emoji_id");

                entity.Property(e => e.MessageId)
                    .HasColumnType("text")
                    .HasColumnName("message_id");

                entity.Property(e => e.RoleId)
                    .HasColumnType("text")
                    .HasColumnName("role_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.MessageRoles)
                    .HasPrincipalKey(p => p.DiscordId)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<ProgrammeRole>(entity =>
            {
                entity.ToTable("programme_roles");

                entity.HasIndex(e => e.DeletedAt, "idx_programme_roles_deleted_at");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.ProgrammeName)
                    .HasColumnType("text")
                    .HasColumnName("programme_name");

                entity.Property(e => e.RoleId)
                    .HasColumnType("text")
                    .HasColumnName("role_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");

                entity.HasIndex(e => e.DiscordId, "IX_roles_discord_id")
                    .IsUnique();

                entity.HasIndex(e => e.DeletedAt, "idx_roles_deleted_at");

                entity.HasIndex(e => e.DiscordId, "idx_roles_discord_id");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.DiscordId)
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("discord_id");

                entity.Property(e => e.Name)
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(e => e.DiscordId, "idx_users_discord_id");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.AuthCode)
                    .HasColumnType("text")
                    .HasColumnName("auth_code");

                entity.Property(e => e.Authorized)
                    .HasColumnType("numeric")
                    .HasColumnName("authorized");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CtuUsername)
                    .HasColumnType("text")
                    .HasColumnName("ctu_username");

                entity.Property(e => e.DiscordId)
                    .HasColumnType("text")
                    .HasColumnName("discord_id");

                entity.Property(e => e.DiscordUser)
                    .HasColumnType("text")
                    .HasColumnName("discord_user");

                entity.Property(e => e.Duplicity)
                    .HasColumnType("numeric")
                    .HasColumnName("duplicity");

                entity.Property(e => e.DuplicityApproved)
                    .HasColumnType("numeric")
                    .HasColumnName("duplicity_approved");

                entity.Property(e => e.Karma)
                    .HasColumnType("integer")
                    .HasColumnName("karma");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<YearRole>(entity =>
            {
                entity.ToTable("year_roles");

                entity.HasIndex(e => e.DeletedAt, "idx_year_roles_deleted_at");

                entity.HasIndex(e => e.Year, "idx_year_roles_year");

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.DeletedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("deleted_at");

                entity.Property(e => e.RoleId)
                    .HasColumnType("text")
                    .HasColumnName("role_id");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updated_at");

                entity.Property(e => e.Year)
                    .HasColumnType("integer")
                    .HasColumnName("year");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
