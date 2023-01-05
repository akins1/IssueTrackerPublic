using IssueTracker.Extensions;
using IssueTracker.Models;
using IssueTracker.Models.Enums;
using IssueTracker.Models.ViewModels;
using IssueTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IssueTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IITCompanyInfoService _companyService;
        private readonly IITProjectService _projectService;
        private readonly IITLookupService _lookupService;
        private readonly IITTicketService _ticketService;
        private readonly SignInManager<IssueTrackerUser> _signInManager;
        private readonly UserManager<IssueTrackerUser> _userManager;
        private readonly IITRolesService _roleService;

        public HomeController(ILogger<HomeController> logger,
                              IITCompanyInfoService companyService,
                              IITProjectService projectService,
                              IITLookupService lookupService,
                              IITTicketService ticketService,
                              SignInManager<IssueTrackerUser> signInManager,
                              UserManager<IssueTrackerUser> userManager,
                              IITRolesService roleService)
        {
            _logger = logger;
            _companyService = companyService;
            _projectService = projectService;
            _lookupService = lookupService;
            _ticketService = ticketService;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleService = roleService;
        }

        public async Task<IActionResult> Index()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(Dashboard)); 
            }
            else
            {
                return View();
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();

            IssueTrackerUser ITUser = await _userManager.GetUserAsync(User);
            int companyId = User.Identity.GetCompanyId().Value;

            model.Company = await _companyService.GetCompanyInfoByIdAsync(companyId);
            model.Projects = (await _projectService.GetAllProjectsByCompany(companyId)).Where(p => p.Archived == false).ToList();
            model.Tickets = model.Projects.SelectMany(p => p.Tickets).Where(t => t.Archived == false).ToList();
            model.Members = model.Company.Members.ToList();

            if (await _roleService.IsUserInRoleAsync(ITUser, nameof(RoleEnum.Admin)))
            {
                model.UserProjects = model.Projects;
                model.UserTickets = model.Tickets;
            }
            else if (await _roleService.IsUserInRoleAsync(ITUser, nameof(RoleEnum.ProjectManager)))
            {
                model.UserProjects = model.Projects.Where(p => (_projectService.IsUserOnProjectAsync(ITUser.Id, p.Id)).Result).ToList();
                model.UserTickets = model.UserProjects.SelectMany(p => p.Tickets).ToList();
            }
            else
            {
                model.UserProjects = model.Projects.Where(p => (_projectService.IsUserOnProjectAsync(ITUser.Id, p.Id)).Result).ToList();
                var userTickets = model.Tickets.Where(t => ((_ticketService.GetTicketDeveloperAsync(t.Id, companyId)).Result)?.Id == ITUser.Id);
                model.UserTickets = userTickets.ToList();
            }

            return View(model);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
        
        /*[HttpPost]
        public Task<IActionResult> Login()
        {
            //var result = _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

        }*/

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "ProjectName", "TicketCount" });

            foreach (Project prj in projects)
            {
                chartData.Add(new object[] { prj.Name, prj.Tickets.Count() });
            }

            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectPriority()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "Priority", "Count" });


            foreach (string priority in Enum.GetNames(typeof(PriorityEnum)))
            {
                int priorityCount = (await _projectService.GetAllProjectsByPriority(companyId, priority)).Count();
                chartData.Add(new object[] { priority, priorityCount });
            }

            return Json(chartData);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string searchParam)
        {
            if (! string.IsNullOrEmpty(searchParam))
            {
                SearchViewModel searchModel = new();

                // searchModel.Companies = await _searchService.GetCompaniesAsync(searchParam);
                // searchModel.Users = await _searchService.GetUsersAsync(searchParam);
                // searchModel.Projects = await _searchService.GetProjectsAsync(searchParam);
                // searchModel.Tickets = await _searchService.GetTicketsAsync(searchParam);

                return RedirectToAction(nameof(Search), new { model = searchModel });
            }
            return View();

            
        }

        public async Task<IActionResult> Search(SearchViewModel model)
        {



            return View(model);
        }
    }
}