using Microsoft.EntityFrameworkCore;
using P01_HospitalDatabase.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace P01_HospitalDatabase.Data
{
    public class HospitalContext : DbContext
    {
        public HospitalContext()
        {
        }

        public HospitalContext(DbContextOptionsBuilder optionsBuilder)
            : base ()
        {        
        }

        DbSet<Patient> Patients { get; set; }
        DbSet<Visitation> Visitations { get; set; }
        DbSet<Diagnose> Diagnoses { get; set; }
        DbSet<Medicament> Medicaments { get; set; }
        DbSet<Doctor> Doctors { get; set; }
        DbSet<PatientMedicament> PatientMedicaments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlServer("Server=.;Integrated Security=true;Database=HospitalDatabase");
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PatientMedicament>(entity => {

                entity
                    .HasKey(c => new { c.PatientId, c.MedicamentId });

                entity
                    .HasOne(p => p.Patient)
                    .WithMany(x => x.Prescriptions)
                    .HasForeignKey(pm => pm.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                    .HasOne(p => p.Medicament)
                    .WithMany(x => x.Prescriptions)
                    .HasForeignKey(pm => pm.MedicamentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Medicament>(entity => {

                entity.HasKey(e => e.MedicamentId);

                entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsRequired(true)
                .IsUnicode(true);
            });

            modelBuilder.Entity<Diagnose>(entity => {

                entity.HasKey(e => e.DiagnoseId);

                entity.HasOne(e => e.Patient)
                .WithMany(p => p.Diagnoses)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsRequired(true)
                .IsUnicode(true);

                entity.Property(e => e.Comments)
                .HasMaxLength(250)
                .IsUnicode(true);
            });

            modelBuilder.Entity<Visitation>(entity =>
            {
                entity
                    .HasKey(v => v.VisitationId);

                entity
                    .HasOne(e => e.Patient)
                    .WithMany(p => p.Visitations)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity
                   .HasOne(e => e.Doctor)
                   .WithMany(p => p.Visitations)
                   .HasForeignKey(e => e.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

                entity
                    .Property(e => e.Comments)
                    .HasMaxLength(250)
                    .IsUnicode(true);
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity
                    .HasKey(v => v.PatientId);

                entity
                    .Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .IsUnicode(true);

                entity
                  .Property(e => e.LastName)
                  .HasMaxLength(50)
                  .IsUnicode(true);

                entity
                  .Property(e => e.Address)
                  .HasMaxLength(250)
                  .IsUnicode(true);

                entity
                 .Property(e => e.Email)
                 .HasMaxLength(80)
                 .IsUnicode(false);
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity
                    .HasKey(v => v.DoctorId);

                entity
                    .Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity
                  .Property(e => e.Specialty)
                  .HasMaxLength(100)
                  .IsUnicode(true);
            });
        }
    }
}
