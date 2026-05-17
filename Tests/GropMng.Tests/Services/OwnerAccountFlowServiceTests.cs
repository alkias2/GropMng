using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using GropMng.Core.Domain.Configuration;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Configuration;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Services.Services.User;
using Microsoft.AspNetCore.DataProtection;
using Moq;

namespace GropMng.Tests.Services;

/// <summary>
/// Covers registration, email confirmation, and password reset flows for owner accounts.
/// </summary>
public class OwnerAccountFlowServiceTests
{
    [Fact]
    public async Task RegisterAsync_WhenEmailConfirmationIsRequired_CreatesPendingOwnerAndReturnsConfirmationToken()
    {
        // Arrange
        var ownerRepository = new Mock<IRepository<Owner>>();
        var roleRepository = new Mock<IRepository<OwnerRole>>();
        var passwordRepository = new Mock<IRepository<OwnerPassword>>();
        var settingService = new Mock<ISettingService>();
        var passwordService = new OwnerPasswordService();
        Owner? createdOwner = null;

        settingService
            .Setup(service => service.LoadAsync<GropOwnerRegistrationSettings>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GropOwnerRegistrationSettings
            {
                RequireEmailConfirmation = true,
                PasswordResetTokenExpirationHours = 24
            });

        ownerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Owner, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Owner?)null);

        roleRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<OwnerRole, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OwnerRole
            {
                Id = 7,
                Name = "Registered Owner",
                SystemName = "RegisteredOwner",
                IsActive = true
            });

        ownerRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<Owner>(), true, It.IsAny<CancellationToken>()))
            .Callback<Owner, bool, CancellationToken>((owner, _, _) =>
            {
                owner.Id = 12;
                createdOwner = owner;
            })
            .ReturnsAsync((Owner owner, bool _, CancellationToken _) => owner);

        passwordRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<OwnerPassword>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OwnerPassword password, bool _, CancellationToken _) => password);

        var service = new OwnerAccountFlowService(
            ownerRepository.Object,
            roleRepository.Object,
            passwordRepository.Object,
            settingService.Object,
            passwordService,
            DataProtectionProvider.Create("GropMng.OwnerAccountFlow.Tests"));

        // Act
        var result = await service.RegisterAsync(new OwnerRegistrationRequest(
            "Alice",
            "Gardener",
            "Alice Gardener",
            "alice@example.com",
            "StrongPass123!"));

        // Assert
        Assert.NotNull(createdOwner);
        Assert.True(result.RequiresEmailConfirmation);
        Assert.False(result.Owner.IsEmailConfirmed);
        Assert.Equal(OwnerAccountStatus.PendingActivation, result.Owner.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.EmailConfirmationToken));
        Assert.Contains(result.Owner.OwnerRoles, role => role.SystemName == "RegisteredOwner");
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenTokenIsValid_ActivatesOwner()
    {
        // Arrange
        var owner = new Owner
        {
            Id = 15,
            OwnerId = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Gardener",
            DisplayName = "Alice Gardener",
            Email = "alice@example.com",
            PasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("StrongPass123!"))),
            Status = OwnerAccountStatus.PendingActivation,
            IsEmailConfirmed = false,
            IsActive = true
        };

        var ownerRepository = new Mock<IRepository<Owner>>();
        var roleRepository = new Mock<IRepository<OwnerRole>>();
        var passwordRepository = new Mock<IRepository<OwnerPassword>>();
        var settingService = new Mock<ISettingService>();
        var passwordService = new OwnerPasswordService();

        ownerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Owner, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        ownerRepository
            .Setup(repository => repository.UpdateAsync(owner, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        var service = new OwnerAccountFlowService(
            ownerRepository.Object,
            roleRepository.Object,
            passwordRepository.Object,
            settingService.Object,
            passwordService,
            DataProtectionProvider.Create("GropMng.OwnerAccountFlow.Tests"));

        var token = service.GenerateEmailConfirmationToken(owner);

        // Act
        var confirmed = await service.ConfirmEmailAsync(owner.Email, token);

        // Assert
        Assert.True(confirmed);
        Assert.True(owner.IsEmailConfirmed);
        Assert.Equal(OwnerAccountStatus.Active, owner.Status);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenTokenIsValid_UpdatesTheStoredPassword()
    {
        // Arrange
        var oldPasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("OldPass123!")));
        var owner = new Owner
        {
            Id = 21,
            OwnerId = Guid.NewGuid(),
            FirstName = "Bob",
            LastName = "Grower",
            DisplayName = "Bob Grower",
            Email = "bob@example.com",
            PasswordHash = oldPasswordHash,
            Status = OwnerAccountStatus.Active,
            IsEmailConfirmed = true,
            IsActive = true
        };

        var currentPassword = new OwnerPassword
        {
            Id = 4,
            OwnerId = owner.Id,
            PasswordHash = oldPasswordHash,
            PasswordSalt = string.Empty,
            IsCurrent = true,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        };

        var ownerRepository = new Mock<IRepository<Owner>>();
        var roleRepository = new Mock<IRepository<OwnerRole>>();
        var passwordRepository = new Mock<IRepository<OwnerPassword>>();
        var settingService = new Mock<ISettingService>();
        var passwordService = new OwnerPasswordService();
        OwnerPassword? createdPassword = null;

        settingService
            .Setup(service => service.LoadAsync<GropOwnerRegistrationSettings>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GropOwnerRegistrationSettings
            {
                RequireEmailConfirmation = false,
                PasswordResetTokenExpirationHours = 24
            });

        ownerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Owner, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        ownerRepository
            .Setup(repository => repository.UpdateAsync(owner, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        passwordRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<OwnerPassword, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPassword);

        passwordRepository
            .Setup(repository => repository.UpdateAsync(currentPassword, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPassword);

        passwordRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<OwnerPassword>(), true, It.IsAny<CancellationToken>()))
            .Callback<OwnerPassword, bool, CancellationToken>((password, _, _) => createdPassword = password)
            .ReturnsAsync((OwnerPassword password, bool _, CancellationToken _) => password);

        var service = new OwnerAccountFlowService(
            ownerRepository.Object,
            roleRepository.Object,
            passwordRepository.Object,
            settingService.Object,
            passwordService,
            DataProtectionProvider.Create("GropMng.OwnerAccountFlow.Tests"));

        var request = await service.RequestPasswordResetAsync(owner.Email);

        // Act
        var resetWorked = await service.ResetPasswordAsync(owner.Email, request.ResetToken!, "NewStrongPass123!");

        // Assert
        Assert.True(resetWorked);
        Assert.False(currentPassword.IsCurrent);
        Assert.NotNull(createdPassword);
        Assert.True(createdPassword!.IsCurrent);
        Assert.True(passwordService.VerifyPassword("NewStrongPass123!", createdPassword.PasswordHash, createdPassword.PasswordSalt));
        Assert.Equal(createdPassword.PasswordHash, owner.PasswordHash);
    }

    [Fact]
    public async Task ChangeOwnerPasswordAsync_WhenOwnerExists_RollsCurrentPasswordAndCreatesNewCurrentRecord()
    {
        // Arrange
        var owner = new Owner
        {
            Id = 55,
            OwnerId = Guid.NewGuid(),
            FirstName = "Maria",
            LastName = "Green",
            DisplayName = "Maria Green",
            Email = "maria@example.com",
            PasswordHash = "legacy-hash",
            Status = OwnerAccountStatus.Active,
            IsEmailConfirmed = true,
            IsActive = true
        };

        var currentPassword = new OwnerPassword
        {
            Id = 11,
            OwnerId = owner.Id,
            PasswordHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("OldPass123!"))),
            PasswordSalt = string.Empty,
            IsCurrent = true,
            PasswordResetToken = "token",
            PasswordResetTokenExpiresAtUtc = DateTime.UtcNow.AddHours(3),
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        var ownerRepository = new Mock<IRepository<Owner>>();
        var roleRepository = new Mock<IRepository<OwnerRole>>();
        var passwordRepository = new Mock<IRepository<OwnerPassword>>();
        var settingService = new Mock<ISettingService>();
        var passwordService = new OwnerPasswordService();
        OwnerPassword? createdPassword = null;

        ownerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Owner, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        ownerRepository
            .Setup(repository => repository.UpdateAsync(owner, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        passwordRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<OwnerPassword, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPassword);

        passwordRepository
            .Setup(repository => repository.UpdateAsync(currentPassword, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPassword);

        passwordRepository
            .Setup(repository => repository.CreateAsync(It.IsAny<OwnerPassword>(), true, It.IsAny<CancellationToken>()))
            .Callback<OwnerPassword, bool, CancellationToken>((password, _, _) => createdPassword = password)
            .ReturnsAsync((OwnerPassword password, bool _, CancellationToken _) => password);

        var service = new OwnerAccountFlowService(
            ownerRepository.Object,
            roleRepository.Object,
            passwordRepository.Object,
            settingService.Object,
            passwordService,
            DataProtectionProvider.Create("GropMng.OwnerAccountFlow.Tests"));

        // Act
        var result = await service.ChangeOwnerPasswordAsync(new ChangeOwnerPasswordRequest(owner.OwnerId, "NewStrongPass456!"));

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Errors);
        Assert.False(currentPassword.IsCurrent);
        Assert.Null(currentPassword.PasswordResetToken);
        Assert.Null(currentPassword.PasswordResetTokenExpiresAtUtc);
        Assert.NotNull(createdPassword);
        Assert.True(createdPassword!.IsCurrent);
        Assert.Equal(owner.Id, createdPassword.OwnerId);
        Assert.True(passwordService.VerifyPassword("NewStrongPass456!", createdPassword.PasswordHash, createdPassword.PasswordSalt));
        Assert.Equal(createdPassword.PasswordHash, owner.PasswordHash);
    }

    [Fact]
    public async Task ChangeOwnerPasswordAsync_WhenNewPasswordMatchesCurrent_ReturnsValidationError()
    {
        // Arrange
        var passwordService = new OwnerPasswordService();
        var existingHash = passwordService.HashPassword("SamePass123!");

        var owner = new Owner
        {
            Id = 67,
            OwnerId = Guid.NewGuid(),
            FirstName = "Nikos",
            LastName = "Fields",
            DisplayName = "Nikos Fields",
            Email = "nikos@example.com",
            PasswordHash = existingHash.Hash,
            Status = OwnerAccountStatus.Active,
            IsEmailConfirmed = true,
            IsActive = true
        };

        var currentPassword = new OwnerPassword
        {
            Id = 13,
            OwnerId = owner.Id,
            PasswordHash = existingHash.Hash,
            PasswordSalt = existingHash.Salt,
            IsCurrent = true,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        var ownerRepository = new Mock<IRepository<Owner>>();
        var roleRepository = new Mock<IRepository<OwnerRole>>();
        var passwordRepository = new Mock<IRepository<OwnerPassword>>();
        var settingService = new Mock<ISettingService>();

        ownerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Owner, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(owner);

        passwordRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<OwnerPassword, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPassword);

        var service = new OwnerAccountFlowService(
            ownerRepository.Object,
            roleRepository.Object,
            passwordRepository.Object,
            settingService.Object,
            passwordService,
            DataProtectionProvider.Create("GropMng.OwnerAccountFlow.Tests"));

        // Act
        var result = await service.ChangeOwnerPasswordAsync(new ChangeOwnerPasswordRequest(owner.OwnerId, "SamePass123!"));

        // Assert
        Assert.False(result.Success);
        Assert.Contains("admin.owner.password.validation.newpassword.different", result.Errors);
        passwordRepository.Verify(repository => repository.CreateAsync(It.IsAny<OwnerPassword>(), true, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangeOwnerPasswordAsync_WhenOwnerIsMissing_ReturnsOwnerNotFoundError()
    {
        // Arrange
        var ownerRepository = new Mock<IRepository<Owner>>();
        var roleRepository = new Mock<IRepository<OwnerRole>>();
        var passwordRepository = new Mock<IRepository<OwnerPassword>>();
        var settingService = new Mock<ISettingService>();
        var passwordService = new OwnerPasswordService();

        ownerRepository
            .Setup(repository => repository.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Owner, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Owner?)null);

        var service = new OwnerAccountFlowService(
            ownerRepository.Object,
            roleRepository.Object,
            passwordRepository.Object,
            settingService.Object,
            passwordService,
            DataProtectionProvider.Create("GropMng.OwnerAccountFlow.Tests"));

        // Act
        var result = await service.ChangeOwnerPasswordAsync(new ChangeOwnerPasswordRequest(Guid.NewGuid(), "AnyPass123!"));

        // Assert
        Assert.False(result.Success);
        Assert.Contains("admin.owner.password.validation.owner.notfound", result.Errors);
    }
}
