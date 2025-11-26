namespace CloudAPI.Services;

public class DeviceCertificateValidator : ICertificateValidator
{
    private readonly HashSet<string> _allowedThumbprints;

    public DeviceCertificateValidator(IConfiguration config)
    {
        _allowedThumbprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "B222DBD0AE450CF38961C300DFA467997AAF2520",
            "8289EFCB4F325B37817D9F65239ECC0A1EFAA122"
        };
    }

    public bool IsValid(X509Certificate2? cert)
    {
        return cert != null && _allowedThumbprints.Contains(cert.Thumbprint);
    }
}