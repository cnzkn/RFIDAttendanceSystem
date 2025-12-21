namespace CloudProject.Core;

public interface ICertificateValidator
{
    Task<bool> IsValidAsync(X509Certificate2 cert);
    Task<bool> IsValidAsync(string thumbprint);
}