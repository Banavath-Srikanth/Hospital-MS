using HospitalApp.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.Data
{
    /// <summary>
    /// EF Core DbContext — acts as the bridge between C# models and SQL Server database.
    /// Registers all entity sets and configures relationships via Fluent API.
    /// Also seeds Department data and exposes stored-procedure result types.
    /// </summary>
    public class HospitalDbContext : DbContext
    {
        // ─── DbSets (maps to SQL tables) ──────────────────────────────────

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Department> Departments { get; set; }

        // ─── Constructor ──────────────────────────────────────────────────

        public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options) { }

        // ─── Model Configuration (Fluent API) ─────────────────────────────

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Department ─────────────────────────────────────────────────
            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("Departments");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
                entity.Property(d => d.Code).IsRequired().HasMaxLength(10);
                entity.HasIndex(d => d.Code).IsUnique();
            });

            // ── Patient ────────────────────────────────────────────────────
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patients");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.FullName).IsRequired();
                entity.Property(p => p.Age).IsRequired();
                entity.Property(p => p.Gender).IsRequired();
                entity.Property(p => p.Disease).IsRequired();
                entity.Property(p => p.PhoneNumber).IsRequired();
            });

            // ── Doctor ─────────────────────────────────────────────────────
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.ToTable("Doctors");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.BadgeId).IsRequired().HasMaxLength(20);
                entity.HasIndex(d => d.BadgeId).IsUnique();
                entity.Property(d => d.FullName).IsRequired();
                entity.Property(d => d.Specialization).IsRequired();
                entity.Property(d => d.PayrollPosition).HasMaxLength(100).HasDefaultValue("");
                entity.Property(d => d.PhoneNumber).IsRequired();
                entity.Property(d => d.Email).IsRequired();
                entity.HasIndex(d => d.Email).IsUnique();

                // Doctor → Department (optional FK)
                entity.HasOne(d => d.Department)
                      .WithMany(dep => dep.Doctors)
                      .HasForeignKey(d => d.DepartmentId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Appointment ────────────────────────────────────────────────
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("Appointments");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.AppointmentDate).IsRequired();
                entity.Property(a => a.Status).IsRequired();

                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── AppUser ────────────────────────────────────────────────────
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();

                // Optional FK to Patient (only set for Patient-role users)
                entity.HasOne(u => u.Patient)
                      .WithMany()
                      .HasForeignKey(u => u.PatientId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            // ── Seed Data ──────────────────────────────────────────────────
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Cardiology",   Code = "CARD" },
                new Department { Id = 2, Name = "Neurology",    Code = "NEUR" },
                new Department { Id = 3, Name = "Orthopedics",  Code = "ORTH" },
                new Department { Id = 4, Name = "General",      Code = "GEN"  }
            );

            // Seed Doctors (with BadgeId, PayrollPosition, DepartmentId)
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = 1,
                    BadgeId = "EMP-0001",
                    FullName = "Rajesh Kumar",
                    Specialization = "Cardiology",
                    PayrollPosition = "Senior Consultant",
                    PhoneNumber = "9876543210",
                    Email = "rajesh.kumar@hospital.com",
                    IsAvailable = true,
                    DepartmentId = 1
                },
                new Doctor
                {
                    Id = 2,
                    BadgeId = "EMP-0002",
                    FullName = "Priya Sharma",
                    Specialization = "Neurology",
                    PayrollPosition = "Consultant",
                    PhoneNumber = "9876543211",
                    Email = "priya.sharma@hospital.com",
                    IsAvailable = true,
                    DepartmentId = 2
                },
                new Doctor
                {
                    Id = 3,
                    BadgeId = "EMP-0003",
                    FullName = "Amit Verma",
                    Specialization = "Orthopedics",
                    PayrollPosition = "Junior Resident",
                    PhoneNumber = "9876543212",
                    Email = "amit.verma@hospital.com",
                    IsAvailable = true,
                    DepartmentId = 3
                }
            );

            // Seed Patients
            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    Id = 1,
                    FullName = "Suresh Reddy",
                    Age = 45,
                    Gender = "Male",
                    Disease = "Hypertension",
                    PhoneNumber = "9897886541",
                    AdmissionDate = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new Patient
                {
                    Id = 2,
                    FullName = "Anitha Rao",
                    Age = 32,
                    Gender = "Female",
                    Disease = "Diabetes",
                    PhoneNumber = "9000000002",
                    AdmissionDate = new DateTime(2025, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                },
                new Patient
                {
                    Id = 3,
                    FullName = "Kiran Babu",
                    Age = 60,
                    Gender = "Male",
                    Disease = "Arthritis",
                    PhoneNumber = "9000000003",
                    AdmissionDate = new DateTime(2025, 3, 5, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true
                }
            );

            // Seed Appointments
            modelBuilder.Entity<Appointment>().HasData(
                new Appointment
                {
                    Id = 1,
                    PatientId = 1,
                    DoctorId = 1,
                    AppointmentDate = new DateTime(2025, 4, 1, 10, 0, 0, DateTimeKind.Utc),
                    Status = AppointmentStatus.Completed
                },
                new Appointment
                {
                    Id = 2,
                    PatientId = 2,
                    DoctorId = 2,
                    AppointmentDate = new DateTime(2025, 4, 10, 11, 0, 0, DateTimeKind.Utc),
                    Status = AppointmentStatus.Scheduled
                }
            );
        }
    }
}
