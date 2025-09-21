using Backend.Models;            // For Automation model
using Backend.Services;          // For AutomationService
using System;
using System.Threading.Tasks;


public class ReportJobService
{
    private readonly AutomationService _automationService;
    private readonly ReportGenerator _reportGenerator;

    public ReportJobService(AutomationService automationService, ReportGenerator reportGenerator)
    {
        _automationService = automationService;
        _reportGenerator = reportGenerator;
    }

    public async Task ProcessScheduledReportsAsync()
    {
        var automations = await _automationService.GetAllAsync();
        var now = DateTime.Now;

        foreach (var auto in automations)
        {
            if (IsItTimeToRun(auto, now))
            {
                await _reportGenerator.GenerateAndSendAsync(auto);
            }
        }
    }

    private bool IsItTimeToRun(Automation auto, DateTime now)
    {
        if (auto.Frequency == "daily")
            return now.ToString("HH:mm") == auto.Time;
        if (auto.Frequency == "weekly")
            return now.DayOfWeek.ToString() == auto.DayOfWeek &&
                   now.ToString("HH:mm") == auto.Time;
        if (auto.Frequency == "monthly")
            return now.Day == auto.DayOfMonth &&
                   now.ToString("HH:mm") == auto.Time;

        return false;
    }
}
