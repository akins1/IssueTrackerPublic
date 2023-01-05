using IssueTracker.Models.Enums;
using IssueTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NuGet.DependencyResolver;

namespace IssueTracker.Data
{

    public static class DataUtility
    {
        //Company Ids
        private static int company1Id;
        private static int company2Id;
        private static int company3Id;
        private static int company4Id;
        private static int company5Id;

        public static string GetConnectionString(IConfiguration configuration)
        {
            //The default connection string will come from appSettings like usual
            var connectionString = configuration.GetConnectionString("defaultConnection");
            //It will be automatically overwritten if we are running on Heroku
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }

        public static string BuildConnectionString(string databaseUrl)
        {
            //Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            //Provides a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer,
                TrustServerCertificate = true
            };
            return builder.ToString();
        }

        public static async Task ManageDataAsync(IHost host)
        {
            using var svcScope = host.Services.CreateScope();
            var svcProvider = svcScope.ServiceProvider;
            //Service: An instance of RoleManager
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            //Service: An instance of RoleManager
            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //Service: An instance of the UserManager
            var userManagerSvc = svcProvider.GetRequiredService<UserManager<IssueTrackerUser>>();
            //Migration: This is the programmatic equivalent to Update-Database
            await dbContextSvc.Database.MigrateAsync();


            //Custom  Bug Tracker Seed Methods
            await SeedRolesAsync(roleManagerSvc);
            await SeedDefaultCompaniesAsync(dbContextSvc);
            await SeedDefaultUsersAsync(userManagerSvc);
            await SeedDemoUsersAsync(userManagerSvc);
            await SeedDefaultTicketTypeAsync(dbContextSvc);
            await SeedDefaultTicketStatusAsync(dbContextSvc);
            await SeedDefaultTicketPriorityAsync(dbContextSvc);
            await SeedDefaultProjectPriorityAsync(dbContextSvc);
            await SeedDefautProjectsAsync(dbContextSvc);
            await SeedDefautTicketsAsync(dbContextSvc);
        }


        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole(RoleEnum.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(RoleEnum.ProjectManager.ToString()));
            await roleManager.CreateAsync(new IdentityRole(RoleEnum.Developer.ToString()));
            await roleManager.CreateAsync(new IdentityRole(RoleEnum.DemoUser.ToString()));
        }

        public static async Task SeedDefaultCompaniesAsync(ApplicationDbContext context)
        {
            try
            {
                IList<Company> defaultcompanies = new List<Company>() {
                    new Company() { Name = "Company1", Description="This is the default seeded Company 1" },
                };

                var dbCompanies = context.Companies.Select(c => c.Name).ToList();
                await context.Companies.AddRangeAsync(defaultcompanies.Where(c => !dbCompanies.Contains(c.Name)));
                await context.SaveChangesAsync();

                //Get company Ids
                company1Id = context.Companies.FirstOrDefault(p => p.Name == "Company1").Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Companies.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultProjectPriorityAsync(ApplicationDbContext context)
        {
            try
            {
                IList<Models.ProjectPriority> projectPriorities = new List<ProjectPriority>() {
                                                    new ProjectPriority() { Name = PriorityEnum.Low.ToString() },
                                                    new ProjectPriority() { Name = PriorityEnum.Medium.ToString() },
                                                    new ProjectPriority() { Name = PriorityEnum.High.ToString() },
                                                    new ProjectPriority() { Name = PriorityEnum.Urgent.ToString() },
                };

                var dbProjectPriorities = context.ProjectPriorities.Select(c => c.Name).ToList();
                await context.ProjectPriorities.AddRangeAsync(projectPriorities.Where(c => !dbProjectPriorities.Contains(c.Name)));
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Project Priorities.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefautProjectsAsync(ApplicationDbContext context)
        {

            //Get project priority Ids
            int priorityLow = context.ProjectPriorities.FirstOrDefault(p => p.Name == PriorityEnum.Low.ToString()).Id;
            int priorityMedium = context.ProjectPriorities.FirstOrDefault(p => p.Name == PriorityEnum.Medium.ToString()).Id;
            int priorityHigh = context.ProjectPriorities.FirstOrDefault(p => p.Name == PriorityEnum.High.ToString()).Id;
            int priorityUrgent = context.ProjectPriorities.FirstOrDefault(p => p.Name == PriorityEnum.Urgent.ToString()).Id;

            try
            {
                IList<Project> projects = new List<Project>() {
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "AntArticulator",
                         Description="Web application written in React to query articulation data between UC and community colleges from Assist.org. Its goal is to aid academic advisors at UCI while updating a student's degree progress" ,
                         StartDate = new DateTime(2022,8,20),
                         EndDate = new DateTime(2023,8,20),
                         ProjectPriorityId = priorityLow
                     },
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "HEYKUBE",
                         Description="HEYKUBE is a research project lead by Professor QV Dang at UCI with the goal of creating an education curriculum to introduce young students to the field of engineering. The project members are UCI students who work with the inventors of HEYKUBE.",
                         StartDate = new DateTime(2021,10,20),
                         EndDate = new DateTime(2023,6,20),
                         ProjectPriorityId = priorityMedium
                     },
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Issue Tracker",
                         Description="A custom designed .Net Core application with postgres database. The application is designed to track issue/bugs in Projects by using and managing Tickets. Implemented with ASP.NET Identity and user roles. Tickets are maintained in projects which are maintained by users in the role of ProjectManager. Each project has a team and team members.",
                         StartDate = new DateTime(2022,8,20),
                         EndDate = new DateTime(2022,8,20).AddMonths(6),
                         ProjectPriorityId = priorityHigh
                     },
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Zotbuddy",
                         Description="A mobile application written in React Native for the UCI research group, Zotbins. It utilizes authentication and leaderboard features by using Firebase Firestore. ",
                         StartDate = new DateTime(2022,4,20),
                         EndDate = new DateTime(2023,3,20),
                         ProjectPriorityId = priorityLow
                     }
                    
                };

                var dbProjects = context.Projects.Select(c => c.Name).ToList();
                await context.Projects.AddRangeAsync(projects.Where(c => !dbProjects.Contains(c.Name)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Projects.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }



        public static async Task SeedDefaultUsersAsync(UserManager<IssueTrackerUser> userManager)
        {
            //Seed Default Admin User
            var defaultUser = new IssueTrackerUser
            {
                UserName = "akins1@gmail.com",
                Email = "akins1@gmail.com",
                FirstName = "Akins",
                LastName = "Admin",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Admin User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

            //Seed Default Admin User
            defaultUser = new IssueTrackerUser
            {
                UserName = "joeshmoeAdmin1@gmail.com",
                Email = "joeshmoeAdmin1@gmail.com",
                FirstName = "Joe",
                LastName = "Shmoe",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.Admin.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Admin User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default ProjectManager1 User
            defaultUser = new IssueTrackerUser
            {
                UserName = "bobrossPM1@gmail.com",
                Email = "bobrossPM1@gmail.com",
                FirstName = "Bob",
                LastName = "Ross",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default ProjectManager1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default ProjectManager2 User
            defaultUser = new IssueTrackerUser
            {
                UserName = "thomastrainPM2@gmail.com",
                Email = "thomastrainPM2@gmail.com",
                FirstName = "Thomas",
                LastName = "Train",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.ProjectManager.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default ProjectManager2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Developer1 User
            defaultUser = new IssueTrackerUser
            {
                UserName = "bobbuilderDev1@gmail.com",
                Email = "bobbuilderDev1@gmail.com",
                FirstName = "Bob",
                LastName = "Builder",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Developer2 User
            defaultUser = new IssueTrackerUser
            {
                UserName = "grahamcrackerDev2@gmail.com",
                Email = "grahamcrackerDev2@gmail.com",
                FirstName = "Graham",
                LastName = "Cracker",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer2 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Developer3 User
            defaultUser = new IssueTrackerUser
            {
                UserName = "rogerfedererDev3@gmail.com",
                Email = "rogerfedererDev3@gmail.com",
                FirstName = "Roger",
                LastName = "Federer",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer3 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Developer4 User
            defaultUser = new IssueTrackerUser
            {
                UserName = "davidblaineDev4@gmail.com",
                Email = "davidblaineDev4@gmail.com",
                FirstName = "David",
                LastName = "Blaine",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer4 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Default Developer5 User
            defaultUser = new IssueTrackerUser
            {
                UserName = "mrbeastDev5@gmail.com",
                Email = "mrbeastDev5@gmail.com",
                FirstName = "Mr.",
                LastName = "Beast",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.Developer.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Default Developer5 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }

        }

        public static async Task SeedDemoUsersAsync(UserManager<IssueTrackerUser> userManager)
        {
            //Seed Demo Admin User
            var defaultUser = new IssueTrackerUser
            {
                UserName = "demoadmin@gmail.com",
                Email = "demoadmin@gmail.com",
                FirstName = "Demo",
                LastName = "Admin",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.DemoUser.ToString());

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Admin User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Demo ProjectManager User
            defaultUser = new IssueTrackerUser
            {
                UserName = "demopm@gmail.com",
                Email = "demopm@gmail.com",
                FirstName = "Demo",
                LastName = "ProjectManager",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.ProjectManager.ToString());
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo ProjectManager1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            //Seed Demo Developer User
            defaultUser = new IssueTrackerUser
            {
                UserName = "demodev@gmail.com",
                Email = "demodev@gmail.com",
                FirstName = "Demo",
                LastName = "Developer",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.Developer.ToString());
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo Developer1 User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }


            


            //Seed Demo New User
            defaultUser = new IssueTrackerUser
            {
                UserName = "demonew@gmail.com",
                Email = "demonew@gmail.com",
                FirstName = "Demo",
                LastName = "NewUser",
                EmailConfirmed = true,
                CompanyId = company1Id
            };
            try
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Abc&123!");
                    await userManager.AddToRoleAsync(defaultUser, RoleEnum.DemoUser.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Demo New User.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }



        public static async Task SeedDefaultTicketTypeAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketType> ticketTypes = new List<TicketType>() {
                     new TicketType() { Name = TicketTypeEnum.NewDevelopment.ToString() },      // Ticket involves development of a new, uncoded solution 
                     new TicketType() { Name = TicketTypeEnum.WorkTask.ToString() },            // Ticket involves development of the specific ticket description 
                     new TicketType() { Name = TicketTypeEnum.Defect.ToString()},               // Ticket involves unexpected development/maintenance on a previously designed feature/functionality
                     new TicketType() { Name = TicketTypeEnum.ChangeRequest.ToString() },       // Ticket involves modification development of a previously designed feature/functionality
                     new TicketType() { Name = TicketTypeEnum.Enhancement.ToString() },         // Ticket involves additional development on a previously designed feature or new functionality
                     new TicketType() { Name = TicketTypeEnum.GeneralTask.ToString() }          // Ticket involves no software development but may involve tasks such as configuations, or hardware setup
                };

                var dbTicketTypes = context.TicketTypes.Select(c => c.Name).ToList();
                await context.TicketTypes.AddRangeAsync(ticketTypes.Where(c => !dbTicketTypes.Contains(c.Name)));
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Types.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultTicketStatusAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketStatus> ticketStatuses = new List<TicketStatus>() {
                    new TicketStatus() { Name = TicketStatusEnum.New.ToString() },                 // Newly Created ticket having never been assigned
                    new TicketStatus() { Name = TicketStatusEnum.Development.ToString() },         // Ticket is assigned and currently being worked 
                    new TicketStatus() { Name = TicketStatusEnum.Testing.ToString()  },            // Ticket is assigned and is currently being tested
                    new TicketStatus() { Name = TicketStatusEnum.Resolved.ToString()  },           // Ticket remains assigned to the developer but work in now complete
                };

                var dbTicketStatuses = context.TicketStatuses.Select(c => c.Name).ToList();
                await context.TicketStatuses.AddRangeAsync(ticketStatuses.Where(c => !dbTicketStatuses.Contains(c.Name)));
                await context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Statuses.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

        public static async Task SeedDefaultTicketPriorityAsync(ApplicationDbContext context)
        {
            try
            {
                IList<TicketPriority> ticketPriorities = new List<TicketPriority>() {
                                                    new TicketPriority() { Name = PriorityEnum.Low.ToString()  },
                                                    new TicketPriority() { Name = PriorityEnum.Medium.ToString() },
                                                    new TicketPriority() { Name = PriorityEnum.High.ToString()},
                                                    new TicketPriority() { Name = PriorityEnum.Urgent.ToString()},
                };

                var dbTicketPriorities = context.TicketPriorities.Select(c => c.Name).ToList();
                await context.TicketPriorities.AddRangeAsync(ticketPriorities.Where(c => !dbTicketPriorities.Contains(c.Name)));
                context.SaveChanges();

            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Ticket Priorities.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }



        public static async Task SeedDefautTicketsAsync(ApplicationDbContext context)
        {
            //Get project Ids
            int AntArticulatorId = context.Projects.FirstOrDefault(p => p.Name == "AntArticulator").Id;
            int HEYKUBEId = context.Projects.FirstOrDefault(p => p.Name == "HEYKUBE").Id;
            int IssueTrackerId = context.Projects.FirstOrDefault(p => p.Name == "Issue Tracker").Id;
            int ZotbuddyId = context.Projects.FirstOrDefault(p => p.Name == "Zotbuddy").Id;

            //Get ticket type Ids
            int typeNewDev = context.TicketTypes.FirstOrDefault(p => p.Name == TicketTypeEnum.NewDevelopment.ToString()).Id;
            int typeWorkTask = context.TicketTypes.FirstOrDefault(p => p.Name == TicketTypeEnum.WorkTask.ToString()).Id;
            int typeDefect = context.TicketTypes.FirstOrDefault(p => p.Name == TicketTypeEnum.Defect.ToString()).Id;
            int typeEnhancement = context.TicketTypes.FirstOrDefault(p => p.Name == TicketTypeEnum.Enhancement.ToString()).Id;
            int typeChangeRequest = context.TicketTypes.FirstOrDefault(p => p.Name == TicketTypeEnum.ChangeRequest.ToString()).Id;

            //Get ticket priority Ids
            int priorityLow = context.TicketPriorities.FirstOrDefault(p => p.Name == PriorityEnum.Low.ToString()).Id;
            int priorityMedium = context.TicketPriorities.FirstOrDefault(p => p.Name == PriorityEnum.Medium.ToString()).Id;
            int priorityHigh = context.TicketPriorities.FirstOrDefault(p => p.Name == PriorityEnum.High.ToString()).Id;
            int priorityUrgent = context.TicketPriorities.FirstOrDefault(p => p.Name == PriorityEnum.Urgent.ToString()).Id;

            //Get ticket status Ids
            int statusNew = context.TicketStatuses.FirstOrDefault(p => p.Name == TicketStatusEnum.New.ToString()).Id;
            int statusDev = context.TicketStatuses.FirstOrDefault(p => p.Name == TicketStatusEnum.Development.ToString()).Id;
            int statusTest = context.TicketStatuses.FirstOrDefault(p => p.Name == TicketStatusEnum.Testing.ToString()).Id;
            int statusResolved = context.TicketStatuses.FirstOrDefault(p => p.Name == TicketStatusEnum.Resolved.ToString()).Id;


            try
            {
                IList<Ticket> tickets = new List<Ticket>() {
                                //AntArticulator
                                new Ticket() {Title = "AntArticulator Ticket 1", Description = "Ticket details for AntArticulator ticket 1", CreatedOn = DateTimeOffset.Now, ProjectId = AntArticulatorId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "AntArticulator Ticket 2", Description = "Ticket details for AntArticulator ticket 2", CreatedOn = DateTimeOffset.Now, ProjectId = AntArticulatorId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "AntArticulator Ticket 3", Description = "Ticket details for AntArticulator ticket 3", CreatedOn = DateTimeOffset.Now, ProjectId = AntArticulatorId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "AntArticulator Ticket 4", Description = "Ticket details for AntArticulator ticket 4", CreatedOn = DateTimeOffset.Now, ProjectId = AntArticulatorId, TicketPriorityId = priorityUrgent, TicketStatusId = statusTest, TicketTypeId = typeDefect},
                                new Ticket() {Title = "AntArticulator Ticket 5", Description = "Ticket details for AntArticulator ticket 5", CreatedOn = DateTimeOffset.Now, ProjectId = AntArticulatorId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "AntArticulator Ticket 6", Description = "Ticket details for AntArticulator ticket 6", CreatedOn = DateTimeOffset.Now, ProjectId = AntArticulatorId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "AntArticulator Ticket 7", Description = "Ticket details for AntArticulator ticket 7", CreatedOn = DateTimeOffset.Now, ProjectId = AntArticulatorId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "AntArticulator Ticket 8", Description = "Ticket details for AntArticulator ticket 8", CreatedOn = DateTimeOffset.Now, ProjectId = AntArticulatorId, TicketPriorityId = priorityUrgent, TicketStatusId = statusTest, TicketTypeId = typeDefect},
                                //HEYKUBE
                                new Ticket() {Title = "HEYKUBE Ticket 1", Description = "Ticket details for HEYKUBE ticket 1", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "HEYKUBE Ticket 2", Description = "Ticket details for HEYKUBE ticket 2", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "HEYKUBE Ticket 3", Description = "Ticket details for HEYKUBE ticket 3", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "HEYKUBE Ticket 4", Description = "Ticket details for HEYKUBE ticket 4", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "HEYKUBE Ticket 5", Description = "Ticket details for HEYKUBE ticket 5", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityLow, TicketStatusId = statusDev,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "HEYKUBE Ticket 6", Description = "Ticket details for HEYKUBE ticket 6", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "HEYKUBE Ticket 7", Description = "Ticket details for HEYKUBE ticket 7", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "HEYKUBE Ticket 8", Description = "Ticket details for HEYKUBE ticket 8", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityUrgent, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "HEYKUBE Ticket 9", Description = "Ticket details for HEYKUBE ticket 9", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityLow, TicketStatusId = statusNew,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "HEYKUBE Ticket 10", Description = "Ticket details for HEYKUBE ticket 10", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "HEYKUBE Ticket 11", Description = "Ticket details for HEYKUBE ticket 11", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "HEYKUBE Ticket 12", Description = "Ticket details for HEYKUBE ticket 12", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "HEYKUBE Ticket 13", Description = "Ticket details for HEYKUBE ticket 13", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "HEYKUBE Ticket 14", Description = "Ticket details for HEYKUBE ticket 14", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "HEYKUBE Ticket 15", Description = "Ticket details for HEYKUBE ticket 15", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "HEYKUBE Ticket 16", Description = "Ticket details for HEYKUBE ticket 16", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "HEYKUBE Ticket 17", Description = "Ticket details for HEYKUBE ticket 17", CreatedOn = DateTimeOffset.Now, ProjectId = HEYKUBEId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                //ISSUE TRACKER                                                                                                                         
                                new Ticket() {Title = "Issue Tracker Ticket 1", Description = "Ticket details for Issue Tracker ticket 1", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 2", Description = "Ticket details for Issue Tracker ticket 2", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 3", Description = "Ticket details for Issue Tracker ticket 3", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 4", Description = "Ticket details for Issue Tracker ticket 4", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 5", Description = "Ticket details for Issue Tracker ticket 5", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 6", Description = "Ticket details for Issue Tracker ticket 6", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 7", Description = "Ticket details for Issue Tracker ticket 7", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 8", Description = "Ticket details for Issue Tracker ticket 8", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 9", Description = "Ticket details for Issue Tracker ticket 9", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 10", Description = "Ticket details for Issue Tracker 10", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 11", Description = "Ticket details for Issue Tracker 11", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 12", Description = "Ticket details for Issue Tracker 12", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 13", Description = "Ticket details for Issue Tracker 13", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 14", Description = "Ticket details for Issue Tracker 14", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 15", Description = "Ticket details for Issue Tracker 15", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 16", Description = "Ticket details for Issue Tracker 16", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 17", Description = "Ticket details for Issue Tracker 17", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 18", Description = "Ticket details for Issue Tracker 18", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 19", Description = "Ticket details for Issue Tracker 19", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 20", Description = "Ticket details for Issue Tracker 20", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 21", Description = "Ticket details for Issue Tracker 21", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 22", Description = "Ticket details for Issue Tracker 22", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 23", Description = "Ticket details for Issue Tracker 23", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 24", Description = "Ticket details for Issue Tracker 24", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 25", Description = "Ticket details for Issue Tracker 25", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 26", Description = "Ticket details for Issue Tracker 26", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 27", Description = "Ticket details for Issue Tracker 27", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 28", Description = "Ticket details for Issue Tracker 28", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 29", Description = "Ticket details for Issue Tracker 29", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Issue Tracker Ticket 30", Description = "Ticket details for Issue Tracker 30", CreatedOn = DateTimeOffset.Now, ProjectId = IssueTrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                //MOVIE
                                new Ticket() {Title = "Zotbuddy Ticket 1", Description = "Ticket details for Zotbuddy ticket 1", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Zotbuddy Ticket 2", Description = "Ticket details for Zotbuddy ticket 2", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Zotbuddy Ticket 3", Description = "Ticket details for Zotbuddy ticket 3", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Zotbuddy Ticket 4", Description = "Ticket details for Zotbuddy ticket 4", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Zotbuddy Ticket 5", Description = "Ticket details for Zotbuddy ticket 5", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityLow, TicketStatusId = statusDev,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "Zotbuddy Ticket 6", Description = "Ticket details for Zotbuddy ticket 6", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Zotbuddy Ticket 7", Description = "Ticket details for Zotbuddy ticket 7", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Zotbuddy Ticket 8", Description = "Ticket details for Zotbuddy ticket 8", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityUrgent, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Zotbuddy Ticket 9", Description = "Ticket details for Zotbuddy ticket 9", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityLow, TicketStatusId = statusNew,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "Zotbuddy Ticket 10", Description = "Ticket details for Zotbuddy ticket 10", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Zotbuddy Ticket 11", Description = "Ticket details for Zotbuddy ticket 11", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Zotbuddy Ticket 12", Description = "Ticket details for Zotbuddy ticket 12", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Zotbuddy Ticket 13", Description = "Ticket details for Zotbuddy ticket 13", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Zotbuddy Ticket 14", Description = "Ticket details for Zotbuddy ticket 14", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Zotbuddy Ticket 15", Description = "Ticket details for Zotbuddy ticket 15", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Zotbuddy Ticket 16", Description = "Ticket details for Zotbuddy ticket 16", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Zotbuddy Ticket 17", Description = "Ticket details for Zotbuddy ticket 17", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Zotbuddy Ticket 18", Description = "Ticket details for Zotbuddy ticket 18", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Zotbuddy Ticket 19", Description = "Ticket details for Zotbuddy ticket 19", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Zotbuddy Ticket 20", Description = "Ticket details for Zotbuddy ticket 20", CreatedOn = DateTimeOffset.Now, ProjectId = ZotbuddyId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},

                };


                var dbTickets = context.Tickets.Select(c => c.Title).ToList();
                await context.Tickets.AddRangeAsync(tickets.Where(c => !dbTickets.Contains(c.Title)));
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("*************  ERROR  *************");
                Console.WriteLine("Error Seeding Tickets.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***********************************");
                throw;
            }
        }

    }
}
