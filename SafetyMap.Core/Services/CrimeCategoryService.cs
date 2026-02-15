using Microsoft.EntityFrameworkCore;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs.CrimeCategory;
using SafetyMapData;
using SafetyMapData.Entities;

namespace SafetyMap.Core.Services
{
    public class CrimeCategoryService : ICrimeCategoryService
    {
        private readonly SafetyMapDbContext _context;

        public CrimeCategoryService(SafetyMapDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CrimeCategoryDTO>> GetAllAsync()
        {
            return await _context.CrimeCategories
                .Select(c => new CrimeCategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ColorCode = c.ColorCode
                })
                .ToListAsync();
        }

        public async Task<CrimeCategoryDTO?> GetByIdAsync(Guid id)
        {
            return await _context.CrimeCategories
                .Where(c => c.Id == id)
                .Select(c => new CrimeCategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ColorCode = c.ColorCode
                })
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CrimeCategoryCreateDTO dto)
        {
            var crimeCategory = new CrimeCategory
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ColorCode = dto.ColorCode
            };

            _context.CrimeCategories.Add(crimeCategory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CrimeCategoryEditDTO dto)
        {
            var crimeCategory = await _context.CrimeCategories.FindAsync(dto.Id);

            if (crimeCategory == null)
            {
                throw new ArgumentException($"CrimeCategory with Id '{dto.Id}' was not found.");
            }

            crimeCategory.Name = dto.Name;
            crimeCategory.ColorCode = dto.ColorCode;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var crimeCategory = await _context.CrimeCategories.FindAsync(id);

            if (crimeCategory != null)
            {
                _context.CrimeCategories.Remove(crimeCategory);
                await _context.SaveChangesAsync();
            }
        }
    }
}
