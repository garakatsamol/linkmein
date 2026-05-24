namespace LinkMeIn.Api.Services;

public interface ITokenEncryptionService
{
    string Protect(string value);
    string Unprotect(string protectedValue);
}
