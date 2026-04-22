using Application.Interfaces;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Infrastructure.Persistence.Models;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class PasskeyService : IPasskeyService
{
    private readonly IFido2 _fido2;
    private readonly IPasskeyCredentialRepository _credentialRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;

    public PasskeyService(
        IFido2 fido2, 
        IPasskeyCredentialRepository credentialRepository, 
        IUserRepository userRepository,
        IMemoryCache cache)
    {
        _fido2 = fido2;
        _credentialRepository = credentialRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<CredentialCreateOptions> RequestRegistrationAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null) throw new Exception("User not found");

        var fidoUser = new Fido2User
        {
            DisplayName = email,
            Name = email,
            Id = user.Id.ToByteArray()
        };

        var existingCredentials = await _credentialRepository.GetByUserIdAsync(user.Id);
        var excludeCredentials = new List<PublicKeyCredentialDescriptor>();
        foreach (var cred in existingCredentials)
        {
            excludeCredentials.Add(new PublicKeyCredentialDescriptor(cred.DescriptorId));
        }

        var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fidoUser,
            ExcludeCredentials = excludeCredentials,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                UserVerification = UserVerificationRequirement.Required,
                ResidentKey = ResidentKeyRequirement.Required
            },
            AttestationPreference = AttestationConveyancePreference.None
        });
        
        _cache.Set($"fido2.registration.{email}", options, TimeSpan.FromMinutes(5));

        return options;
    }

    public async Task RegisterAsync(string email, AuthenticatorAttestationRawResponse attestation)
    {
        if (!_cache.TryGetValue($"fido2.registration.{email}", out CredentialCreateOptions options))
            throw new Exception("Registration challenge not found or expired");

        var success = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = attestation,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = async (args, cancellationToken) =>
            {
                var exists = await _credentialRepository.GetByIdAsync(args.CredentialId);
                return exists == null;
            }
        });

        if (success == null) throw new Exception("Passkey verification failed");

        var user = await _userRepository.GetUserByEmailAsync(email);
        
        var newCredential = new PasskeyCredential
        {
            UserId = user!.Id,
            DescriptorId = success.Id,
            PublicKey = success.PublicKey,
            UserHandle = success.User.Id,
            SignatureCounter = success.SignCount,
            CredType = success.Type.ToString() ?? "public-key",
            RegDate = DateTime.UtcNow,
            AaGuid = success.AaGuid
        };

        await _credentialRepository.AddAsync(newCredential);
        _cache.Remove($"fido2.registration.{email}");
    }

    public async Task<AssertionOptions> RequestLoginAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null) throw new Exception("User not found");

        var existingCredentials = await _credentialRepository.GetByUserIdAsync(user.Id);
        var allowedCredentials = new List<PublicKeyCredentialDescriptor>();
        foreach (var cred in existingCredentials)
        {
            allowedCredentials.Add(new PublicKeyCredentialDescriptor(cred.DescriptorId));
        }

        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = allowedCredentials,
            UserVerification = UserVerificationRequirement.Discouraged
        });
        
        _cache.Set($"fido2.login.{email}", options, TimeSpan.FromMinutes(5));

        return options;
    }

    public async Task VerifyLoginAsync(string email, AuthenticatorAssertionRawResponse assertion)
    {
        if (!_cache.TryGetValue($"fido2.login.{email}", out AssertionOptions options))
            throw new Exception("Login challenge not found or expired");

        var user = await _userRepository.GetUserByEmailAsync(email);
        
        // Decode Base64Url manually
        var base64 = assertion.Id.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        var credentialIdBytes = Convert.FromBase64String(base64);
        var credential = await _credentialRepository.GetByIdAsync(credentialIdBytes);

        if (credential == null) throw new Exception("Credential not found");

        var success = await _fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = assertion,
            OriginalOptions = options,
            StoredPublicKey = credential.PublicKey,
            StoredSignatureCounter = credential.SignatureCounter,
            IsUserHandleOwnerOfCredentialIdCallback = async (args, cancellationToken) =>
            {
                var cred = await _credentialRepository.GetByIdAsync(args.CredentialId);
                return cred != null;
            }
        });

        if (success == null) throw new Exception("Passkey login verification failed");

        await _credentialRepository.UpdateCounterAsync(credentialIdBytes, success.SignCount);
        _cache.Remove($"fido2.login.{email}");
    }
}
