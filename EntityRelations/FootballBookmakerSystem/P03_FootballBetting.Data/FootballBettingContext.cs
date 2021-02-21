using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using P03_FootballBetting.Data.Models;
using System;
using System.Collections.Generic;


namespace P03_FootballBetting.Data
{
    public class FootballBettingContext : DbContext
    {
        public FootballBettingContext()
        {

        }

        public FootballBettingContext(DbContextOptions options)
        {

        }

        public virtual DbSet<Bet> Bets { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<PlayerStatistic> PlayerStatistics { get; set; }
        public virtual DbSet<Game> Games { get; set; }
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
        public virtual DbSet<Color> Colors { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<Town> Towns { get; set; }
        public virtual DbSet<Country> Countries { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlServer("Server=.; Database=FootballBookmakerSystem; Integrated Security=true;");
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>(entity => entity.HasKey(e => e.TeamId));

            modelBuilder.Entity<Team>()
                .HasOne(t => t.PrimaryKitColor)
                .WithMany(c => c.PrimaryKitTeams)
                .HasForeignKey(t => t.PrimaryKitColorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.SecondaryKitColor)
                .WithMany(c => c.SecondaryKitTeams)
                .HasForeignKey(t => t.SecondaryKitColorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Town)
                .WithMany(c => c.Teams)
                .HasForeignKey(t => t.TownId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>(entity =>

            entity.Property(e => e.Name)
            .HasMaxLength(120)
            .IsRequired(true)
            .IsUnicode(true)

             );

            modelBuilder.Entity<Team>(entity =>

            entity.Property(e => e.LogoUrl)
            .HasMaxLength(300)
            .IsRequired(true)
            .IsUnicode(false)
             );

            modelBuilder.Entity<Team>(entity =>

             entity.Property(e => e.Initials)
            .HasMaxLength(5)
            .IsRequired(true)
            .IsUnicode(false)
             );

            modelBuilder.Entity<Color>(entity => entity.HasKey(e => e.ColorId));

            modelBuilder.Entity<Color>(entity =>

              entity.Property(e => e.Name)
             .HasMaxLength(25)
             .IsRequired(true)
             .IsUnicode(true)
              );

            modelBuilder.Entity<Town>(entity => entity.HasKey(e => e.TownId));

            modelBuilder.Entity<Town>()
               .HasOne(t => t.Country)
               .WithMany(c => c.Towns)
               .HasForeignKey(t => t.CountryId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Town>(entity =>

             entity.Property(e => e.Name)
            .HasMaxLength(30)
            .IsRequired(true)
            .IsUnicode(true)
             );

            modelBuilder.Entity<Game>(entity => entity.HasKey(e => e.GameId));

            modelBuilder.Entity<Game>()
             .HasOne(t => t.HomeTeam)
             .WithMany(c => c.HomeGames)
             .HasForeignKey(t => t.HomeTeamId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Game>()
           .HasOne(t => t.AwayTeam)
           .WithMany(c => c.AwayGames)
           .HasForeignKey(t => t.AwayTeamId)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bet>(entity => entity.HasKey(e => e.BetId));

            modelBuilder.Entity<Bet>()
            .HasOne(t => t.Game)
            .WithMany(c => c.Bets)
            .HasForeignKey(t => t.GameId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bet>()
           .HasOne(t => t.User)
           .WithMany(c => c.Bets)
           .HasForeignKey(t => t.UserId)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bet>(entity =>

            entity.Property(e => e.Prediction)
           .HasMaxLength(30)
           .IsRequired(true)
           .IsUnicode(true)
            );


            modelBuilder.Entity<Player>(entity => entity.HasKey(e => e.PlayerId));

            modelBuilder.Entity<Player>()
           .HasOne(t => t.Position)
           .WithMany(c => c.Players)
           .HasForeignKey(t => t.PositionId)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Player>()
          .HasOne(t => t.Team)
          .WithMany(c => c.Players)
          .HasForeignKey(t => t.TeamId)
          .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Player>(entity =>

           entity.Property(e => e.Name)
           .HasMaxLength(120)
           .IsRequired(true)
           .IsUnicode(true)

            );

            modelBuilder
                .Entity<PlayerStatistic>(entity => entity.HasKey(c => new { c.PlayerId, c.GameId }));


            modelBuilder.Entity<PlayerStatistic>()
                .HasOne(t => t.Game)
                .WithMany(g => g.PlayerStatistics)
                .HasForeignKey(ps => ps.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PlayerStatistic>()
               .HasOne(t => t.Player)
               .WithMany(g => g.PlayerStatistics)
               .HasForeignKey(ps => ps.PlayerId)
               .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
