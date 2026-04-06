using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasySheba.Data;
using EasySheba.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EasySheba.Services;

namespace EasySheba.Services
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentReminderService> _logger;
        private readonly IEmailService _emailService;

        public AppointmentReminderService(IServiceProvider serviceProvider, ILogger<AppointmentReminderService> logger, IEmailService emailService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _emailService = emailService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var now = DateTime.Now;
                        var oneHourLater = now.AddHours(1);

                        // Get approved appointments that have not yet had a reminder sent,
                        // and whose scheduled datetime is between now and one hour later.
                        var appointments = await db.Appointments
                            .Where(a => a.Status == "Approved" &&
                                        !a.ReminderSent &&
                                        a.Email != null &&
                                        !string.IsNullOrEmpty(a.Email))
                            .ToListAsync();

                        foreach (var appt in appointments)
                        {
                            // Parse time (assuming format like "10:00")
                            if (TimeSpan.TryParse(appt.AppointmentTime, out var apptTime))
                            {
                                var apptDateTime = appt.AppointmentDate.Date + apptTime;
                                // Send only if the appointment is within the next hour (and not in the past)
                                if (apptDateTime > now && apptDateTime <= oneHourLater)
                                {
                                    // Send reminder
                                    var message = new ContactMessage
                                    {
                                        Name = appt.PatientName,
                                        Email = appt.Email,
                                        Subject = "Appointment Reminder - EasySheba",
                                        Message = $"Dear {appt.PatientName},\n\nThis is a reminder for your appointment at {appt.AppointmentTime} on {appt.AppointmentDate:dd/MM/yyyy}. Please be on time.\n\nThank you,\nEasySheba Team"
                                    };
                                    await _emailService.SendContactEmailAsync(message);

                                    // Mark reminder as sent so we don't send it again
                                    appt.ReminderSent = true;
                                    await db.SaveChangesAsync();
                                    _logger.LogInformation($"Reminder sent to {appt.Email} for appointment ID {appt.Id}");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Could not parse appointment time for ID {appt.Id}: {appt.AppointmentTime}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AppointmentReminderService");
                }

                // Wait 5 minutes before next check
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}