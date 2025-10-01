using Microsoft.EntityFrameworkCore;
using FormDesigner.API.Models.Entities;

namespace FormDesigner.API.Data.Repositories;

/// <summary>
/// Repository implementation for form definition operations.
/// </summary>
public class FormRepository : IFormRepository
{
    private readonly ApplicationDbContext _context;

    public FormRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FormDefinitionEntity>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.FormDefinitions.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(f => f.IsActive);
        }

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<FormDefinitionEntity?> GetByIdAsync(string formId, bool includeInactive = false)
    {
        var query = _context.FormDefinitions.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(f => f.IsActive);
        }

        return await query.FirstOrDefaultAsync(f => f.FormId == formId);
    }

    public async Task<FormDefinitionEntity> CreateAsync(FormDefinitionEntity formDefinition)
    {
        formDefinition.CreatedAt = DateTime.UtcNow;
        formDefinition.IsActive = true;
        formDefinition.IsCommitted = false;

        _context.FormDefinitions.Add(formDefinition);
        await _context.SaveChangesAsync();

        return formDefinition;
    }

    public async Task<FormDefinitionEntity> UpdateAsync(FormDefinitionEntity formDefinition)
    {
        formDefinition.UpdatedAt = DateTime.UtcNow;

        _context.FormDefinitions.Update(formDefinition);
        await _context.SaveChangesAsync();

        return formDefinition;
    }

    public async Task<bool> DeleteAsync(string formId)
    {
        var form = await _context.FormDefinitions
            .FirstOrDefaultAsync(f => f.FormId == formId && f.IsActive);

        if (form == null)
        {
            return false;
        }

        // Soft delete
        form.IsActive = false;
        form.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string formId)
    {
        return await _context.FormDefinitions
            .AnyAsync(f => f.FormId == formId && f.IsActive);
    }

    public async Task<bool> CommitAsync(string formId)
    {
        var form = await _context.FormDefinitions
            .FirstOrDefaultAsync(f => f.FormId == formId && f.IsActive);

        if (form == null || form.IsCommitted)
        {
            return false;
        }

        form.IsCommitted = true;
        form.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<FormDefinitionEntity>> GetCommittedFormsAsync()
    {
        return await _context.FormDefinitions
            .Where(f => f.IsActive && f.IsCommitted)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }
}
