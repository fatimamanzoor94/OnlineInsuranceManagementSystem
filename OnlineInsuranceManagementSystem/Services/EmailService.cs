using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OnlineInsuranceManagementSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink)
        {
            try
            {
                var smtpConfig = _config.GetSection("Email");
                using var smtpClient = new SmtpClient(smtpConfig["SmtpServer"])
                {
                    Port = int.Parse(smtpConfig["Port"]),
                    Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpConfig["FromEmail"], smtpConfig["FromName"]),
                    Subject = "Password Reset Request - OIMS",
                    IsBodyHtml = true,
                    Body = GeneratePasswordResetEmailBody(fullName, resetLink),
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Password reset email sent to {Email}", toEmail);
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", toEmail);
                return false;
            }
        }

        // Send Agent Account Created Email
        public async Task<bool> SendAgentAccountCreatedEmailAsync(string toEmail, string fullName, string password)
        {
            try
            {
                var smtpConfig = _config.GetSection("Email");
                using var smtpClient = new SmtpClient(smtpConfig["SmtpServer"])
                {
                    Port = int.Parse(smtpConfig["Port"]),
                    Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpConfig["FromEmail"], smtpConfig["FromName"]),
                    Subject = "Welcome to OIMS - Your Agent Account Has Been Created",
                    IsBodyHtml = true,
                    Body = GenerateAgentCreatedEmailBody(fullName, password),
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Agent creation email sent to {Email}", toEmail);
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error sending agent creation email to {Email}", toEmail);
                return false;
            }
        }

        // Send Agent Approval Email - PROFESSIONAL DESIGN
        public async Task<bool> SendAgentApprovalEmailAsync(string toEmail, string fullName)
        {
            try
            {
                var smtpConfig = _config.GetSection("Email");
                using var smtpClient = new SmtpClient(smtpConfig["SmtpServer"])
                {
                    Port = int.Parse(smtpConfig["Port"]),
                    Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpConfig["FromEmail"], smtpConfig["FromName"]),
                    Subject = "Your Agent Account Has Been Approved - OIMS",
                    IsBodyHtml = true,
                    Body = GenerateAgentApprovalEmailBody(fullName),
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Agent approval email sent to {Email}", toEmail);
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error sending agent approval email to {Email}", toEmail);
                return false;
            }
        }

        public async Task SendAgentAssignmentNotificationAsync(string agentEmail, int applicationId, string subject)
        {
            try
            {
                // TODO: Implement actual email sending logic
                // Example using your existing email logic:
                var message = $"A new application (ID: {applicationId}) has been assigned to you for review.";

                // Send email here (use your SMTP logic)
                await Task.CompletedTask; // Placeholder
            }
            catch
            {
                // Log error but don't throw
            }
        }

        private string GeneratePasswordResetEmailBody(string fullName, string resetLink)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""x-apple-disable-message-reformatting"">
    <title>Password Reset</title>
    <!--[if mso]>
    <noscript>
    <xml>
    <o:OfficeDocumentSettings>
    <o:PixelsPerInch>96</o:PixelsPerInch>
    </o:OfficeDocumentSettings>
    </xml>
    </noscript>
    <![endif]-->
    <style>
        body {{ margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8f9fa; }}
        table {{ border-spacing: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }}
        td {{ padding: 0; vertical-align: top; }}
        .button {{ display: inline-block; padding: 14px 32px; background-color: #08979D; color: #ffffff; text-decoration: none; border-radius: 4px; font-weight: 500; }}
        .footer-link {{ color: #6c757d; text-decoration: none; font-size: 12px; }}
        .footer-link:hover {{ text-decoration: underline; }}
        @media only screen and (max-width: 600px) {{
            .container {{ width: 100% !important; }}
            .content {{ padding: 20px !important; }}
            .button {{ width: 100%; text-align: center; box-sizing: border-box; }}
        }}
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #f8f9fa; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f8f9fa; padding: 40px 0;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" class=""container"" style=""background-color: #ffffff; border-radius: 8px; margin: 0 auto; box-shadow: 0 2px 8px rgba(0,0,0,0.08); overflow: hidden;"">
                    <tr>
                        <td style=""background-color: #08979D; padding: 24px 32px; text-align: left;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 20px; font-weight: 600;"">Online Insurance Management System</h1>
                        </td>
                    </tr>
                    <tr>
                        <td class=""content"" style=""padding: 32px;"">
                            <h2 style=""color: #212529; margin: 0 0 16px 0; font-size: 18px; font-weight: 600;"">Password Reset Request</h2>
                            <p style=""color: #495057; margin: 0 0 16px 0; font-size: 14px; line-height: 1.6;"">Hello {fullName},</p>
                            <p style=""color: #495057; margin: 0 0 24px 0; font-size: 14px; line-height: 1.6;"">We received a request to reset your password. Click the button below to create a new password:</p>
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 24px 0;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{resetLink}"" class=""button"" style=""display: inline-block; padding: 14px 32px; background-color: #08979D; color: #ffffff; text-decoration: none; border-radius: 4px; font-weight: 500; font-size: 14px;"">Reset Password</a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #6c757d; margin: 0 0 0 0; font-size: 13px; line-height: 1.6;"">This link will expire in 1 hour for security purposes.</p>
                            <p style=""color: #6c757d; margin: 16px 0 0 0; font-size: 13px; line-height: 1.6;"">If you did not request this password reset, please ignore this email or contact our support team.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 32px; text-align: center; border-top: 1px solid #e9ecef;"">
                            <p style=""color: #6c757d; margin: 0 0 8px 0; font-size: 12px;"">Online Insurance Management System</p>
                            <p style=""color: #adb5bd; margin: 0; font-size: 11px;"">&copy; 2026 OIMS. All rights reserved.</p>
                            <p style=""margin: 12px 0 0 0;"">
                                <a href=""https://yourdomain.com/privacy"" class=""footer-link"">Privacy Policy</a> | 
                                <a href=""https://yourdomain.com/terms"" class=""footer-link"">Terms of Service</a> | 
                                <a href=""https://yourdomain.com/support"" class=""footer-link"">Support</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string GenerateAgentCreatedEmailBody(string fullName, string password)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""x-apple-disable-message-reformatting"">
    <title>Welcome to OIMS</title>
    <!--[if mso]>
    <noscript>
    <xml>
    <o:OfficeDocumentSettings>
    <o:PixelsPerInch>96</o:PixelsPerInch>
    </o:OfficeDocumentSettings>
    </xml>
    </noscript>
    <![endif]-->
    <style>
        body {{ margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8f9fa; }}
        table {{ border-spacing: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }}
        td {{ padding: 0; vertical-align: top; }}
        .credentials-box {{ background-color: #f8f9fa; border-left: 4px solid #08979D; padding: 16px 20px; margin: 20px 0; border-radius: 0 4px 4px 0; }}
        .credential-label {{ color: #6c757d; font-size: 12px; margin: 0 0 4px 0; }}
        .credential-value {{ color: #212529; font-size: 14px; font-weight: 500; margin: 0; font-family: 'Courier New', monospace; }}
        .button {{ display: inline-block; padding: 14px 32px; background-color: #08979D; color: #ffffff; text-decoration: none; border-radius: 4px; font-weight: 500; }}
        .footer-link {{ color: #6c757d; text-decoration: none; font-size: 12px; }}
        .footer-link:hover {{ text-decoration: underline; }}
        @media only screen and (max-width: 600px) {{
            .container {{ width: 100% !important; }}
            .content {{ padding: 20px !important; }}
            .button {{ width: 100%; text-align: center; box-sizing: border-box; }}
        }}
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #f8f9fa; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f8f9fa; padding: 40px 0;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" class=""container"" style=""background-color: #ffffff; border-radius: 8px; margin: 0 auto; box-shadow: 0 2px 8px rgba(0,0,0,0.08); overflow: hidden;"">
                    <tr>
                        <td style=""background-color: #08979D; padding: 24px 32px; text-align: left;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 20px; font-weight: 600;"">Online Insurance Management System</h1>
                        </td>
                    </tr>
                    <tr>
                        <td class=""content"" style=""padding: 32px;"">
                            <h2 style=""color: #212529; margin: 0 0 16px 0; font-size: 18px; font-weight: 600;"">Welcome to OIMS</h2>
                            <p style=""color: #495057; margin: 0 0 16px 0; font-size: 14px; line-height: 1.6;"">Hello {fullName},</p>
                            <p style=""color: #495057; margin: 0 0 24px 0; font-size: 14px; line-height: 1.6;"">Your agent account has been successfully created by our administration team. You can now access your dashboard and begin managing insurance policies.</p>
                            
                            <div class=""credentials-box"">
                                <p class=""credential-label"">Email Address</p>
                                <p class=""credential-value"" style=""margin-bottom: 12px;"">{fullName.Split(' ')[0].ToLower() + "@domain.com"}</p>
                                <p class=""credential-label"">Temporary Password</p>
                                <p class=""credential-value"">{password}</p>
                            </div>
                            
                            <p style=""color: #6c757d; margin: 0 0 24px 0; font-size: 13px; line-height: 1.6;"">For security purposes, please change your password after your first login.</p>
                            
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 24px 0;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""https://yourdomain.com/account/login"" class=""button"" style=""display: inline-block; padding: 14px 32px; background-color: #08979D; color: #ffffff; text-decoration: none; border-radius: 4px; font-weight: 500; font-size: 14px;"">Access Your Dashboard</a>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style=""color: #6c757d; margin: 0 0 0 0; font-size: 13px; line-height: 1.6;"">If you have any questions or need assistance, please contact our support team.</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 32px; text-align: center; border-top: 1px solid #e9ecef;"">
                            <p style=""color: #6c757d; margin: 0 0 8px 0; font-size: 12px;"">Online Insurance Management System</p>
                            <p style=""color: #adb5bd; margin: 0; font-size: 11px;"">&copy; 2026 OIMS. All rights reserved.</p>
                            <p style=""margin: 12px 0 0 0;"">
                                <a href=""https://yourdomain.com/privacy"" class=""footer-link"">Privacy Policy</a> | 
                                <a href=""https://yourdomain.com/terms"" class=""footer-link"">Terms of Service</a> | 
                                <a href=""https://yourdomain.com/support"" class=""footer-link"">Support</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        private string GenerateAgentApprovalEmailBody(string fullName)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""x-apple-disable-message-reformatting"">
    <title>Account Approved</title>
    <!--[if mso]>
    <noscript>
    <xml>
    <o:OfficeDocumentSettings>
    <o:PixelsPerInch>96</o:PixelsPerInch>
    </o:OfficeDocumentSettings>
    </xml>
    </noscript>
    <![endif]-->
    <style>
        body {{ margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8f9fa; }}
        table {{ border-spacing: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }}
        td {{ padding: 0; vertical-align: top; }}
        .feature-list {{ margin: 20px 0; padding-left: 20px; }}
        .feature-list li {{ color: #495057; font-size: 14px; line-height: 1.8; margin: 0 0 8px 0; }}
        .button {{ display: inline-block; padding: 14px 32px; background-color: #08979D; color: #ffffff; text-decoration: none; border-radius: 4px; font-weight: 500; }}
        .footer-link {{ color: #6c757d; text-decoration: none; font-size: 12px; }}
        .footer-link:hover {{ text-decoration: underline; }}
        @media only screen and (max-width: 600px) {{
            .container {{ width: 100% !important; }}
            .content {{ padding: 20px !important; }}
            .button {{ width: 100%; text-align: center; box-sizing: border-box; }}
            .feature-list {{ padding-left: 16px; }}
        }}
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #f8f9fa; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f8f9fa; padding: 40px 0;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" class=""container"" style=""background-color: #ffffff; border-radius: 8px; margin: 0 auto; box-shadow: 0 2px 8px rgba(0,0,0,0.08); overflow: hidden;"">
                    <tr>
                        <td style=""background-color: #08979D; padding: 24px 32px; text-align: left;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 20px; font-weight: 600;"">Online Insurance Management System</h1>
                        </td>
                    </tr>
                    <tr>
                        <td class=""content"" style=""padding: 32px;"">
                            <h2 style=""color: #212529; margin: 0 0 16px 0; font-size: 18px; font-weight: 600;"">Account Approval Confirmation</h2>
                            <p style=""color: #495057; margin: 0 0 16px 0; font-size: 14px; line-height: 1.6;"">Dear {fullName},</p>
                            <p style=""color: #495057; margin: 0 0 24px 0; font-size: 14px; line-height: 1.6;"">We are pleased to inform you that your agent account has been reviewed and approved by our administration team. You now have full access to the OIMS agent portal.</p>
                            
                            <p style=""color: #212529; margin: 0 0 12px 0; font-size: 14px; font-weight: 500;"">With your approved account, you can:</p>
                            <ul class=""feature-list"">
                                <li>View and manage insurance policies</li>
                                <li>Submit applications on behalf of customers</li>
                                <li>Track claims and payment status</li>
                                <li>Access reports and analytics dashboard</li>
                                <li>Communicate with customers through the platform</li>
                            </ul>
                            
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin: 24px 0;"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""https://yourdomain.com/agent/dashboard"" class=""button"" style=""display: inline-block; padding: 14px 32px; background-color: #08979D; color: #ffffff; text-decoration: none; border-radius: 4px; font-weight: 500; font-size: 14px;"">Go to Agent Dashboard</a>
                                    </td>
                                </tr>
                            </table>
                            
                            <p style=""color: #6c757d; margin: 0 0 0 0; font-size: 13px; line-height: 1.6;"">If you require any assistance or have questions about using the platform, please do not hesitate to contact our support team.</p>
                            <p style=""color: #6c757d; margin: 16px 0 0 0; font-size: 13px; line-height: 1.6;"">We look forward to your continued partnership.</p>
                            <p style=""color: #212529; margin: 24px 0 0 0; font-size: 14px; font-weight: 500;"">Best regards,<br><span style=""font-weight: 400; color: #495057;"">OIMS Administration Team</span></p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 32px; text-align: center; border-top: 1px solid #e9ecef;"">
                            <p style=""color: #6c757d; margin: 0 0 8px 0; font-size: 12px;"">Online Insurance Management System</p>
                            <p style=""color: #adb5bd; margin: 0; font-size: 11px;"">&copy; 2026 OIMS. All rights reserved.</p>
                            <p style=""margin: 12px 0 0 0;"">
                                <a href=""https://yourdomain.com/privacy"" class=""footer-link"">Privacy Policy</a> | 
                                <a href=""https://yourdomain.com/terms"" class=""footer-link"">Terms of Service</a> | 
                                <a href=""https://yourdomain.com/support"" class=""footer-link"">Support</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }





        // Send Reply to Customer Email
        public async Task<bool> SendReplyToCustomerEmailAsync(string toEmail, string customerName, string subject, string message, string originalMessageId)
        {
            try
            {
                var smtpConfig = _config.GetSection("Email");
                using var smtpClient = new SmtpClient(smtpConfig["SmtpServer"])
                {
                    Port = int.Parse(smtpConfig["Port"]),
                    Credentials = new NetworkCredential(smtpConfig["Username"], smtpConfig["Password"]),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpConfig["FromEmail"], smtpConfig["FromName"]),
                    Subject = $"Re: {subject}",
                    IsBodyHtml = true,
                    Body = GenerateReplyToCustomerEmailBody(customerName, message, originalMessageId),
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Reply email sent to {Email}", toEmail);
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error sending reply email to {Email}", toEmail);
                return false;
            }
        }

        private string GenerateReplyToCustomerEmailBody(string customerName, string replyMessage, string originalMessageId)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""x-apple-disable-message-reformatting"">
    <title>Reply from OIMS Support</title>
    <!--[if mso]>
    <noscript>
    <xml>
    <o:OfficeDocumentSettings>
    <o:PixelsPerInch>96</o:PixelsPerInch>
    </o:OfficeDocumentSettings>
    </xml>
    </noscript>
    <![endif]-->
    <style>
        body {{ margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8f9fa; }}
        table {{ border-spacing: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }}
        td {{ padding: 0; vertical-align: top; }}
        .reply-box {{ background-color: #f8f9fa; border-left: 4px solid #08979D; padding: 16px 20px; margin: 20px 0; border-radius: 0 4px 4px 0; }}
        .original-message {{ background-color: #ffffff; border: 1px solid #e9ecef; padding: 16px; margin: 20px 0; border-radius: 4px; }}
        .footer-link {{ color: #6c757d; text-decoration: none; font-size: 12px; }}
        .footer-link:hover {{ text-decoration: underline; }}
        @media only screen and (max-width: 600px) {{
            .container {{ width: 100% !important; }}
            .content {{ padding: 20px !important; }}
        }}
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #f8f9fa; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #f8f9fa; padding: 40px 0;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" border=""0"" class=""container"" style=""background-color: #ffffff; border-radius: 8px; margin: 0 auto; box-shadow: 0 2px 8px rgba(0,0,0,0.08); overflow: hidden;"">
                    <tr>
                        <td style=""background-color: #08979D; padding: 24px 32px; text-align: left;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 20px; font-weight: 600;"">Online Insurance Management System</h1>
                        </td>
                    </tr>
                    <tr>
                        <td class=""content"" style=""padding: 32px;"">
                            <h2 style=""color: #212529; margin: 0 0 16px 0; font-size: 18px; font-weight: 600;"">Response to Your Inquiry</h2>
                            <p style=""color: #495057; margin: 0 0 16px 0; font-size: 14px; line-height: 1.6;"">Dear {customerName},</p>
                            <p style=""color: #495057; margin: 0 0 24px 0; font-size: 14px; line-height: 1.6;"">Thank you for contacting us. We have received your message and are pleased to provide you with the following response:</p>
                            
                            <div class=""reply-box"">
                                <p style=""color: #212529; margin: 0 0 8px 0; font-size: 13px; font-weight: 600;"">Our Response:</p>
                                <p style=""color: #495057; margin: 0; font-size: 14px; line-height: 1.6; white-space: pre-wrap;"">{replyMessage}</p>
                            </div>
                            
                            <div class=""original-message"">
                                <p style=""color: #6c757d; margin: 0 0 8px 0; font-size: 12px; font-weight: 600;"">Original Message (Reference: {originalMessageId})</p>
                                <p style=""color: #adb5bd; margin: 0; font-size: 12px; line-height: 1.6;"">You can view the full conversation in your OIMS dashboard.</p>
                            </div>
                            
                            <p style=""color: #6c757d; margin: 0 0 0 0; font-size: 13px; line-height: 1.6;"">If you have any further questions or need additional assistance, please don't hesitate to reply to this email or contact our support team.</p>
                            <p style=""color: #212529; margin: 24px 0 0 0; font-size: 14px; font-weight: 500;"">Best regards,<br><span style=""font-weight: 400; color: #495057;"">OIMS Support Team<br>support@oims.com</span></p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 20px 32px; text-align: center; border-top: 1px solid #e9ecef;"">
                            <p style=""color: #6c757d; margin: 0 0 8px 0; font-size: 12px;"">Online Insurance Management System</p>
                            <p style=""color: #adb5bd; margin: 0; font-size: 11px;"">&copy; 2026 OIMS. All rights reserved.</p>
                            <p style=""margin: 12px 0 0 0;"">
                                <a href=""https://yourdomain.com/privacy"" class=""footer-link"">Privacy Policy</a> | 
                                <a href=""https://yourdomain.com/terms"" class=""footer-link"">Terms of Service</a> | 
                                <a href=""https://yourdomain.com/support"" class=""footer-link"">Support</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
    }
}