using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OMSV1.Domain.Entities.Lectures;

namespace OMSV1.Infrastructure.Configurations;

public class LectureConfiguration : IEntityTypeConfiguration<Lecture>
{
    public void Configure(EntityTypeBuilder<Lecture> builder)
    {
        // Setting the primary key for the Lecture entity
        builder.HasKey(l => l.Id); // Assuming the base class Entity has an Id property

        // Configuring the properties
        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(200); // Adjust length as necessary
                          // Add index on Title
            builder.HasIndex(dd => dd.Title)
                .IsUnique(false); // Set to true if you want a unique index
            

        builder.Property(l => l.Date)
            .IsRequired();
            
        builder.Property(a => a.Note)
            .HasMaxLength(500); 

        builder.Property(l => l.OfficeId)
            .IsRequired();

        builder.Property(l => l.GovernorateId)
            .IsRequired();

        builder.Property(l => l.ProfileId)
            .IsRequired();

        // Set CompanyId and LectureTypeId to be nullable
        builder.Property(l => l.CompanyId)
            .IsRequired(false);  // Company is optional
        builder.Property(l => l.LectureTypeId)
            .IsRequired(false);  // LectureType is optional

        builder.HasOne(l => l.Governorate) 
            .WithMany()  
            .HasForeignKey(l => l.GovernorateId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasOne(l => l.Office)  
            .WithMany()  
            .HasForeignKey(l => l.OfficeId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.HasOne(l => l.Profile)  
            .WithMany()  
            .HasForeignKey(l => l.ProfileId)
            .OnDelete(DeleteBehavior.Restrict); 
        // Optional Company and LectureType relationships (foreign keys are nullable)
        builder.HasOne(l => l.Company)
            .WithMany()  
            .HasForeignKey(l => l.CompanyId)
            .OnDelete(DeleteBehavior.SetNull) // Set null when the related entity is deleted
            .IsRequired(false); // Company is optional

        builder.HasOne(l => l.LectureType)
            .WithMany()  
            .HasForeignKey(l => l.LectureTypeId)
            .OnDelete(DeleteBehavior.SetNull) // Set null when the related entity is deleted
            .IsRequired(false); // LectureType is optional



        // builder.HasMany(a => a.Attachments)
        //     .WithOne()
        //     .HasForeignKey(a => a.EntityId)
        //     .HasPrincipalKey(dd => dd.Id)
        //     .OnDelete(DeleteBehavior.Cascade)
        //     .HasConstraintName("FK_Lecture_Attachments")
        //     .IsRequired(false)
        //     .HasAnnotation("EntityType", OMSV1.Domain.Enums.EntityType.DamagedDevice);

        builder.ToTable("Lectures");
    }
}