using IT_Helpdesk_Platform.Models;
using Microsoft.AspNetCore.Identity;

namespace IT_Helpdesk_Platform.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, HelpdeskContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            // 1. Seed Departments
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { Name = "IT Support" },
                    new Department { Name = "Human Resources" },
                    new Department { Name = "Finance" },
                    new Department { Name = "Marketing" },
                    new Department { Name = "Sales" },
                    new Department { Name = "Engineering" }
                );
                await context.SaveChangesAsync();
            }

            var itDept = context.Departments.FirstOrDefault(d => d.Name == "IT Support");
            var hrDept = context.Departments.FirstOrDefault(d => d.Name == "Human Resources");
            var engDept = context.Departments.FirstOrDefault(d => d.Name == "Engineering");
            if (itDept == null || hrDept == null) return;

            // 2. Seed Ticket Categories (For the Tickets table lookup)
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Hardware" },
                    new Category { Name = "Software" },
                    new Category { Name = "Network & Connectivity" },
                    new Category { Name = "Accounts & Access" }
                );
                await context.SaveChangesAsync();
            }

            var hwCat = context.Categories.First(c => c.Name == "Hardware");
            var swCat = context.Categories.First(c => c.Name == "Software");
            var netCat = context.Categories.First(c => c.Name == "Network & Connectivity");

            // 3. Seed Roles
            string[] roles = { "Admin", "Agent", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                }
            }

            // 4. Seed Users
            if (!userManager.Users.Any())
            {
                var admin = new User { UserName = "admin@helpdesk.com", Email = "admin@helpdesk.com", FirstName = "System", LastName = "Admin", Role = "Admin", DepartmentId = itDept.Id, EmailConfirmed = true };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");

                var agent = new User { UserName = "agent@helpdesk.com", Email = "agent@helpdesk.com", FirstName = "Support", LastName = "Agent", Role = "Agent", DepartmentId = itDept.Id, EmailConfirmed = true };
                await userManager.CreateAsync(agent, "Agent123!");
                await userManager.AddToRoleAsync(agent, "Agent");

                var user = new User { UserName = "user@helpdesk.com", Email = "user@helpdesk.com", FirstName = "Employee", LastName = "User", Role = "User", DepartmentId = hrDept.Id, EmailConfirmed = true };
                await userManager.CreateAsync(user, "User123!");
                await userManager.AddToRoleAsync(user, "User");
            }

            var dbUser = await userManager.FindByEmailAsync("user@helpdesk.com");
            var dbAgent = await userManager.FindByEmailAsync("agent@helpdesk.com");

            // 5. Seed Tickets & Comments
            if (!context.Tickets.Any() && dbUser != null && dbAgent != null)
            {
                var t1 = new Ticket { Title = "Laptop screen flickering", Description = "My secondary monitor works fine, but the laptop screen flashes black every few seconds.", Priority = "High", Status = "In Progress", CreatedAt = DateTime.UtcNow.AddDays(-2), CategoryId = hwCat.Id, RequesterId = dbUser.Id, AssignedAgentId = dbAgent.Id };
                var t2 = new Ticket { Title = "VPN Connection Timed Out", Description = "I cannot connect to the internal network from home. It says 'Gateway Not Found'.", Priority = "Critical", Status = "Open", CreatedAt = DateTime.UtcNow.AddHours(-10), CategoryId = netCat.Id, RequesterId = dbUser.Id };
                var t3 = new Ticket { Title = "Request for Adobe License", Description = "I need Photoshop for the new marketing campaign assets.", Priority = "Low", Status = "Resolved", CreatedAt = DateTime.UtcNow.AddDays(-5), CategoryId = swCat.Id, RequesterId = dbUser.Id, AssignedAgentId = dbAgent.Id };
                var t4 = new Ticket { Title = "Keyboard keys sticking", Description = "The 'E' and 'R' keys are not responding properly after a small coffee spill.", Priority = "Medium", Status = "Closed", CreatedAt = DateTime.UtcNow.AddDays(-10), CategoryId = hwCat.Id, RequesterId = dbUser.Id, AssignedAgentId = dbAgent.Id };

                context.Tickets.AddRange(t1, t2, t3, t4);
                await context.SaveChangesAsync();

                // Add discussion thread to Ticket 1
                context.Comments.AddRange(
                    new Comment { TicketId = t1.Id, UserId = dbAgent.Id, Text = "Hi Employee, please try updating your graphics drivers from the Software Center first.", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                    new Comment { TicketId = t1.Id, UserId = dbUser.Id, Text = "I just finished the update, but the flickering is still happening. Should I bring the laptop in?", CreatedAt = DateTime.UtcNow.AddHours(-5) }
                );
                await context.SaveChangesAsync();
            }

            // 6. Seed Knowledge Articles (Matching your Create.cshtml options)
            if (!context.KnowledgeArticles.Any())
            {
                context.KnowledgeArticles.AddRange(
                    // HARDWARE
                    new KnowledgeArticle { Title = "Standard Laptop Upgrade Policy", Content = "Laptops are eligible for replacement every 36 months. To request an upgrade, please ensure your current device is backed up to OneDrive.", Category = "Hardware", IconClass = "bi-pc-display", ColorClass = "text-success" },
                    new KnowledgeArticle { Title = "Cleaning your peripherals", Content = "Use 70% isopropyl alcohol wipes for keyboards and mice. Do not spray liquids directly onto the equipment.", Category = "Hardware", IconClass = "bi-pc-display", ColorClass = "text-success" },

                    // GEAR
                    new KnowledgeArticle { Title = "Dual Monitor Configuration", Content = "Connect both monitors to the docking station. Right-click desktop > Display Settings > Extend these displays.", Category = "Gear", IconClass = "bi-pc-display", ColorClass = "text-primary" },

                    // SOFTWARE
                    new KnowledgeArticle { Title = "Microsoft Teams Troubleshooting", Content = "If Teams is lagging, try clearing the cache in %appdata%\\Microsoft\\Teams and restarting the application.", Category = "Software", IconClass = "bi-window-stack", ColorClass = "text-primary" },
                    new KnowledgeArticle { Title = "Installing Approved Software", Content = "Open the 'Company Portal' app on your Windows machine to browse and install IT-approved software without admin rights.", Category = "Software", IconClass = "bi-window-stack", ColorClass = "text-primary" },

                    // ACCOUNTS
                    new KnowledgeArticle { Title = "MFA Setup Guide", Content = "Download the Microsoft Authenticator app on your smartphone. Login to your work mail and follow the QR code prompts.", Category = "Accounts", IconClass = "bi-shield-check", ColorClass = "text-warning" },
                    new KnowledgeArticle { Title = "Unlocking your ERP Account", Content = "After 3 failed attempts, your account locks for 30 minutes. You can use the self-service portal to unlock it immediately.", Category = "Accounts", IconClass = "bi-shield-check", ColorClass = "text-warning" },

                    // NETWORK
                    new KnowledgeArticle { Title = "Connecting to Office Wi-Fi", Content = "Use the 'Corporate-Secure' SSID. Your credentials are the same as your Windows login.", Category = "Network", IconClass = "bi-shield-check", ColorClass = "text-info" },
                    new KnowledgeArticle { Title = "Slow VPN Solutions", Content = "Try changing the protocol to TCP in Cisco AnyConnect settings if you are on a weak home connection.", Category = "Network", IconClass = "bi-shield-check", ColorClass = "text-info" },

                    // SECURITY
                    new KnowledgeArticle { Title = "How to spot a Phishing Email", Content = "Check the sender's email address carefully. Hover over links before clicking. IT will never ask for your password via email.", Category = "Security", IconClass = "bi-shield-check", ColorClass = "text-danger" },
                    new KnowledgeArticle { Title = "Working with Sensitive Data", Content = "Always encrypt files containing personal user data (GDPR). Use the built-in 7-Zip encryption tool.", Category = "Security", IconClass = "bi-shield-check", ColorClass = "text-danger" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}