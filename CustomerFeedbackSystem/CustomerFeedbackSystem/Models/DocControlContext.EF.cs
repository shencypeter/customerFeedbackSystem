using Microsoft.EntityFrameworkCore;

namespace CustomerFeedbackSystem.Models
{

    /// <summary>
    /// 這一半用來寫 EF Core 實體類別
    /// </summary>
    public partial class DocControlContext : DbContext
    {

        IConfiguration _config;

        public DocControlContext(DbContextOptions<DocControlContext> options)
            : base(options)
        {
        }

        public DocControlContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _config.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }


        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<UserRole> UserRoles { get; set; }

        public virtual DbSet<Bulletin> Bulletins { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }

        public virtual DbSet<FeedbackResponse> FeedbackResponses { get; set; }

        public virtual DbSet<FeedbackAttachment> FeedbackAttachments { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
            .ToTable("user")
            .HasKey(u => u.Id);

            modelBuilder.Entity<Role>()
                .ToTable("role")
                .HasKey(r => r.Id);

            modelBuilder.Entity<UserRole>()
                .ToTable("user_role")
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);


            modelBuilder.Entity<Bulletin>(entity =>
            {
                entity.ToTable("bulletin");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("code");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value");

                entity.Property(e => e.ValueType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("value_type");
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("Feedback", "dbo");

                entity.HasKey(e => e.FeedbackId);

                entity.Property(e => e.FeedbackNo)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.SubmittedByRole)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.SubmittedByName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.SubmittedByEmail)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.SubmittedOrg)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Urgency)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Content)
                    .IsRequired();

                // Dates: no mapping needed if names match
                entity.Property(e => e.SubmittedDate);
                entity.Property(e => e.ExpectedFinishDate);
                entity.Property(e => e.ClosedDate);
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedAt);

                entity.HasMany(e => e.Responses)
                    .WithOne(r => r.Feedback)
                    .HasForeignKey(r => r.FeedbackId);

                entity.HasMany(e => e.Attachments)
                    .WithOne(a => a.Feedback)
                    .HasForeignKey(a => a.FeedbackId);
            });


            modelBuilder.Entity<FeedbackResponse>(entity =>
            {
                entity.ToTable("FeedbackResponse", "dbo");

                entity.HasKey(e => e.ResponseId);

                entity.Property(e => e.FeedbackId);

                entity.Property(e => e.ResponderRole)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.ResponderName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ResponderEmail)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ResponderOrg)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.StatusAfterResponse)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Content)
                    .IsRequired();

                entity.Property(e => e.ResponseDate);
                entity.Property(e => e.CreatedAt);

                entity.HasMany(e => e.Attachments)
                    .WithOne(a => a.FeedbackResponse)
                    .HasForeignKey(a => a.ResponseId);
            });

            modelBuilder.Entity<FeedbackAttachment>(entity =>
            {
                entity.ToTable("FeedbackAttachment", "dbo");

                entity.HasKey(e => e.AttachmentId);

                entity.Property(e => e.FeedbackId);
                entity.Property(e => e.ResponseId);

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FileExtension)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.FileSizeBytes);

                entity.Property(e => e.StorageKey)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.UploadedByRole)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.UploadedByName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.UploadedAt);
            });


        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    }
}
