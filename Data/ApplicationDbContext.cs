using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Models;

namespace MotorcycleRepairShop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ตารางอะไหล่
        public DbSet<Part> Parts { get; set; }
        
        // ตารางบริการ
        public DbSet<Service> Services { get; set; }
        
        // ตารางลูกค้า
        public DbSet<Customer> Customers { get; set; }
        
        // ตารางใบสั่งซ่อม
        public DbSet<RepairOrder> RepairOrders { get; set; }
        
        // ตารางรายละเอียดใบสั่งซ่อม
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; } //ລະບົບນັດໝາຍ
        public DbSet<Mechanic> Mechanics { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<InventoryLog> InventoryLogs { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Supplier entity
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasMany(s => s.Parts)
                      .WithOne(p => p.Supplier)
                      .HasForeignKey(p => p.SupplierId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Part entity
            modelBuilder.Entity<Part>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompatibleModels).HasColumnType("text[]");
                entity.HasIndex(e => e.Barcode).IsUnique();
                entity.HasIndex(e => e.PartNumber).IsUnique();
            });

            // Configure Service entity
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.LicensePlate).IsUnique();
            });

            // Configure RepairOrder entity
            modelBuilder.Entity<RepairOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.Property(e => e.Status).HasDefaultValue("Pending");
                
                entity.HasOne(r => r.Customer)
                      .WithMany()
                      .HasForeignKey(r => r.CustomerId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.Mechanic)
                      .WithMany()
                      .HasForeignKey(r => r.MechanicId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure OrderDetail entity
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(d => d.RepairOrder)
                      .WithMany(r => r.OrderDetails)
                      .HasForeignKey(d => d.RepairOrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(d => d.Part)
                      .WithMany()
                      .HasForeignKey(d => d.PartId)
                      .OnDelete(DeleteBehavior.SetNull);
                      
                entity.HasOne(d => d.Service)
                      .WithMany()
                      .HasForeignKey(d => d.ServiceId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Role).HasDefaultValue("Staff");
            });

            // Configure AppSetting entity
            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Key).IsUnique();
                
                // Seed data
                entity.HasData(
                    new AppSetting { Id = 1, Key = "ShopName", Value = "ຮ້ານສ້ອມແປງລົດຈັກ (MotorShop)", DisplayName = "ຊື່ຮ້ານ", Group = "Shop Info" },
                    new AppSetting { Id = 2, Key = "ShopAddress", Value = "123 ຖ.ສຸຂຸມວິທ ເມືອງວຽງຈັນ", DisplayName = "ທີ່ຢູ່ຮ້ານ", Group = "Shop Info" },
                    new AppSetting { Id = 3, Key = "ShopPhone", Value = "081-234-5678", DisplayName = "ເບີໂທລະສັບຮ້ານ", Group = "Shop Info" },
                    new AppSetting { Id = 4, Key = "TaxRatePercentage", Value = "10", DisplayName = "ອັດຕາພາສີ (%)", Group = "Financial" },
                    new AppSetting { Id = 5, Key = "ShopLogo", Value = "", DisplayName = "ໂລໂກ້ຮ້ານ (Base64)", Group = "Shop Info" },
                    new AppSetting { Id = 6, Key = "ReceiptFooter", Value = "-- ຂໍຂອບໃຈທີ່ໃຊ້ບໍລິການ --", DisplayName = "ຂໍ້ຄວາມທ້າຍບິນ", Group = "Shop Info" }
                );
            });
        }
    }
}