using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Identity;
using IssueTracker.Extensions;
using IssueTracker.Models.Enums;
using IssueTracker.Services.Interfaces;
using IssueTracker.Services;
using Microsoft.AspNetCore.Authorization;
using IssueTracker.Models.ViewModels;

namespace IssueTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly UserManager<IssueTrackerUser> _userManager;
        private readonly IITProjectService _projectService;
        private readonly IITLookupService _lookupService;
        private readonly IITTicketService _ticketService;
        private readonly IITFileService _fileService;
        private readonly IITTicketHistoryService _historyService;

        public TicketsController(UserManager<IssueTrackerUser> userManager,
                                 IITProjectService projectService,
                                 IITLookupService lookupService,
                                 IITTicketService ticketService,
                                 IITFileService fileService,
                                 IITTicketHistoryService historyService)
        {
            _userManager = userManager;
            _projectService = projectService;
            _lookupService = lookupService;
            _ticketService = ticketService;
            _fileService = fileService;
            _historyService = historyService;
        }

        public async Task<IActionResult> Index()
        {
            return RedirectToAction("AllTickets");
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            IssueTrackerUser user = await _userManager.GetUserAsync(User);

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

            //bool isDevOrSubmitter = (await _userManager.IsInRoleAsync(user, nameof(RoleEnum.Developer))) ||
            //                        (await _userManager.IsInRoleAsync(user, nameof(RoleEnum.Submitter)))

            bool isDevOrSubmitter = User.IsInRole(nameof(RoleEnum.Developer)) ||
                                  User.IsInRole(nameof(RoleEnum.Submitter));

            if (isDevOrSubmitter)
            {
                tickets = tickets.Where(t => t.Archived == false).ToList();
                //await _ticketService.GetTicketsByUserIdAsync(user.Id, companyId);
            }

            return View(tickets);

        }

        public async Task<IActionResult> MyTickets()
        {
            IssueTrackerUser user = await _userManager.GetUserAsync(User);

            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(user.Id, companyId);

            return View(tickets);
        }

        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId);

            return View(tickets);
        }

        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            string userId = _userManager.GetUserId(User);

            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            if (User.IsInRole(nameof(RoleEnum.Admin)))
            {
                return View(tickets);
            }
            
            List<Ticket> projectManagerTickets = new();
                
            foreach(Ticket ticket in tickets)
            {
                if (await _projectService.IsUserProjectManagerAsync(userId, ticket.ProjectId))
                {
                    projectManagerTickets.Add(ticket);
                }
            }

            return View(projectManagerTickets);
        }

        [HttpGet]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> AssignDeveloper(int id)
        {

            AssignDeveloperViewModel model = new();

            model.Ticket = await _ticketService.GetTicketByIdAsync(id);

            List<IssueTrackerUser> projectMembers = await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(RoleEnum.Developer));

            model.Developers = new SelectList(projectMembers, "Id", "FullName");

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if (model.DeveloperId != null)
            {
                IssueTrackerUser ITUser = await _userManager.GetUserAsync(User);

                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);
                
                await _ticketService.AssignTicketAsync(model.Ticket.Id, model.DeveloperId);



                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket.Id);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, ITUser.Id);

                return RedirectToAction(nameof(Details), new { id = model.Ticket.Id });

            }
            return RedirectToAction(nameof(AssignDeveloper), new { id=model.Ticket.Id});
        }

        
        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            IssueTrackerUser ITUser = await _userManager.GetUserAsync(User);

            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(RoleEnum.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");

            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(ITUser.Id), "Id", "Name");

            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            //
            IssueTrackerUser ITUser = await _userManager.GetUserAsync(User);

            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, x.Value.Errors })
                .ToArray();

            if (ticket != null) // errors.Length <= 6 ) //ModelState.IsValid)
            {

                try
                {
                    ticket.CreatedOn = DateTimeOffset.Now;
                    ticket.OwnerUserId = ITUser.Id;
                    ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync(nameof(TicketStatusEnum.New))).Value;

                    await _ticketService.AddNewTicketAsync(ticket);


                    Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                    await _historyService.AddHistoryAsync(null, newTicket, ITUser.Id);

                }
                catch (Exception)
                {

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(RoleEnum.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");

            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(ITUser.Id), "Id", "Name");

            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");
            
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }
            
            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CreatedOn,Updated,Archived,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ticket != null) //ModelState.IsValid)
            {
                IssueTrackerUser ITUser = await _userManager.GetUserAsync(User);

                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                try
                {
                    ticket.Updated = DateTimeOffset.Now;
                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id).Result)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                await _historyService.AddHistoryAsync(oldTicket, newTicket, ITUser.Id);

                return RedirectToAction(nameof(Index));
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }

        // GET: Tickets/Archive/5
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);

            await _ticketService.ArchiveTicketAsync(ticket);

            return RedirectToAction(nameof(Index));
        }


        // GET: Tickets/Restore/5
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);


            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(RoleEnum.Admin)}, {nameof(RoleEnum.ProjectManager)}")]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);

            await _ticketService.RestoreTicketAsync(ticket);

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketComment([Bind("Id,TicketId,Comment")] TicketComment ticketComment)
        {
            if (true) //ModelState.IsValid)
            {
                try
                {
                    ticketComment.UserId = _userManager.GetUserId(User);
                    ticketComment.CreatedOn = DateTimeOffset.Now;

                    await _ticketService.AddTicketCommentAsync(ticketComment);

                    await _historyService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);
                }
                catch (Exception)
                {

                    throw;
                }
            }

            return RedirectToAction(nameof(Details), new { id = ticketComment.TicketId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ticketAttachment.FormFile != null && true) //ModelState.IsValid && )
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.CreatedOn = DateTimeOffset.Now;
                ticketAttachment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.UserId);

                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data";
            }

            return RedirectToAction(nameof(Details), new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> GetFile(Guid id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);

            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string fileExtension = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{fileExtension}");

        }

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

          return (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).Any(t => t.Id == id);
        }
    }
}
