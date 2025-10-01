using Microsoft.EntityFrameworkCore;
using FormDesigner.API.Models.Entities;

namespace FormDesigner.API.Data.Repositories;

/// <summary>
/// Repository implementation for form instance operations (runtime mode).
/// </summary>
public class FormInstanceRepository : IFormInstanceRepository
{
    private readonly ApplicationDbContext _context;

    public FormInstanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FormInstanceEntity> CreateInstanceAsync(string formId, string? userId = null)
    {
        var instance = new FormInstanceEntity
        {
            InstanceId = Guid.NewGuid(),
            FormId = formId,
            Status = "draft",
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        _context.FormInstances.Add(instance);
        await _context.SaveChangesAsync();

        return instance;
    }

    public async Task<FormInstanceEntity?> GetInstanceAsync(Guid instanceId)
    {
        return await _context.FormInstances
            .Include(i => i.FormDefinition)
            .FirstOrDefaultAsync(i => i.InstanceId == instanceId);
    }

    public async Task<List<FormInstanceEntity>> GetInstancesByFormIdAsync(string formId, string? userId = null)
    {
        var query = _context.FormInstances
            .Where(i => i.FormId == formId);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(i => i.UserId == userId);
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<TemporaryStateEntity> SaveProgressAsync(Guid instanceId, string dataJson, string? userId = null)
    {
        var state = new TemporaryStateEntity
        {
            StateId = Guid.NewGuid(),
            InstanceId = instanceId,
            DataJson = dataJson,
            SavedAt = DateTime.UtcNow,
            UserId = userId
        };

        _context.TemporaryStates.Add(state);
        await _context.SaveChangesAsync();

        return state;
    }

    public async Task<TemporaryStateEntity?> GetLatestProgressAsync(Guid instanceId)
    {
        return await _context.TemporaryStates
            .Where(s => s.InstanceId == instanceId)
            .OrderByDescending(s => s.SavedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<FormInstanceEntity> SubmitInstanceAsync(Guid instanceId, string dataJson)
    {
        var instance = await _context.FormInstances
            .FirstOrDefaultAsync(i => i.InstanceId == instanceId);

        if (instance == null)
        {
            throw new InvalidOperationException($"Form instance {instanceId} not found");
        }

        if (instance.Status == "submitted")
        {
            throw new InvalidOperationException($"Form instance {instanceId} already submitted");
        }

        instance.Status = "submitted";
        instance.DataJson = dataJson;
        instance.SubmittedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return instance;
    }

    public async Task<bool> DeleteInstanceAsync(Guid instanceId)
    {
        var instance = await _context.FormInstances
            .FirstOrDefaultAsync(i => i.InstanceId == instanceId);

        if (instance == null || instance.Status == "submitted")
        {
            return false;
        }

        // Delete related temporary states first (cascade will handle this, but explicit for clarity)
        var states = await _context.TemporaryStates
            .Where(s => s.InstanceId == instanceId)
            .ToListAsync();
        _context.TemporaryStates.RemoveRange(states);

        _context.FormInstances.Remove(instance);
        await _context.SaveChangesAsync();

        return true;
    }
}
