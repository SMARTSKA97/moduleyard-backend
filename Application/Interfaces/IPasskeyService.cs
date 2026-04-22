using Fido2NetLib;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IPasskeyService
{
    Task<CredentialCreateOptions> RequestRegistrationAsync(string email);
    Task RegisterAsync(string email, AuthenticatorAttestationRawResponse attestation);
    Task<AssertionOptions> RequestLoginAsync(string email);
    Task VerifyLoginAsync(string email, AuthenticatorAssertionRawResponse assertion);
}
