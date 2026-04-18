using GropMng.Core.Domain.Garden.Owners;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GropMng.Web.Initialization.Seeders;

/// <summary>
/// Seeds the default owner required by the baseline startup flow.
/// </summary>
internal sealed class OwnerSeeder
{
    private static readonly Guid DefaultOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private const string DefaultOwnerEmail = "owner@gropmng.local";

    private readonly GropContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnerSeeder"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public OwnerSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Seeds the default owner if it does not already exist.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous seed operation.</returns>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var owner = await _dbContext.Owners.FirstOrDefaultAsync(entity => entity.Email == DefaultOwnerEmail, cancellationToken);
        if (owner is not null)
            return;

        var now = DateTime.UtcNow;
        owner = new Owner
        {
            OwnerId = DefaultOwnerBusinessId,
            FirstName = "Default",
            LastName = "Owner",
            Email = DefaultOwnerEmail,
            PasswordHash = ComputeSha256("ChangeMe123!"),
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            IsDeleted = false
        };

        _dbContext.Owners.Add(owner);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeSha256(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }
}