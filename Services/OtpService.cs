using Backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text.Json;
using AuthAPI.Services;

namespace Backend.Services
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string email, string purpose, string? tempUserData = null);
        Task<bool> VerifyOtpAsync(string email, string otpCode, string purpose);
        Task<string?> GetTempUserDataAsync(string email, string purpose);
        Task CleanupExpiredOtpsAsync();
        Task<bool> ResendOtpAsync(string email, string purpose);
    }

    public class OtpService : IOtpService
    {
        private readonly IMongoCollection<OtpVerification> _otpCollection;
        private readonly IEmailService _emailService;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            IOptions<Backend.Models.MongoDBSettings> mongoDbSettings, 
            IEmailService emailService,
            ILogger<OtpService> logger)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.AdminDatabaseName);
            _otpCollection = mongoDatabase.GetCollection<OtpVerification>("otpVerifications");
            _emailService = emailService;
            _logger = logger;

            // Create index for automatic cleanup of expired OTPs
            var indexKeys = Builders<OtpVerification>.IndexKeys.Ascending(x => x.ExpiresAt);
            var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
            _otpCollection.Indexes.CreateOne(new CreateIndexModel<OtpVerification>(indexKeys, indexOptions));
        }

        public async Task<string> GenerateOtpAsync(string email, string purpose, string? tempUserData = null)
        {
            try
            {
                // Clean up any existing OTPs for this email and purpose
                await _otpCollection.DeleteManyAsync(x => x.Email == email && x.Purpose == purpose);

                // Generate 6-digit OTP
                var otpCode = GenerateRandomOtp();
                
                var otpVerification = new OtpVerification
                {
                    Email = email,
                    OtpCode = otpCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10), // OTP expires in 10 minutes
                    Purpose = purpose,
                    TempUserData = tempUserData ?? string.Empty
                };

                await _otpCollection.InsertOneAsync(otpVerification);

                // Send OTP email
                await SendOtpEmail(email, otpCode, purpose);

                _logger.LogInformation($"OTP generated and sent to {email} for purpose: {purpose}");
                return otpCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating OTP for {email}");
                throw;
            }
        }

        public async Task<bool> VerifyOtpAsync(string email, string otpCode, string purpose)
        {
            try
            {
                var filter = Builders<OtpVerification>.Filter.And(
                    Builders<OtpVerification>.Filter.Eq(x => x.Email, email),
                    Builders<OtpVerification>.Filter.Eq(x => x.Purpose, purpose),
                    Builders<OtpVerification>.Filter.Eq(x => x.IsUsed, false)
                );

                var otpVerification = await _otpCollection.Find(filter)
                    .SortByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otpVerification == null)
                {
                    _logger.LogWarning($"No valid OTP found for {email} with purpose {purpose}");
                    return false;
                }

                // Increment attempts
                var updateAttempts = Builders<OtpVerification>.Update.Inc(x => x.Attempts, 1);
                await _otpCollection.UpdateOneAsync(x => x.Id == otpVerification.Id, updateAttempts);

                // Check if OTP is valid
                if (!otpVerification.IsValid)
                {
                    _logger.LogWarning($"Invalid OTP attempt for {email}: expired or max attempts reached");
                    return false;
                }

                // Check if OTP code matches
                if (otpVerification.OtpCode != otpCode)
                {
                    _logger.LogWarning($"Incorrect OTP code for {email}");
                    return false;
                }

                // Mark OTP as used
                var updateUsed = Builders<OtpVerification>.Update.Set(x => x.IsUsed, true);
                await _otpCollection.UpdateOneAsync(x => x.Id == otpVerification.Id, updateUsed);

                _logger.LogInformation($"OTP verified successfully for {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying OTP for {email}");
                return false;
            }
        }

        public async Task<string?> GetTempUserDataAsync(string email, string purpose)
        {
            try
            {
                var filter = Builders<OtpVerification>.Filter.And(
                    Builders<OtpVerification>.Filter.Eq(x => x.Email, email),
                    Builders<OtpVerification>.Filter.Eq(x => x.Purpose, purpose),
                    Builders<OtpVerification>.Filter.Eq(x => x.IsUsed, true)
                );

                var otpVerification = await _otpCollection.Find(filter)
                    .SortByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                return otpVerification?.TempUserData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting temp user data for {email}");
                return null;
            }
        }

        public async Task<bool> ResendOtpAsync(string email, string purpose)
        {
            try
            {
                // Check if user can request another OTP (rate limiting)
                var recentOtp = await _otpCollection.Find(x => 
                    x.Email == email && 
                    x.Purpose == purpose && 
                    x.CreatedAt > DateTime.UtcNow.AddMinutes(-2))
                    .FirstOrDefaultAsync();

                if (recentOtp != null)
                {
                    _logger.LogWarning($"Rate limit exceeded for OTP resend: {email}");
                    return false;
                }

                // Get the temp user data from the most recent OTP
                var lastOtp = await _otpCollection.Find(x => x.Email == email && x.Purpose == purpose)
                    .SortByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                var tempUserData = lastOtp?.TempUserData;

                // Generate new OTP
                await GenerateOtpAsync(email, purpose, tempUserData);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resending OTP for {email}");
                return false;
            }
        }

        public async Task CleanupExpiredOtpsAsync()
        {
            try
            {
                var filter = Builders<OtpVerification>.Filter.Lt(x => x.ExpiresAt, DateTime.UtcNow);
                var result = await _otpCollection.DeleteManyAsync(filter);
                _logger.LogInformation($"Cleaned up {result.DeletedCount} expired OTPs");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired OTPs");
            }
        }

        private string GenerateRandomOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0);
            return (value % 1000000).ToString("D6");
        }

        private async Task SendOtpEmail(string email, string otpCode, string purpose)
        {
            var subject = purpose switch
            {
                "SIGNUP" => "Complete Your Account Registration - OTP Verification",
                "PASSWORD_RESET" => "Password Reset Verification Code",
                _ => "Verification Code"
            };

            var bodyTemplate = purpose switch
            {
                "SIGNUP" => GetSignupOtpEmailTemplate(otpCode),
                "PASSWORD_RESET" => GetPasswordResetOtpEmailTemplate(otpCode),
                _ => GetGenericOtpEmailTemplate(otpCode)
            };

            await _emailService.SendEmailAsync(email, subject, bodyTemplate, true);
        }

        private string GetSignupOtpEmailTemplate(string otpCode)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #7ECBD1 0%, #69b1b6 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .otp-code {{ background: #7ECBD1; color: white; font-size: 32px; font-weight: bold; padding: 15px; text-align: center; border-radius: 8px; margin: 20px 0; letter-spacing: 5px; }}
                        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 8px; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔐 Account Verification</h1>
                        </div>
                        <div class='content'>
                            <h2>Welcome! Complete Your Registration</h2>
                            <p>Thank you for signing up! To complete your account registration, please use the verification code below:</p>
                            
                            <div class='otp-code'>{otpCode}</div>
                            
                            <div class='warning'>
                                <strong>⚠️ Important:</strong>
                                <ul>
                                    <li>This code expires in <strong>10 minutes</strong></li>
                                    <li>Do not share this code with anyone</li>
                                    <li>If you didn't request this, please ignore this email</li>
                                </ul>
                            </div>
                            
                            <p>Enter this code in the verification form to activate your account and start using our platform.</p>
                            
                            <p>If you have any questions, please contact our support team.</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message. Please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetPasswordResetOtpEmailTemplate(string otpCode)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .otp-code {{ background: #e74c3c; color: white; font-size: 32px; font-weight: bold; padding: 15px; text-align: center; border-radius: 8px; margin: 20px 0; letter-spacing: 5px; }}
                        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 8px; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔑 Password Reset</h1>
                        </div>
                        <div class='content'>
                            <h2>Password Reset Verification</h2>
                            <p>We received a request to reset your password. Use the verification code below to proceed:</p>
                            
                            <div class='otp-code'>{otpCode}</div>
                            
                            <div class='warning'>
                                <strong>⚠️ Security Notice:</strong>
                                <ul>
                                    <li>This code expires in <strong>10 minutes</strong></li>
                                    <li>If you didn't request this reset, please ignore this email</li>
                                    <li>Your password remains unchanged until you complete the reset process</li>
                                </ul>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message. Please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GetGenericOtpEmailTemplate(string otpCode)
        {
            return $@"
                <h2>Verification Code</h2>
                <p>Your verification code is: <strong>{otpCode}</strong></p>
                <p>This code expires in 10 minutes.</p>
                <p>If you didn't request this, please contact our support team: allybees23@gmail.com</p>";
        }
    }
}
