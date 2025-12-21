namespace CloudProject.API.Services;

public class DeviceCertificateValidator : ICertificateValidator
{
    private readonly DeviceManager _deviceManager;

    public DeviceCertificateValidator(DeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
    }

    public async Task<bool> IsValidAsync(X509Certificate2? cert)
    {
        return cert != null && (await _deviceManager.GetByFingerprintAsync(Convert.FromHexString(cert.Thumbprint))) != null;
    }

    public async Task<bool> IsValidAsync(string? thumbprint)
    {
        return !string.IsNullOrEmpty(thumbprint) && (await _deviceManager.GetByFingerprintAsync(Convert.FromHexString(thumbprint))) != null;
    }
}