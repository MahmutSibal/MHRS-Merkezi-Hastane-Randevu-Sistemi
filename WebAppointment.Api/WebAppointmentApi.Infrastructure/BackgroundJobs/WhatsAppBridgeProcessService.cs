using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebAppointmentApi.Infrastructure.Messaging;

namespace WebAppointmentApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Spawns and supervises the mhrs-whatsapp-bot Node.js process (wppconnect bridge) so it
/// doesn't need to be started manually in a separate terminal. Skips silently if the bridge
/// is already reachable (e.g. someone started it by hand) or its folder isn't found.
/// </summary>
public sealed class WhatsAppBridgeProcessService : IHostedService
{
    private readonly WhatsAppBridgeOptions _options;
    private readonly IHostEnvironment _env;
    private readonly ILogger<WhatsAppBridgeProcessService> _logger;
    private Process? _process;

    public WhatsAppBridgeProcessService(
        IOptions<WhatsAppBridgeOptions> options, IHostEnvironment env, ILogger<WhatsAppBridgeProcessService> logger)
    {
        _options = options.Value;
        _env = env;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        if (!_options.AutoStart)
        {
            _logger.LogInformation("WhatsApp bridge auto-start disabled (WhatsAppBridge:AutoStart=false).");
            return;
        }

        if (await IsAlreadyReachableAsync(ct))
        {
            _logger.LogInformation("WhatsApp bridge already reachable at {BaseUrl}; not starting a new one.", _options.BaseUrl);
            return;
        }

        var workingDirectory = Path.GetFullPath(Path.Combine(_env.ContentRootPath, _options.WorkingDirectory));
        if (!Directory.Exists(workingDirectory) || !File.Exists(Path.Combine(workingDirectory, "index.js")))
        {
            _logger.LogWarning(
                "WhatsApp bridge folder not found at {Dir}; skipping auto-start. Set WhatsAppBridge:WorkingDirectory or start it manually.",
                workingDirectory);
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = "index.js",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            startInfo.Environment["DOTNET_API_BASE_URL"] = "http://localhost:5233";
            startInfo.Environment["BRIDGE_WEBHOOK_SECRET"] = _options.WebhookSecret;

            _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            _process.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data)) _logger.LogInformation("[whatsapp-bridge] {Line}", e.Data);
            };
            _process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data)) _logger.LogWarning("[whatsapp-bridge] {Line}", e.Data);
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            _logger.LogInformation("WhatsApp bridge process started (PID {Pid}) in {Dir}.", _process.Id, workingDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start WhatsApp bridge process. QR/WhatsApp features will be unavailable until it's started manually.");
        }
    }

    public Task StopAsync(CancellationToken ct)
    {
        if (_process is { HasExited: false })
        {
            try
            {
                _process.Kill(entireProcessTree: true);
                _logger.LogInformation("WhatsApp bridge process stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to stop WhatsApp bridge process cleanly.");
            }
        }

        return Task.CompletedTask;
    }

    private async Task<bool> IsAlreadyReachableAsync(CancellationToken ct)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            using var response = await http.GetAsync(new Uri(new Uri(_options.BaseUrl), "/status"), ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
