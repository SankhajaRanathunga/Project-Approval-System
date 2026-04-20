using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectApprovalSystem.Interfaces;
using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;

namespace ProjectApprovalSystem.Controllers
{
    [Authorize(Roles = "ModuleLeader,SystemAdmin")]
    public class AdminController : Controller
    {
        private readonly IProposalService _proposalService;
        private readonly IMatchingService _matchingService;
        private readonly IResearchAreaService _researchAreaService;
        private readonly IUserManagementService _userService;

        public AdminController(
            IProposalService proposalService, 
            IMatchingService matchingService, 
            IResearchAreaService researchAreaService,
            IUserManagementService userService)
        {
            _proposalService = proposalService;
            _matchingService = matchingService;
            _researchAreaService = researchAreaService;
            _userService = userService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var proposals = await _proposalService.GetAllProposalsAsync();
            
            ViewBag.TotalProposals = proposals.Count();
            ViewBag.MatchedProposals = proposals.Count(p => p.IsMatched);
            ViewBag.PendingProposals = proposals.Count(p => p.Status == ProjectStatus.Pending);
            
            return View(proposals);
        }

        public async Task<IActionResult> ManageResearchAreas()
        {
            var areas = await _researchAreaService.GetAllAsync();
            return View(areas);
        }

        [HttpPost]
        public async Task<IActionResult> CreateResearchArea(ResearchArea area)
        {
            await _researchAreaService.CreateAsync(area);
            return RedirectToAction(nameof(ManageResearchAreas));
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string fullName, string email, string password, string role)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            try
            {
                // In a real system, the Admin creates the Identity User and gets back the Id
                // Here we'll simulate the creation flow
                if (role == "Student")
                {
                    await _userService.CreateStudentAccountAsync(Guid.NewGuid().ToString(), fullName, $"S{new Random().Next(100000, 999999)}");
                }
                else if (role == "Supervisor")
                {
                    await _userService.CreateSupervisorAccountAsync(Guid.NewGuid().ToString(), fullName, $"T{new Random().Next(1000, 9999)}");
                }
                
                TempData["Success"] = $"Successfully created {role} account for {fullName}.";
                return RedirectToAction(nameof(ManageUsers));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating user: " + ex.Message);
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelMatch(int proposalId)
        {
            var result = await _matchingService.CancelMatchAsync(proposalId, "Cancelled by Module Leader.");
            if (result) TempData["Success"] = "Match cancelled.";
            else TempData["Error"] = "Failed to cancel match.";

            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> MatchOversight()
        {
            var allProposals = await _proposalService.GetAllProposalsAsync();
            var matchedProposals = allProposals.Where(p => p.IsMatched).ToList();
            
            var details = new List<ViewModels.MatchDetailsViewModel>();
            foreach (var p in matchedProposals)
            {
                var d = await _matchingService.GetMatchDetailsAsync(p.Id);
                if (d != null) details.Add(d);
            }

            return View(details);
        }
    }
}
