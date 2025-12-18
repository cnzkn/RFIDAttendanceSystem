namespace CloudProject.Core;

public interface ICertificateValidator
{
    bool IsValid(X509Certificate2 cert);
}