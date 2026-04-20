using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectApprovalSystem.Data;
using ProjectApprovalSystem.Interfaces;
using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;

namespace ProjectApprovalSystem.Services
{
    public class ResearchAreaService : IResearchAreaService
    {
        private readonly ApplicationDbContext _context;

        public ResearchAreaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ResearchArea>> GetAllAsync() => await _context.ResearchAreas.ToListAsync();

        public async Task<ResearchArea?> GetByIdAsync(int id) => await _context.ResearchAreas.FindAsync(id);

        public async Task<ResearchArea> CreateAsync(ResearchArea area)
        {
            _context.ResearchAreas.Add(area);
            await _context.SaveChangesAsync();
            return area;
        }

        public async Task<bool> UpdateAsync(ResearchArea area)
        {
            var existing = await _context.ResearchAreas.FindAsync(area.Id);
            if (existing == null) return false;
            existing.Name = area.Name;
            existing.Description = area.Description;
            existing.IsActive = area.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var area = await _context.ResearchAreas.FindAsync(id);
            if (area == null) return false;
            area.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();
            return true;
        }
    }

    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserManagementService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string id) => await _userManager.FindByIdAsync(id);

        public async Task<StudentProfile?> GetStudentProfileAsync(string userId)
        {
            return await _context.StudentProfiles
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<SupervisorProfile?> GetSupervisorProfileAsync(string userId)
        {
            return await _context.SupervisorProfiles
                .Include(s => s.User)
                .Include(s => s.Expertise)
                .ThenInclude(e => e.ResearchArea)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<bool> CreateStudentAccountAsync(ApplicationUser user, string password, StudentProfile profile)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return false;

            await _userManager.AddToRoleAsync(user, "Student");
            profile.UserId = user.Id;
            _context.StudentProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateStudentAccountAsync(string userId, string fullName, string studentId)
        {
            var profile = new StudentProfile { UserId = userId, StudentId = studentId };
            _context.StudentProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateSupervisorAccountAsync(ApplicationUser user, string password, SupervisorProfile profile)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return false;

            await _userManager.AddToRoleAsync(user, "Supervisor");
            profile.UserId = user.Id;
            _context.SupervisorProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateSupervisorAccountAsync(string userId, string fullName, string staffId)
        {
            var profile = new SupervisorProfile { UserId = userId, StaffId = staffId };
            _context.SupervisorProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSupervisorExpertiseAsync(int supervisorId, List<int> researchAreaIds)
        {
            var supervisor = await _context.SupervisorProfiles.Include(s => s.Expertise).FirstOrDefaultAsync(s => s.Id == supervisorId);
            if (supervisor == null) return false;

            // Remove existing
            _context.SupervisorResearchAreas.RemoveRange(supervisor.Expertise);
            
            // Add new
            foreach (var areaId in researchAreaIds)
            {
                supervisor.Expertise.Add(new SupervisorResearchArea { SupervisorId = supervisorId, ResearchAreaId = areaId });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync() => await _userManager.Users.ToListAsync();
    }
}
