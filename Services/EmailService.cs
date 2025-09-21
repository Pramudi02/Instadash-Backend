

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace AuthAPI.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = true;
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendPasswordResetEmailAsync(string to, string resetToken, string resetUrl);
        Task SendWelcomeEmailAsync(string to, string userName);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                Console.WriteLine($"🔍 Attempting to send email to: {to}");
                Console.WriteLine($"🔍 SMTP Server: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
                Console.WriteLine($"🔍 Sender Email: {_emailSettings.SenderEmail}");
                Console.WriteLine($"🔍 Use SSL: {_emailSettings.UseSsl}");
                
                // Validate email settings
                if (string.IsNullOrEmpty(_emailSettings.SenderEmail) || _emailSettings.SenderEmail.Contains("PLACEHOLDER"))
                {
                    throw new InvalidOperationException("Email sender not configured properly");
                }
                
                if (string.IsNullOrEmpty(_emailSettings.Password) || _emailSettings.Password.Contains("PLACEHOLDER"))
                {
                    throw new InvalidOperationException("Email password not configured properly");
                }

                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                
                message.To.Add(new MailAddress(to));

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                    EnableSsl = _emailSettings.UseSsl,
                    UseDefaultCredentials = false, // Important: Set this to false
                    Timeout = 30000,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                Console.WriteLine("🔍 Attempting SMTP connection...");
                await client.SendMailAsync(message);
                Console.WriteLine($"✅ Email sent successfully to {to}");
                _logger.LogInformation($"✅ Email sent successfully to {to}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($" SMTP Error: {ex.Message}");
                Console.WriteLine($"Status Code: {ex.StatusCode}");
                _logger.LogError($" SMTP failed to send email to {to}. SMTP Error: {ex.Message}, Status: {ex.StatusCode}");
                
                // Log for development, but still try to send in production
                _logger.LogInformation($"Would have sent email to: {to}");
                _logger.LogInformation($"Subject: {subject}");
                _logger.LogInformation($"Body: {body}");
                
                // Re-throw for now to see the specific error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" General email error: {ex.Message}");
                _logger.LogError($" Failed to send email to {to}. Error: {ex.Message}");
                
                // Log for development
                _logger.LogInformation($"Would have sent email to: {to}");
                _logger.LogInformation($"Subject: {subject}");
                _logger.LogInformation($"Body: {body}");
                
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetToken, string resetUrl)
        {
            var subject = "Password Reset Request - Allybees";
            
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Password Reset</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #00b8d4 0%, #0095b3 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ display: inline-block; background: #00b8d4; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .button:hover {{ background: #0095b3; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 14px; color: #666; }}
                        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔐 Password Reset Request</h1>
                            <p>Allybees Account Security</p>
                        </div>
                        <div class='content'>
                            <h2>Hello!</h2>
                            <p>We received a request to reset your password for your Allybees account.</p>
                            <p>Click the button below to reset your password:</p>
                            
                            <a href='{resetUrl}?token={resetToken}' class='button'>Reset My Password</a>
                            
                            <div class='warning'>
                                <strong>⚠️ Important:</strong>
                                <ul>
                                    <li>This link will expire in 1 hour for security reasons</li>
                                    <li>If you didn't request this reset, please ignore this email</li>
                                    <li>Never share this link with anyone</li>
                                </ul>
                            </div>
                            
                            <p>If the button doesn't work, copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; background: #f5f5f5; padding: 10px; border-radius: 5px;'>
                                {resetUrl}?token={resetToken}
                            </p>
                            
                            <p>If you have any questions, please contact our support team.</p>
                            
                            <p>Best regards,<br>The Allybees Team</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message. Please do not reply to this email.</p>
                            <p>© 2025 Allybees. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body, true);
        }

        public async Task SendWelcomeEmailAsync(string to, string userName)
        {
            var subject = "Welcome to Allybees!";
            
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #00b8d4 0%, #0095b3 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎉 Welcome to Allybees!</h1>
                        </div>
                        <div class='content'>
                            <h2>Hello {userName}!</h2>
                            <p>Welcome to Allybees! Your account has been successfully created.</p>
                            <p>If this activity is not yours contact immediately <a href=""mailto:allybees23@gmail.com"">allybees23@gmail.com</a></p>
                            <p>You can now access all the features of our platform.</p>
                            <p>Best regards,<br>The Allybees Team</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body, true);
        }
    }
}