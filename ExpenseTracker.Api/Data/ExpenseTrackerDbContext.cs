using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Data
{
    public class ExpenseTrackerDbContext : DbContext
    {
        public ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options) : base(options)
        {
        }

        public DbSet<Paycheck> Paychecks { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User>Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expense>()
                .HasOne(x => x.Category)
                .WithMany(x => x.Expenses)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Category>().HasData(
new Category { Id = 1, Name = "Food", IsSystemCategory = true },
new Category { Id = 2, Name = "Rent", IsSystemCategory = true },
new Category { Id = 3, Name = "Utilities", IsSystemCategory = true },
new Category { Id = 4, Name = "Transportation", IsSystemCategory = true },
new Category { Id = 5, Name = "Entertainment", IsSystemCategory = true },
new Category { Id = 6, Name = "Medical", IsSystemCategory = true }
);

            base.OnModelCreating(modelBuilder);


        }
    }
}
