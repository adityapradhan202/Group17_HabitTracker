using HabitTrackerAspNetMVCWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerAspNetMVCWebApp.Data
{
    public static class DemoDataSeeder
    {
        private const string DemoPrefix = "Demo - ";
        private const string DemoUserEmail = "demo.user@habittracker.local";
        private const string DemoUserPassword = "Qw123456$";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var admin = await userManager.Users.FirstOrDefaultAsync(u => u.Email == DbInitializer.DefaultAdminEmail);
            if (admin == null)
            {
                throw new InvalidOperationException("Default admin was not found.");
            }

            var demoUser = await userManager.Users.FirstOrDefaultAsync(u => u.Email == DemoUserEmail);
            if (demoUser == null)
            {
                demoUser = new ApplicationUser
                {
                    UserName = DemoUserEmail,
                    Email = DemoUserEmail,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(demoUser, DemoUserPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create demo user: {errors}");
                }
            }

            if (!demoUser.IsActive)
            {
                demoUser.IsActive = true;
                await userManager.UpdateAsync(demoUser);
            }

            if (!await userManager.IsInRoleAsync(demoUser, "User"))
            {
                await userManager.AddToRoleAsync(demoUser, "User");
            }

            await RemoveExistingDemoDataAsync(context);
            await SeedAdminDemoHabitsAsync(context, admin.Id);
            await SeedRegularUserDemoHabitsAsync(context, demoUser.Id);

            await context.SaveChangesAsync();
        }

        private static async Task RemoveExistingDemoDataAsync(ApplicationDbContext context)
        {
            var demoHabits = await context.Habits
                .Include(h => h.HabitLogs)
                .Where(h => h.Title.StartsWith(DemoPrefix))
                .ToListAsync();

            if (demoHabits.Count == 0)
            {
                return;
            }

            var logs = demoHabits.SelectMany(h => h.HabitLogs).ToList();
            context.HabitLogs.RemoveRange(logs);
            context.Habits.RemoveRange(demoHabits);
        }

        private static async Task SeedAdminDemoHabitsAsync(ApplicationDbContext context, string adminUserId)
        {
            var today = DateTime.Today;

            var habits = new List<Habit>
            {
                new Habit
                {
                    Title = $"{DemoPrefix}Morning Stretch",
                    Description = "10-minute mobility routine before work.",
                    Frequency = Frequency.Daily,
                    Status = HabitStatus.Active,
                    KanbanStatus = KanbanStatus.InProgress,
                    StartDate = today.AddDays(-20),
                    EndDate = null,
                    UserId = adminUserId
                },
                new Habit
                {
                    Title = $"{DemoPrefix}Read QA Articles",
                    Description = "Read one testing or automation article.",
                    Frequency = Frequency.Daily,
                    Status = HabitStatus.Active,
                    KanbanStatus = KanbanStatus.Todo,
                    StartDate = today.AddDays(-18),
                    EndDate = null,
                    UserId = adminUserId
                },
                new Habit
                {
                    Title = $"{DemoPrefix}Weekly Project Review",
                    Description = "Review progress for HabitTracker improvements.",
                    Frequency = Frequency.Weekly,
                    Status = HabitStatus.Active,
                    KanbanStatus = KanbanStatus.InProgress,
                    StartDate = today.AddDays(-28),
                    EndDate = null,
                    UserId = adminUserId
                },
                new Habit
                {
                    Title = $"{DemoPrefix}Water Intake",
                    Description = "Track daily hydration goal.",
                    Frequency = Frequency.Daily,
                    Status = HabitStatus.Completed,
                    KanbanStatus = KanbanStatus.Done,
                    StartDate = today.AddDays(-30),
                    EndDate = today.AddDays(-2),
                    UserId = adminUserId
                },
                new Habit
                {
                    Title = $"{DemoPrefix}Prepare Interview Notes",
                    Description = "Write short notes for portfolio presentation.",
                    Frequency = Frequency.Weekly,
                    Status = HabitStatus.Paused,
                    KanbanStatus = KanbanStatus.Todo,
                    StartDate = today.AddDays(-14),
                    EndDate = null,
                    UserId = adminUserId
                }
            };

            context.Habits.AddRange(habits);
            await context.SaveChangesAsync();

            await AddLogsAsync(context, habits[0], 14, HabitLogStatus.Completed, HabitLogStatus.PartiallyCompleted);
            await AddLogsAsync(context, habits[1], 10, HabitLogStatus.Completed, HabitLogStatus.Skipped);
            await AddLogsAsync(context, habits[2], 4, HabitLogStatus.Completed, HabitLogStatus.Completed, 7);
            await AddLogsAsync(context, habits[3], 12, HabitLogStatus.Completed, HabitLogStatus.Completed);
            await AddLogsAsync(context, habits[4], 3, HabitLogStatus.Skipped, HabitLogStatus.PartiallyCompleted, 3);
        }

        private static async Task SeedRegularUserDemoHabitsAsync(ApplicationDbContext context, string userId)
        {
            var today = DateTime.Today;

            var habits = new List<Habit>
            {
                new Habit
                {
                    Title = $"{DemoPrefix}Morning Walk",
                    Description = "Walk outside for at least 20 minutes.",
                    Frequency = Frequency.Daily,
                    Status = HabitStatus.Active,
                    KanbanStatus = KanbanStatus.InProgress,
                    StartDate = today.AddDays(-16),
                    EndDate = null,
                    UserId = userId
                },
                new Habit
                {
                    Title = $"{DemoPrefix}Read 20 Pages",
                    Description = "Read a book every evening.",
                    Frequency = Frequency.Daily,
                    Status = HabitStatus.Active,
                    KanbanStatus = KanbanStatus.Todo,
                    StartDate = today.AddDays(-12),
                    EndDate = null,
                    UserId = userId
                },
                new Habit
                {
                    Title = $"{DemoPrefix}Meal Prep Sunday",
                    Description = "Prepare meals for the week.",
                    Frequency = Frequency.Weekly,
                    Status = HabitStatus.Active,
                    KanbanStatus = KanbanStatus.Done,
                    StartDate = today.AddDays(-35),
                    EndDate = null,
                    UserId = userId
                },
                new Habit
                {
                    Title = $"{DemoPrefix}Sleep Before 11 PM",
                    Description = "Keep a stable evening schedule.",
                    Frequency = Frequency.Daily,
                    Status = HabitStatus.Completed,
                    KanbanStatus = KanbanStatus.Done,
                    StartDate = today.AddDays(-25),
                    EndDate = today.AddDays(-1),
                    UserId = userId
                },
                new Habit
                {
                    Title = $"{DemoPrefix}Gym Training",
                    Description = "3 strength sessions per week.",
                    Frequency = Frequency.Weekly,
                    Status = HabitStatus.Paused,
                    KanbanStatus = KanbanStatus.InProgress,
                    StartDate = today.AddDays(-21),
                    EndDate = null,
                    UserId = userId
                }
            };

            context.Habits.AddRange(habits);
            await context.SaveChangesAsync();

            await AddLogsAsync(context, habits[0], 13, HabitLogStatus.Completed, HabitLogStatus.PartiallyCompleted);
            await AddLogsAsync(context, habits[1], 11, HabitLogStatus.Completed, HabitLogStatus.Skipped);
            await AddLogsAsync(context, habits[2], 5, HabitLogStatus.Completed, HabitLogStatus.Completed, 7);
            await AddLogsAsync(context, habits[3], 10, HabitLogStatus.Completed, HabitLogStatus.Completed);
            await AddLogsAsync(context, habits[4], 4, HabitLogStatus.PartiallyCompleted, HabitLogStatus.Skipped, 7);
        }

        private static async Task AddLogsAsync(
            ApplicationDbContext context,
            Habit habit,
            int entries,
            HabitLogStatus primaryStatus,
            HabitLogStatus alternateStatus,
            int dayStep = 1)
        {
            var logs = new List<HabitLog>();

            for (int i = 0; i < entries; i++)
            {
                logs.Add(new HabitLog
                {
                    HabitId = habit.Id,
                    LogDate = DateTime.Today.AddDays(-(i * dayStep)),
                    Status = i % 4 == 0 ? alternateStatus : primaryStatus
                });
            }

            context.HabitLogs.AddRange(logs);
            await context.SaveChangesAsync();
        }
    }
}