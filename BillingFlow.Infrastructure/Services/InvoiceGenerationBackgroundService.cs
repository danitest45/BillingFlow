using BillingFlow.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Infrastructure.Services
{
    public class InvoiceGenerationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InvoiceGenerationBackgroundService> _logger;

        public InvoiceGenerationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<InvoiceGenerationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("InvoiceGenerationBackgroundService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var automationService = scope.ServiceProvider
                        .GetRequiredService<IInvoiceAutomationService>();

                    var created = await automationService
                        .GenerateMissingInvoicesForCurrentMonthAsync(stoppingToken);

                    _logger.LogInformation(
                        "Execução da rotina diária concluída. {Count} cobranças criadas.",
                        created);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro na rotina diária de geração de cobranças.");
                }

                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1).AddHours(3); // 03:00 UTC do próximo dia
                var delay = nextRun - now;

                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.FromHours(24);
                }

                _logger.LogInformation(
                    "Próxima execução agendada para {NextRunUtc}.",
                    nextRun);

                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogInformation("InvoiceGenerationBackgroundService finalizado.");
        }
    }
}
