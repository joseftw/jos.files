using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace JOS.Files.Functions;

public class UploadProgressHandler : IProgress<long>
{
    private bool _initialized;
    private readonly Stopwatch _stopwatch;
    private readonly ILogger<UploadProgressHandler> _logger;
    private DateTime _lastProgressLogged;

    public UploadProgressHandler(ILogger<UploadProgressHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _lastProgressLogged = DateTime.UtcNow;
        _stopwatch = new Stopwatch();
    }
        
    public void Report(long value)
    {
        if (!_initialized)
        {
            _initialized = true;
            _lastProgressLogged = DateTime.UtcNow;
            _stopwatch.Start();
        }
            
        if ((DateTime.UtcNow - _lastProgressLogged).TotalSeconds >= 30)
        {
            _lastProgressLogged = DateTime.UtcNow;
            var megabytes = (value > 0 ? value : 1) / 1048576;
            var seconds = _stopwatch.ElapsedMilliseconds / 1000d;
            var averageUploadMbps = Math.Round((value / seconds) / 125000d, 2);
            _logger.LogInformation("Progress: {UploadedMB}MB Average Upload: {AverageUpload}Mbps", megabytes, averageUploadMbps);
        }
    }
}