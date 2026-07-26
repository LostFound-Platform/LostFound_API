using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class BackToMeDbContext : DbContext
    {
        // Get DbContext from DI
        #region Constructor
        public BackToMeDbContext(DbContextOptions<BackToMeDbContext> options) : base(options)
        {
        }
        #endregion

        #region Entities
        public DbSet<Users> Users { get; set; }
        public DbSet<Student> Student { get; set; }
        public DbSet<Posts> Posts { get; set; }
        public DbSet<CategoryPost> CategoryPost { get; set; }
        public DbSet<Match> Match { get; set; }
        public DbSet<VerificationCode> VerificationCode { get; set; }
        public DbSet<TransferRequests> TransferRequests { get; set; }
        public DbSet<PickUpRequest> PickUpRequest { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<MessageChat> MessageChat { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<InstitutionRequest> InstitutionRequests { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageChat>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notifications>()
                .HasOne(m => m.PostOriginal)
                .WithMany()
                .HasForeignKey(m => m.PostOriginalId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent multiple cascade paths issue in SQL Server

            modelBuilder.Entity<Notifications>()
                .HasOne(m => m.PostMatched)
                .WithMany()
                .HasForeignKey(m => m.PostMatchedId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent multiple cascade paths issue in SQL Server

            modelBuilder.Entity<Match>()
                .HasOne(m => m.LostPost)
                .WithMany()
                .HasForeignKey(m => m.LostPostId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent multiple cascade paths issue in SQL Server

            modelBuilder.Entity<Match>()
                .HasOne(m => m.FoundPost)
                .WithMany()
                .HasForeignKey(m => m.FoundPostId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent multiple cascade paths issue in SQL Server

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.UserA)
                .WithMany()
                .HasForeignKey(c => c.UserAId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent multiple cascade paths issue in SQL Server

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.UserB)
                .WithMany()
                .HasForeignKey(c => c.UserBId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent multiple cascade paths issue in SQL Server

            // Allow converting enum to string
            modelBuilder.Entity<Users>()
                .Property(u => u.Role)
                .HasConversion<string>();

            // Allow converting enum to string
            modelBuilder.Entity<Posts>()
                .Property(u => u.TypePost)
                .HasConversion<string>();

            // Allow converting enum to string
            modelBuilder.Entity<TransferRequests>()
                .Property(u => u.Status)
                .HasConversion<string>();

            // Allow converting enum to string
            modelBuilder.Entity<InstitutionRequest>()
                .Property(u => u.Status)
                .HasConversion<string>();

            // Allow converting enum to string
            modelBuilder.Entity<PickUpRequest>()
                .Property(u => u.Status)
                .HasConversion<string>();

            // Allow converting enum to string
            modelBuilder.Entity<Notifications>()
                .Property(u => u.NotificationType)
                .HasConversion<string>();

            // Add admin user sample data
            modelBuilder.Entity<Users>().HasData(new Users
            {
                UserId = new Random().Next(),
                FirstName = "System",
                LastName = "Admin",
                Password = "$2a$11$92uWViLQUKTIVIADFxhzqe39tDMoLWJX5e1FyXaeedfrq5CoMAGQ6",
                CreatedAt = DateTime.Now,
                IsActive = true,
                IsVerifiedEmail = true,
                IsAgreedToTerms = true,
                Email = "mycampuslostfound@gmail.com",
                PickImage1 = "1",
                PickImage2 = "12",
                Role = Role.SystemAdmin,
                Avatar = "avatar_CV.jpg",
                DateOfBirth = new DateTime(2007, 01, 30),
            });

            // Add category post sample data
            modelBuilder.Entity<CategoryPost>().HasData(
                new CategoryPost
                {
                    CategoryPostId = new Random().Next(),
                    CategoryPostName = "IPhone",
                    CreatedAt = DateTime.Now
                },
                new CategoryPost
                {
                    CategoryPostId = new Random().Next(),
                    CategoryPostName = "IPad",
                    CreatedAt = DateTime.Now
                },
                new CategoryPost
                {
                    CategoryPostId = new Random().Next(),
                    CategoryPostName = "Chromebook",
                    CreatedAt = DateTime.Now
                },
                new CategoryPost
                {
                    CategoryPostId = new Random().Next(),
                    CategoryPostName = "Earbuds",
                    CreatedAt = DateTime.Now
                },
                new CategoryPost
                {
                    CategoryPostId = new Random().Next(),
                    CategoryPostName = "Wallet",
                    CreatedAt = DateTime.Now
                },
                new CategoryPost
                {
                    CategoryPostId = new Random().Next(),
                    CategoryPostName = "Charger",
                    CreatedAt = DateTime.Now
                }
            );
        }
    }
}
