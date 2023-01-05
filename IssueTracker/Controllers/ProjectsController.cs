using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Extensions;
using IssueTracker.Models.ViewModels;
using IssueTracker.Services.Interfaces;
using IssueTracker.Models.Enums;
using IssueTracker.Services;
using System.Drawing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace IssueTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IITRolesService _roleService;
        private readonly IITLookupService _lookupService;
        private readonly IITFileService _fileService;
        private readonly IITProjectService _projectService;
        private readonly UserManager<IssueTrackerUser> _userManager;
        private readonly IITCompanyInfoService _companyInfoService;

        public ProjectsController(IITRolesService roleService,
                                  IITLookupService lookupService,
                                  IITFileService fileService,
                                  IITProjectService projectService,
                                  UserManager<IssueTrackerUser> userManager,
                                  IITCompanyInfoService companyInfoService)
        {
            _roleService = roleService;
            _lookupService = lookupService;
            _fileService = fileService;
            _projectService = projectService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }

        public async Task<IActionResult> Index()
        {
            return RedirectToAction("AllProjects");
        }

        public async Task<IActionResult> MyProjects()
        {
            string userId = _userManager.GetUserId(User);

            List<Project> projects = await _projectService.GetUserProjectsAsync(userId);

            return View(projects);
        }

        public async Task<IActionResult> AllProjects()
        {
            List<Project> projects = new();

            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(RoleEnum.Admin)) || User.IsInRole(nameof(RoleEnum.ProjectManager)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompany(companyId);
            }

            return View(projects);
        }

        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetArchivedProjectsByCompany(companyId);

            return View(projects);
        }

        [Authorize(Roles = nameof(RoleEnum.Admin))]
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = await _projectService.GetUnassignedProjectsByCompany(companyId);

            return View(projects);
        }

        [Authorize(Roles = nameof(RoleEnum.Admin))]
        public async Task<IActionResult> AssignPM(int projectId)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(projectId, companyId);

            List<IssueTrackerUser> PMs = await _roleService.GetUsersInRoleAsync(nameof(RoleEnum.ProjectManager), companyId);

            AssignPMViewModel model = new();
            model.Project = project;
            model.PMList = new SelectList(PMs, "Id", "FullName");


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(RoleEnum.Admin))]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if (! string.IsNullOrEmpty(model.PMId))
            {
                await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);

                return RedirectToAction(nameof(Details), new { id=model.Project.Id });
            }

            return RedirectToAction(nameof(AssignPM), new { projectId = model.Project.Id });
        }

        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> AssignProjectMembers(int projectId)
        {
            AssignProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;
            List<IssueTrackerUser> developers = await _roleService.GetUsersInRoleAsync(nameof(RoleEnum.Developer), companyId);
            List<IssueTrackerUser> submitters = await _roleService.GetUsersInRoleAsync(nameof(RoleEnum.Submitter), companyId);
            List<IssueTrackerUser> companyMembers = developers.Concat(submitters).ToList();

            model.Project = await _projectService.GetProjectByIdAsync(projectId, companyId);


            List<string> projectMembers = model.Project.Members.Select(m => m.Id).ToList();

            model.Users = new MultiSelectList(companyMembers, "Id", "FullName", projectMembers);

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> AssignProjectMembers(AssignProjectMembersViewModel model)
        {
            if (model.SelectedUsers != null && model.SelectedUsers.Count > 0)
            {
                List<string> oldMembers = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id)).Select(u => u.Id).ToList();
                foreach (string oldMember in oldMembers)
                {
                    await _projectService.RemoveUserFromProjectAsync(oldMember, model.Project.Id);
                }

                foreach (string userId in model.SelectedUsers)
                {
                    await _projectService.AddUserToProjectAsync(userId, model.Project.Id);
                }

                return RedirectToAction(nameof(Details), new { id=model.Project.Id });
            }


            return RedirectToAction(nameof(AssignProjectMembers), new { projectId=model.Project.Id });
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AddProjectWithPMViewModel model = new();

            model.PMList = new SelectList(
                await _roleService.GetUsersInRoleAsync(RoleEnum.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");

            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                int companyId = User.Identity.GetCompanyId().Value;

                try
                {
                    model.Project.CompanyId = companyId;

                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }

                    await _projectService.AddNewProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PMId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                    }
                }
                catch (Exception)
                {

                    throw;
                }

                //TODO: redirect to all projects
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Create));
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> Edit(int? id)
        {

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            IssueTrackerUser PM = await _projectService.GetProjectManagerAsync(project.Id);


            AddProjectWithPMViewModel model = new();

            model.Project = project;
            model.PMId = PM?.Id;
            model.PMList = new SelectList(
                await _roleService.GetUsersInRoleAsync(RoleEnum.ProjectManager.ToString(), companyId), "Id", "FullName", model.PMId);
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                try
                {
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                    }

                    await _projectService.UpdateProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PMId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PMId, model.Project.Id);
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (! await ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                    
                }

                //TODO: redirect to all projects
                
            }
            return RedirectToAction(nameof(Edit));
        }

        // GET: Projects/Archive/5
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);


            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.ArchiveProjectAsync(project);
            
            return RedirectToAction(nameof(Index));
        }


        // GET: Projects/Restore/5
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> Restore(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id, companyId);


            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.RestoreProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return (await _projectService.GetAllProjectsByCompany(companyId)).Any(p => p.Id == id);
        }
    }
}
