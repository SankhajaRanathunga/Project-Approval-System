using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectApprovalSystem.Interfaces;
using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;

namespace ProjectApprovalSystem.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly IMatchingService _matchingService;
        private readonly IUserManagementService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IResearchAreaService _researchAreaService;

        public SupervisorController(
            IMatchingService matchingService, 
            IUserManagementService userService, 
            UserManager<ApplicationUser> userManager,
            IResearchAreaService researchAreaService)
        {
            _matchingService = matchingService;
            _userService = userService;
            _userManager = userManager;
            _researchAreaService = researchAreaService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var supervisor = await _userService.GetSupervisorProfileAsync(userId!);
            
            // For now, let's just show anonymous proposals matching expertise
            // In a real app, we might filter by supervisor.Expertise
            var proposals = await _matchingService.GetAnonymousProposalsAsync();
            
            ViewBag.Supervisor = supervisor;
            return View(proposals);
        }

        public async Task<IActionResult> BlindReview(int? researchAreaId)
        {
            var proposals = await _matchingService.GetAnonymousProposalsAsync(researchAreaId);
            ViewBag.ResearchAreas = await _researchAreaService.GetAllAsync();
            return View(proposals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int proposalId)
        {
            var userId = _userManager.GetUserId(User);
            var supervisor = await _userService.GetSupervisorProfileAsync(userId!);

            var result = await _matchingService.ExpressInterestAsync(proposalId, supervisor!.Id);
            if (result) TempData["Success"] = "Interest expressed! Project status updated.";
            else TempData["Error"] = "Unable to express interest.";

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmMatch(int proposalId)
        {
            var userId = _userManager.GetUserId(User);
            var supervisor = await _userService.GetSupervisorProfileAsync(userId!);

            var result = await _matchingService.ConfirmMatchAsync(proposalId, supervisor!.Id);
            if (result)
            {
                TempData["Success"] = "Match Confirmed! Identity Revealed.";
                return RedirectToAction(nameof(MatchDetails), new { id = proposalId });
            }
            
            TempData["Error"] = "Confirmation failed.";
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> MatchDetails(int id)
        {
            var details = await _matchingService.GetMatchDetailsAsync(id);
            if (details == null) return NotFound();

            return View(details);
        }

        [HttpGet]
        public async Task<IActionResult> ManageExpertise()
        {
            var userId = _userManager.GetUserId(User);
            var supervisor = await _userService.GetSupervisorProfileAsync(userId!);
            
            ViewBag.AllAreas = await _researchAreaService.GetAllAsync();
            return View(supervisor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageExpertise(List<int> researchAreaIds)
        {
            var userId = _userManager.GetUserId(User);
            var supervisor = await _userService.GetSupervisorProfileAsync(userId!);

            await _userService.UpdateSupervisorExpertiseAsync(supervisor!.Id, researchAreaIds);
            
            TempData["Success"] = "Expertise updated.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
