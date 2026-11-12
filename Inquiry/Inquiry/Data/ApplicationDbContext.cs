using Microsoft.EntityFrameworkCore;
using Inquiry.Models;

namespace Inquiry.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ResponseTimeOption> ResponseTimeOptions => Set<ResponseTimeOption>();
    public DbSet<OwnerProfile> OwnerProfiles => Set<OwnerProfile>();
    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyAmenity> PropertyAmenities => Set<PropertyAmenity>();
    public DbSet<PropertyPhoto> PropertyPhotos => Set<PropertyPhoto>();
    public DbSet<PropertyRequirement> PropertyRequirements => Set<PropertyRequirement>();
    public DbSet<InquiryStatus> InquiryStatuses => Set<InquiryStatus>();
    public DbSet<Inquiry.Models.Inquiry> Inquiries => Set<Inquiry.Models.Inquiry>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Owner Profiles (1:1 with User)
        modelBuilder.Entity<OwnerProfile>(entity =>
        {
            entity.HasOne(op => op.User)
                  .WithOne(u => u.OwnerProfile)
                  .HasForeignKey<OwnerProfile>(op => op.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Property Amenities (Composite Key)
        modelBuilder.Entity<PropertyAmenity>(entity =>
        {
            entity.HasKey(pa => new { pa.PropertyId, pa.AmenityId });

            entity.HasOne(pa => pa.Property)
                  .WithMany(p => p.PropertyAmenities)
                  .HasForeignKey(pa => pa.PropertyId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pa => pa.Amenity)
                  .WithMany(a => a.PropertyAmenities)
                  .HasForeignKey(pa => pa.AmenityId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Inquiries Relationships
        modelBuilder.Entity<Inquiry.Models.Inquiry>(entity =>
        {
            entity.HasOne(i => i.Property)
                  .WithMany(p => p.Inquiries)
                  .HasForeignKey(i => i.PropertyId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Renter)
                  .WithMany(u => u.Inquiries)
                  .HasForeignKey(i => i.RenterId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Status)
                  .WithMany(s => s.Inquiries)
                  .HasForeignKey(i => i.StatusId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Messages Relationships
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasOne(m => m.Inquiry)
                  .WithMany(i => i.Messages)
                  .HasForeignKey(m => m.InquiryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Sender)
                  .WithMany(u => u.Messages)
                  .HasForeignKey(m => m.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Notifications Relationships
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.Inquiry)
                  .WithMany(i => i.Notifications)
                  .HasForeignKey(n => n.InquiryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(n => n.Message)
                  .WithMany(m => m.Notifications)
                  .HasForeignKey(n => n.MessageId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
