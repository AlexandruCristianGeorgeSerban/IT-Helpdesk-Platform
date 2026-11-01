using IT_Helpdesk_Platform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IT_Helpdesk_Platform.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, HelpdeskContext context, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            // 1. SEED DEPARTMENTS
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { Name = "IT Support" },
                    new Department { Name = "Network Support" },
                    new Department { Name = "Hardware & Devices" },
                    new Department { Name = "Software & Apps" },
                    new Department { Name = "System Administration" },
                    new Department { Name = "Human Resources" }
                );
                await context.SaveChangesAsync();
            }

            var deptMap = await context.Departments.ToDictionaryAsync(d => d.Name!, d => d.Id);

            // 2. SEED CATEGORIES
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Hardware" },
                    new Category { Name = "Network" },
                    new Category { Name = "Software" },
                    new Category { Name = "Access & Security" },
                    new Category { Name = "General Inquiry" }
                );
                await context.SaveChangesAsync();
            }

            var catMap = await context.Categories.ToDictionaryAsync(c => c.Name!, c => c.Id);

            // 3. SEED ROLES
            string[] roles = { "Admin", "Agent", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                }
            }

            // 4. SEED USERS (Admin, 3 Support Agents for Load Balancing, 2 Employees)
            if (!userManager.Users.Any())
            {
                var usersToSeed = new[]
                {
                    new
                    {
                        User = new User
                        {
                            UserName = "admin@helpdesk.com",
                            Email = "admin@helpdesk.com",
                            FirstName = "System",
                            LastName = "Administrator",
                            Role = "Admin",
                            DepartmentId = deptMap["System Administration"],
                            PhoneNumber = "0720100100",
                            EmailNotificationsEnabled = true,
                            PushNotificationsEnabled = true,
                            ProfilePicturePath = "/uploads/avatars/default_admin.png",
                            EmailConfirmed = true
                        },
                        Password = "Admin123!",
                        Role = "Admin"
                    },
                    // Agent 1: Primary Hardware/Helpdesk Agent (Higher initial workload)
                    new
                    {
                        User = new User
                        {
                            UserName = "agent@helpdesk.com",
                            Email = "agent@helpdesk.com",
                            FirstName = "Support",
                            LastName = "Agent",
                            Role = "Agent",
                            DepartmentId = deptMap["Hardware & Devices"],
                            PhoneNumber = "0720100201",
                            EmailNotificationsEnabled = true,
                            PushNotificationsEnabled = true,
                            ProfilePicturePath = "/uploads/avatars/default_agent.png",
                            EmailConfirmed = true
                        },
                        Password = "Agent123!",
                        Role = "Agent"
                    },
                    // Agent 2: Network Support Agent (Medium initial workload)
                    new
                    {
                        User = new User
                        {
                            UserName = "andrei.agent@helpdesk.com",
                            Email = "andrei.agent@helpdesk.com",
                            FirstName = "Andrei",
                            LastName = "Ionescu",
                            Role = "Agent",
                            DepartmentId = deptMap["Network Support"],
                            PhoneNumber = "0720100202",
                            EmailNotificationsEnabled = true,
                            PushNotificationsEnabled = true,
                            ProfilePicturePath = "/uploads/avatars/default_agent.png",
                            EmailConfirmed = true
                        },
                        Password = "Agent123!",
                        Role = "Agent"
                    },
                    // Agent 3: Software & Apps Agent (Lowest workload = 0, picked first by smart load balancer!)
                    new
                    {
                        User = new User
                        {
                            UserName = "mihai.agent@helpdesk.com",
                            Email = "mihai.agent@helpdesk.com",
                            FirstName = "Mihai",
                            LastName = "Popescu",
                            Role = "Agent",
                            DepartmentId = deptMap["Software & Apps"],
                            PhoneNumber = "0720100203",
                            EmailNotificationsEnabled = true,
                            PushNotificationsEnabled = false,
                            ProfilePicturePath = "/uploads/avatars/default_agent.png",
                            EmailConfirmed = true
                        },
                        Password = "Agent123!",
                        Role = "Agent"
                    },
                    // Employee 1
                    new
                    {
                        User = new User
                        {
                            UserName = "user@helpdesk.com",
                            Email = "user@helpdesk.com",
                            FirstName = "Employee",
                            LastName = "User",
                            Role = "User",
                            DepartmentId = deptMap["Human Resources"],
                            PhoneNumber = "0720100301",
                            EmailNotificationsEnabled = true,
                            PushNotificationsEnabled = true,
                            ProfilePicturePath = "/uploads/avatars/default_user.png",
                            EmailConfirmed = true
                        },
                        Password = "User123!",
                        Role = "User"
                    },
                    // Employee 2
                    new
                    {
                        User = new User
                        {
                            UserName = "diana.user@helpdesk.com",
                            Email = "diana.user@helpdesk.com",
                            FirstName = "Diana",
                            LastName = "Marin",
                            Role = "User",
                            DepartmentId = deptMap["Human Resources"],
                            PhoneNumber = "0720100302",
                            EmailNotificationsEnabled = true,
                            PushNotificationsEnabled = false,
                            ProfilePicturePath = "/uploads/avatars/default_user.png",
                            EmailConfirmed = true
                        },
                        Password = "User123!",
                        Role = "User"
                    }
                };

                foreach (var item in usersToSeed)
                {
                    var result = await userManager.CreateAsync(item.User, item.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(item.User, item.Role);
                    }
                }
            }

            // 5. SEED KNOWLEDGE BASE ARTICLES (For TF-IDF & Cosine Similarity Recommendations)
            if (!context.KnowledgeArticles.Any())
            {
                var articles = new List<KnowledgeArticle>
                {
                    new()
                    {
                        Title = "Fixing Laptop Screen Flickering and Black Display Flashes",
                        Category = "Hardware",
                        IconClass = "bi-display",
                        ColorClass = "text-primary",
                        Content = "If your laptop screen flickers or flashes black every few seconds while a secondary monitor works fine, open Software Center, update the Intel/NVIDIA graphics drivers, and set the internal display refresh rate to 60Hz in Display Settings."
                    },
                    new()
                    {
                        Title = "Troubleshooting Company VPN Connection Timed Out & Auth Errors",
                        Category = "Network",
                        IconClass = "bi-shield-lock",
                        ColorClass = "text-danger",
                        Content = "When the VPN client reports a connection timeout or authentication error after a password reset, clear cached credentials in Windows Credential Manager, restart the Cisco AnyConnect / OpenVPN service, and re-authenticate using your new domain password and MFA token."
                    },
                    new()
                    {
                        Title = "How to Reset Locked Domain Account and Expired Windows Password",
                        Category = "Access & Security",
                        IconClass = "bi-key",
                        ColorClass = "text-warning",
                        Content = "If your workstation account is locked out after multiple failed login attempts or your password expired, use the Self-Service Password Portal or contact System Administration to unlock Active Directory credentials."
                    },
                    new()
                    {
                        Title = "Resolving Outlook Email Sync Issues and Attachment Crashes",
                        Category = "Software",
                        IconClass = "bi-envelope-exclamation",
                        ColorClass = "text-info",
                        Content = "If Microsoft Outlook fails to synchronize incoming emails, hangs on 'Trying to connect', or crashes when opening PDF attachments, start Outlook in Safe Mode (outlook.exe /safe), disable faulty COM add-ins, and rebuild the local .OST data file."
                    },
                    new()
                    {
                        Title = "Office Wi-Fi Dropping Signal and Slow Wireless Network Speeds",
                        Category = "Network",
                        IconClass = "bi-wifi",
                        ColorClass = "text-success",
                        Content = "For intermittent wireless disconnects on the corporate Wi-Fi SSID, forget the network profile, update the WLAN adapter driver, and ensure 802.1X certificate authentication is enabled on the 5GHz band."
                    },
                    new()
                    {
                        Title = "Requesting Software Installation & Development Licenses (Visual Studio, SQL)",
                        Category = "Software",
                        IconClass = "bi-code-square",
                        ColorClass = "text-secondary",
                        Content = "Standard applications can be installed directly from the company Software Center without administrator rights. For licensed tools like Visual Studio Enterprise, JetBrains, or SQL Server Management Studio, submit a Software ticket with manager approval."
                    },
                    new()
                    {
                        Title = "External USB Keyboard, Mouse & Docking Station Not Recognized",
                        Category = "Hardware",
                        IconClass = "bi-keyboard",
                        ColorClass = "text-primary",
                        Content = "Disconnect the USB-C docking station from power for 30 seconds, plug peripherals directly into the laptop USB ports to isolate hardware faults, and install the latest Thunderbolt firmware update."
                    },
                    new()
                    {
                        Title = "Shared Network Drive & HR Folder Permission Requests",
                        Category = "Access & Security",
                        IconClass = "bi-folder-check",
                        ColorClass = "text-warning",
                        Content = "To gain read or write access to department shared folders (e.g., \\\\fileserver\\HR), specify the exact network path and your department manager in the ticket description so security groups can be updated."
                    }
                };

                context.KnowledgeArticles.AddRange(articles);
                await context.SaveChangesAsync();
            }

            // 6. SEED TICKETS (Active Tickets + Historical Resolved Tickets for TF-IDF & ML.NET)
            if (!context.Tickets.Any())
            {
                var agent1 = await userManager.FindByEmailAsync("agent@helpdesk.com");
                var agent2 = await userManager.FindByEmailAsync("andrei.agent@helpdesk.com");
                var agent3 = await userManager.FindByEmailAsync("mihai.agent@helpdesk.com");
                var employee = await userManager.FindByEmailAsync("user@helpdesk.com");
                var client2 = await userManager.FindByEmailAsync("diana.user@helpdesk.com");

                if (agent1 != null && agent2 != null && agent3 != null && employee != null && client2 != null)
                {
                    var now = DateTime.UtcNow;

                    var tickets = new List<Ticket>
                    {
                        // --- A. ACTIVE TICKETS (Dashboard display & Workload scoring) ---
                        // Ticket #1: In Progress, High Priority -> Agent 1 (Workload score +3)
                        new()
                        {
                            Title = "Laptop screen flickering",
                            Description = "My secondary monitor works fine, but the laptop screen flashes black every few seconds.",
                            Status = "In Progress",
                            Priority = "High",
                            CreatedAt = now.AddDays(-2),
                            DueDate = now.AddHours(6),
                            CategoryId = catMap["Hardware"],
                            RequesterId = employee.Id,
                            AssignedAgentId = agent1.Id
                        },
                        // Ticket #2: Open, High Priority -> Agent 1 (Workload score +3 => Total Agent 1 = 6)
                        new()
                        {
                            Title = "VPN Connection Timed Out",
                            Description = "After resetting my password, I cannot connect to the company VPN from home. The client times out during authentication.",
                            Status = "Open",
                            Priority = "High",
                            CreatedAt = now.AddDays(-1),
                            DueDate = now.AddHours(2),
                            CategoryId = catMap["Network"],
                            RequesterId = employee.Id,
                            AssignedAgentId = agent1.Id
                        },
                        // Ticket #3: Open, Medium Priority -> Agent 2 (Workload score +2 => Total Agent 2 = 2)
                        // DueDate in the past => SLA Breached indicator for Dashboard!
                        new()
                        {
                            Title = "SLA Breach Monitoring Test Ticket",
                            Description = "Urgent network switch configuration required in building B conference room.",
                            Status = "Open",
                            Priority = "Medium",
                            CreatedAt = now.AddDays(-3),
                            DueDate = now.AddHours(-5),
                            CategoryId = catMap["Network"],
                            RequesterId = client2.Id,
                            AssignedAgentId = agent2.Id
                        },

                        // --- B. HISTORICAL RESOLVED TICKETS (For TF-IDF "Similar Cases" & ML.NET Training) ---
                        new()
                        {
                            Title = "Display flashes black after Windows graphics driver update",
                            Description = "Internal laptop monitor flickers and goes black intermittently while external HDMI screen works properly. Resolved by rolling back display driver and setting 60Hz.",
                            Status = "Resolved",
                            Priority = "High",
                            CreatedAt = now.AddDays(-14),
                            DueDate = now.AddDays(-13),
                            CategoryId = catMap["Hardware"],
                            RequesterId = client2.Id,
                            AssignedAgentId = agent1.Id
                        },
                        new()
                        {
                            Title = "Laptop monitor screen blinking on battery power",
                            Description = "Screen flashes black every few seconds when unplugged. Fixed by updating integrated GPU drivers from Software Center.",
                            Status = "Resolved",
                            Priority = "Medium",
                            CreatedAt = now.AddDays(-12),
                            DueDate = now.AddDays(-11),
                            CategoryId = catMap["Hardware"],
                            RequesterId = employee.Id,
                            AssignedAgentId = agent1.Id
                        },
                        new()
                        {
                            Title = "Cannot connect to corporate VPN after domain password change",
                            Description = "VPN client reports authentication failure and connection timeout even though new password works in browser. Cleared Windows Credential Manager cache.",
                            Status = "Resolved",
                            Priority = "High",
                            CreatedAt = now.AddDays(-10),
                            DueDate = now.AddDays(-9),
                            CategoryId = catMap["Network"],
                            RequesterId = client2.Id,
                            AssignedAgentId = agent2.Id
                        },
                        new()
                        {
                            Title = "Remote access VPN error 412 connection timed out",
                            Description = "Unable to establish VPN tunnel to office gateway. Reinstalled OpenVPN network tap adapter and refreshed security token.",
                            Status = "Resolved",
                            Priority = "High",
                            CreatedAt = now.AddDays(-9),
                            DueDate = now.AddDays(-8),
                            CategoryId = catMap["Network"],
                            RequesterId = employee.Id,
                            AssignedAgentId = agent2.Id
                        },
                        new()
                        {
                            Title = "Outlook email client not syncing inbox and crashing on attachments",
                            Description = "Microsoft Outlook freezes when downloading PDF attachments and fails to sync mail folders. Rebuilt OST profile and disabled add-ins.",
                            Status = "Resolved",
                            Priority = "Medium",
                            CreatedAt = now.AddDays(-8),
                            DueDate = now.AddDays(-7),
                            CategoryId = catMap["Software"],
                            RequesterId = employee.Id,
                            AssignedAgentId = agent3.Id
                        },
                        new()
                        {
                            Title = "Request to install Visual Studio 2022 and SQL Server Management Studio",
                            Description = "Need software installation and developer license activation for C# .NET project on new workstation.",
                            Status = "Resolved",
                            Priority = "Low",
                            CreatedAt = now.AddDays(-7),
                            DueDate = now.AddDays(-5),
                            CategoryId = catMap["Software"],
                            RequesterId = client2.Id,
                            AssignedAgentId = agent3.Id
                        },
                        new()
                        {
                            Title = "Domain account locked out after password expiration",
                            Description = "Cannot log in to Windows workstation this morning, account locked in Active Directory. Unlocked user and forced password reset.",
                            Status = "Resolved",
                            Priority = "High",
                            CreatedAt = now.AddDays(-6),
                            DueDate = now.AddDays(-5),
                            CategoryId = catMap["Access & Security"],
                            RequesterId = employee.Id,
                            AssignedAgentId = agent1.Id
                        },
                        new()
                        {
                            Title = "Grant read/write permissions to HR shared network drive",
                            Description = "New employee in Human Resources needs folder access permissions to shared departmental directory on file server.",
                            Status = "Resolved",
                            Priority = "Low",
                            CreatedAt = now.AddDays(-5),
                            DueDate = now.AddDays(-3),
                            CategoryId = catMap["Access & Security"],
                            RequesterId = client2.Id,
                            AssignedAgentId = agent3.Id
                        },
                        new()
                        {
                            Title = "Wireless USB keyboard and mouse not responding on dock",
                            Description = "External keyboard keys not registering when connected through USB-C docking station. Replaced faulty receiver and updated dock firmware.",
                            Status = "Resolved",
                            Priority = "Low",
                            CreatedAt = now.AddDays(-4),
                            DueDate = now.AddDays(-2),
                            CategoryId = catMap["Hardware"],
                            RequesterId = employee.Id,
                            AssignedAgentId = agent1.Id
                        },
                        new()
                        {
                            Title = "Blue Screen of Death (BSOD) when connecting external display",
                            Description = "Laptop crashes with Video TDR Failure when plugging in HDMI monitor. Resolved by updating display adapter graphics driver.",
                            Status = "Resolved",
                            Priority = "High",
                            CreatedAt = now.AddDays(-15),
                            DueDate = now.AddDays(-14),
                            CategoryId = catMap["Hardware"],
                            RequesterId = employee.Id,
                            AssignedAgentId = agent1.Id
                        },
                        new()
                        {
                            Title = "Wi-Fi disconnecting intermittently on office 5GHz network",
                            Description = "Wireless connection drops every 20 minutes in conference room. Updated wireless network card driver and verified 802.1X profile.",
                            Status = "Resolved",
                            Priority = "Medium",
                            CreatedAt = now.AddDays(-11),
                            DueDate = now.AddDays(-10),
                            CategoryId = catMap["Network"],
                            RequesterId = client2.Id,
                            AssignedAgentId = agent2.Id
                        },
                        new()
                        {
                            Title = "License activation error for JetBrains Rider",
                            Description = "JetBrains Rider license activation ticket expired after annual renewal. Assigned corporate license server key.",
                            Status = "Resolved",
                            Priority = "Low",
                            CreatedAt = now.AddDays(-8),
                            DueDate = now.AddDays(-6),
                            CategoryId = catMap["Software"],
                            RequesterId = employee.Id,
                            AssignedAgentId = agent3.Id
                        },
                        new()
                        {
                            Title = "Reset expired BitLocker recovery key for encrypted drive",
                            Description = "User prompted for BitLocker recovery key after BIOS firmware update. Retrieved 48-digit key from Active Directory.",
                            Status = "Resolved",
                            Priority = "High",
                            CreatedAt = now.AddDays(-5),
                            DueDate = now.AddDays(-4),
                            CategoryId = catMap["Access & Security"],
                            RequesterId = client2.Id,
                            AssignedAgentId = agent1.Id
                        }
                    };

                    context.Tickets.AddRange(tickets);
                    await context.SaveChangesAsync();

                    // 7. SEED COMMENTS (Discussion thread on Ticket #1)
                    var firstTicket = tickets.First();
                    context.Comments.AddRange(
                        new Comment
                        {
                            TicketId = firstTicket.Id,
                            UserId = agent1.Id,
                            CreatedAt = now.AddDays(-2).AddHours(1),
                            Text = "Hi Employee, please try updating your graphics drivers from the Software Center first and check if the refresh rate is set to 60Hz."
                        },
                        new Comment
                        {
                            TicketId = firstTicket.Id,
                            UserId = employee.Id,
                            CreatedAt = now.AddDays(-1).AddHours(2),
                            Text = "I just finished the driver update, but the flickering is still happening when both screens are active. Should I bring the laptop in?"
                        }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
