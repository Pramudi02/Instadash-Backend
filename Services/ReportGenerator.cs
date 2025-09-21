using Backend.Models;
using Backend.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using Microsoft.Extensions.Configuration;

public class ReportGenerator
{
    private readonly MongoDBService _mongoService;

    public ReportGenerator(MongoDBService mongoService)
    {
        _mongoService = mongoService;
    }

    public async Task GenerateAndSendAsync(Automation automation)
    {
        var collection = _mongoService.GetCollectionByType(automation.ReportType);

         var filter = Builders<BsonDocument>.Filter.Eq("CompanyId", automation.CompanyId);

        var reportData = await collection.Find(filter).ToListAsync(); // ✅ correct

        byte[] fileBytes;
        string fileName;

        if (automation.Format.ToLower() == "excel")
        {
            fileBytes = GenerateExcel(reportData, out fileName, automation.ReportType);
        }
        else
        {
            fileBytes = GeneratePdf(reportData, out fileName);
        }

        await SendEmailAsync(automation.Emails, automation.Subject, automation.Message, fileBytes, fileName);
    }

    private byte[] GenerateExcel(List<BsonDocument> data, out string fileName, string reportType)
    {
        fileName = $"{reportType}_Report.xlsx";
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Report");

        if (data.Count == 0)
            return package.GetAsByteArray();

        var keys = data[0].Names.ToList();

        for (int i = 0; i < keys.Count; i++)
        {
            sheet.Cells[1, i + 1].Value = keys[i];
        }

        for (int row = 0; row < data.Count; row++)
        {
            var doc = data[row];
            for (int col = 0; col < keys.Count; col++)
            {
                var value = doc.GetValue(keys[col], BsonNull.Value);
                sheet.Cells[row + 2, col + 1].Value = value.ToString();
            }
        }

        return package.GetAsByteArray();
    }

    private byte[] GeneratePdf(List<BsonDocument> data, out string fileName)
{
    fileName = "Report.pdf";
    var keys = data.Count > 0 ? data[0].Names.ToList() : new List<string>();

    var topic = "Generated Report";
    var dateText = $"Date: {DateTime.Now:yyyy-MM-dd}";

    var doc = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(12));

            page.Header().Column(header =>
            {
                header.Item().Text(topic).FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);
                header.Item().Text(dateText).FontSize(10).FontColor(Colors.Grey.Darken2);
                header.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    foreach (var _ in keys)
                        columns.RelativeColumn();
                });

                if (keys.Count == 0) return;

                table.Header(header =>
                {
                    foreach (var key in keys)
                    {
                        header.Cell().Element(CellStyle).Text(key).SemiBold().FontSize(12);
                    }
                });

                foreach (var doc in data)
                {
                    foreach (var key in keys)
                    {
                        var value = doc.Contains(key) ? doc[key]?.ToString() ?? "" : "";
                        table.Cell().Element(CellStyle).Text(value);
                    }
                }

                static IContainer CellStyle(IContainer container)
                {
                    return container
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(5)
                        .AlignLeft();
                }
            });
        });
    });

    using var ms = new MemoryStream();
    doc.GeneratePdf(ms);
    return ms.ToArray();
}


    private async Task SendEmailAsync(string recipientEmail, string subject, string message, byte[] attachment, string filename)
    {
        try
        {
            var emailUser = Environment.GetEnvironmentVariable("EMAIL_USER");
            var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

            Console.WriteLine($"[DEBUG] EMAIL_USER: '{emailUser}'");
            Console.WriteLine($"[DEBUG] EMAIL_PASSWORD is null or empty: {string.IsNullOrEmpty(emailPassword)}");

            if (string.IsNullOrEmpty(emailUser) || string.IsNullOrEmpty(emailPassword))
                throw new InvalidOperationException("Email credentials are missing in environment variables.");

            using var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(emailUser, emailPassword),
                EnableSsl = true
            };

            var mail = new MailMessage(emailUser, recipientEmail)
            {
                Subject = subject,
                Body = message
            };

            mail.Attachments.Add(new Attachment(new MemoryStream(attachment), filename));
            await smtpClient.SendMailAsync(mail);

            Console.WriteLine($"✅ Email sent successfully to {recipientEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send email: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
    }
}
