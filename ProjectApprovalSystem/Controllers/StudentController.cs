using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectApprovalSystem.Interfaces;
using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;

namespace ProjectApprovalSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IProposalService _proposalService;
        private readonly IUserManagementService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IResearchAreaService _researchAreaService;

        public StudentController(
            IProposalService proposalService, 
            IUserManagementService userService, 
            UserManager<ApplicationUser> userManager,
            IResearchAreaService researchAreaService)
        {
            _proposalService = proposalService;
            _userService = userService;
            _userManager = userManager;
            _researchAreaService = researchAreaService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            var proposals = await _proposalService.GetStudentProposalsAsync(userId);
            return View(proposals);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProposal()
        {
            ViewBag.ResearchAreas = await _researchAreaService.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProposal(ProjectProposal proposal)
        {
            var userId = _userManager.GetUserId(User);
            var student = await _userService.GetStudentProfileAsync(userId!);
            
            if (student == null) return NotFound("Student profile not found.");

            proposal.StudentId = student.Id;
            await _proposalService.CreateProposalAsync(proposal);

            TempData["Success"] = "Proposal submitted successfully!";
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> Details(int id)
        {
            var proposal = await _proposalService.GetProposalByIdAsync(id);
            if (proposal == null || proposal.Student?.UserId != _userManager.GetUserId(User)) return NotFound();

            return View(proposal);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var proposal = await _proposalService.GetProposalByIdAsync(id);
            
            if (proposal == null || proposal.Student?.UserId != userId) return NotFound();
            if (proposal.IsMatched)
            {
                TempData["Error"] = "Matched proposals cannot be edited.";
                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.ResearchAreas = await _researchAreaService.GetAllAsync();
            return View(proposal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectProposal proposal)
        {
            if (id != proposal.Id) return BadRequest();

            var userId = _userManager.GetUserId(User);
            var existing = await _proposalService.GetProposalByIdAsync(id);
            if (existing == null || existing.Student?.UserId != userId) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _proposalService.UpdateProposalAsync(proposal);
                if (result)
                {
                    TempData["Success"] = "Proposal updated successfully!";
                    return RedirectToAction(nameof(Dashboard));
                }
                TempData["Error"] = "Unable to update proposal (it might be matched).";
            }

            ViewBag.ResearchAreas = await _researchAreaService.GetAllAsync();
            return View(proposal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _proposalService.WithdrawProposalAsync(id, userId!);
            
            if (result) TempData["Success"] = "Proposal withdrawn.";
            else TempData["Error"] = "Cannot withdraw this proposal.";

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
