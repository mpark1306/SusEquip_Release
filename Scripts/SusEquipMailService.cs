using System;
using System.ServiceProcess;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SusEquipMailSender.Service
{
    public partial class SusEquipMailService : ServiceBase
    {
        private System.Timers.Timer? _timer;
        private readonly ILogger<SusEquipMailService> _logger;

        public SusEquipMailService()
        {
            InitializeComponent();
            ServiceName = "SusEquipMailService";
        }

        protected override void OnStart(string[] args)
        {
            // Calculate time until next first of month at 9:00 AM
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, 1, 9, 0, 0).AddMonths(1);
            
            // If we're already past the 1st of this month, schedule for next month
            if (now.Day > 1 || (now.Day == 1 && now.Hour >= 9))
            {
                nextRun = new DateTime(now.Year, now.Month, 1, 9, 0, 0).AddMonths(1);
            }
            else
            {
                nextRun = new DateTime(now.Year, now.Month, 1, 9, 0, 0);
            }

            var timeUntilNextRun = nextRun - now;
            
            // Set up timer to trigger on the calculated time
            _timer = new System.Timers.Timer(timeUntilNextRun.TotalMilliseconds);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = false; // Only run once, then recalculate
            _timer.Start();

            LogMessage($"Service started. Next run scheduled for: {nextRun:yyyy-MM-dd HH:mm:ss}");
        }

        protected override void OnStop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            LogMessage("Service stopped.");
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                LogMessage("Starting monthly mail sending task...");
                
                // Run the mail sending logic
                ExecuteMailSending();
                
                LogMessage("Monthly mail sending task completed.");
                
                // Schedule next run for first of next month
                ScheduleNextRun();
            }
            catch (Exception ex)
            {
                LogMessage($"Error during mail sending: {ex.Message}");
            }
        }

        private void ExecuteMailSending()
        {
            // Build configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Get connection string
            string connectionString = config.GetConnectionString("SusEquipDb");
            DatabaseHelper.ConnectionString = connectionString;

            // Create service and send mails
            var equipmentService = new EquipmentService();
            var mailSender = new MailSender();
            mailSender.SendExpirationMails(equipmentService);
        }

        private void ScheduleNextRun()
        {
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, 1, 9, 0, 0).AddMonths(1);
            var timeUntilNextRun = nextRun - now;

            if (_timer != null)
            {
                _timer.Interval = timeUntilNextRun.TotalMilliseconds;
                _timer.Start();
            }

            LogMessage($"Next run scheduled for: {nextRun:yyyy-MM-dd HH:mm:ss}");
        }

        private void LogMessage(string message)
        {
            // Log to Windows Event Log
            var eventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("SusEquipMailService"))
            {
                System.Diagnostics.EventLog.CreateEventSource("SusEquipMailService", "Application");
            }
            eventLog.Source = "SusEquipMailService";
            eventLog.WriteEntry(message, System.Diagnostics.EventLogEntryType.Information);
            
            // Also log to console if running in debug mode
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }
    }
}
