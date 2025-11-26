namespace CloudAPI.Services;

public interface ICertificateValidator
{
    bool IsValid(X509Certificate2 cert);
}