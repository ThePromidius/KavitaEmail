﻿using System;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Skeleton.Controllers;

namespace Skeleton.Services;

public interface IValidationService
{
    Task<bool> ValidateInstall(string installId);
}

public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;
    private readonly IConfiguration _configuration;
    private const string StatsApiUrl = "https://stats.kavitareader.com";

    public ValidationService(ILogger<ValidationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        FlurlHttp.ConfigureClient(StatsApiUrl, cli =>
            cli.Settings.HttpClientFactory = new UntrustedCertClientFactory());
    }

    /// <summary>
    /// If this is not enabled in IConfiguration, this will return true
    /// </summary>
    /// <param name="installId">Kavita Install Id to validate</param>
    /// <returns></returns>
    public async Task<bool> ValidateInstall(string installId)
    {
        if (!_configuration.GetValue<bool>("EnableValidation"))
        {
            return true;
        }

        return await SendEmailWithGet(StatsApiUrl + "/api/v2/stats/validate?installId=" + installId);
    }
    
    private async Task<bool> SendEmailWithGet(string url)
    {
        try
        {
            var response = await (url)
                .WithHeader("Accept", "application/json")
                .WithHeader("User-Agent", "Kavita")
                .WithHeader("x-api-key", "MsnvA2DfQqxSK5jh")
                .WithHeader("Content-Type", "application/json")
                .WithTimeout(TimeSpan.FromSeconds(30))
                .GetStringAsync();

            if (!string.IsNullOrEmpty(response) && bool.Parse(response))
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured when validating install");
            return false;
        }
        return false;
    }
}