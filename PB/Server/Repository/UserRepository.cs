using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using PB.DatabaseFramework;
using PB.Model;
using PB.Shared.Helpers;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.Text;
using PB.Shared.Tables;
using PB.EntityFramework;

namespace PB.Server.Repository
{
    public interface IUserRepository
    {
        public Task<int> SaveUser(UserCustom user, bool needEmailConfirmation, IDbTransaction tran = null, string brandName = "", string emailAddress = "");
    }
    public class UserRepository : IUserRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IEmailSender _email;
        public UserRepository(IDbContext dbContext, IEmailSender email)
        {
            this._dbContext = dbContext;
            this._email = email;
        }

        public async Task<int> SaveUser(UserCustom user, bool needEmailConfirmation, IDbTransaction tran = null, string brandName = "", string emailAddress = "")
        {
            var u = await _dbContext.GetAsync<UserCustom>($@"(UserName='{user.UserName}') and UserID<>{Convert.ToInt32(user.UserID)}", null, tran);
            if (u != null)
            {
                if (u.UserName == user.UserName)
                    throw new PBException("UserNameExist");
            }

            if (user.UserID == 0 && string.IsNullOrEmpty(user.Password))
            {
                user.Password = Guid.NewGuid().ToString("n").Substring(0, 8);
            }

            if (user.UserID == 0 && needEmailConfirmation)
            {
                int _min = 1000;
                int _max = 9999;
                Random _rdm = new();
                user.OTP = (_rdm.Next(_min, _max)).ToString();
                user.OTPGeneratedAt= DateTime.UtcNow;
            }

            if (!string.IsNullOrEmpty(user.Password))
            {
                user.PasswordHash = Guid.NewGuid().ToString("n").Substring(0, 8);
                user.Password = GetHashPassword(user.Password, user.PasswordHash);
            }

            int userId = await _dbContext.SaveAsync(user, tran);

            if (user.UserID == 0 && needEmailConfirmation && !user.EmailConfirmed)
            {
                var message = $@"
                       <div style = ""font-family: Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2;direction:rtl;"">
                            <div style = ""margin:50px auto;width:70%;padding:20px 0"">
                                <div style = ""border-bottom:1px solid #eee"" >
                                    <a href = """" style = ""font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600""> {PDV.BrandName} </a>
                                </div>
                                <p style = ""font-size:1.1em"">    Hi, Thank you for choosing {PDV.BrandName}.</p>
                                <p>Use the following OTP to complete your Sign Up procedures. </p> </p>
                              
                                <h2 style = ""background: #00466a;text-align:right;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;""> {user.OTP} </h2> 
                               <hr style = ""border: none; border - top:1px solid #eee"" />
                            <div style=""float:right; padding: 8px 0; color:#aaa;font-size:0.8em;line-height:1;font-weight:300"">
                                <p> Regards,</p>
                                    < p>  {PDV.BrandName} team</p>
                                </div>
                            </div>
                        </div>";

                if (string.IsNullOrEmpty(emailAddress))
                    emailAddress = user.UserName;
                //  await _email.SendHtmlEmailAsync(emailAddress, "Verify your e-mail address of your account", message, tran);
                await SendHtmlEmailAsync(emailAddress, "Verify your e-mail address of your account", message, tran);
            }
            return userId;
        }
        public static string GetHashPassword(string password, string salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.ASCII.GetBytes(salt),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
            return hashed;
        }
        private async Task<BaseErrorResponse> SendHtmlEmailAsync(string email, string subject, string message, IDbTransaction tran = null)
        {
            var body = $@"<!DOCTYPE html>
                            <html>  
                                <head>
                                    <title>{subject}</title>
                                    <link href='https://fonts.googleapis.com/css?family=Open+Sans:300,400,600,700,800&display=swap' rel='stylesheet'>
                                </head>
                                 <body style='padding: 0;margin: 0; font-family: 'Open Sans', sans-serif;'>
                                    {message}
                                 </body>
                            </html>";
            return await Send(email, subject, body, tran);
        }
        private async Task<BaseErrorResponse> Send(string email, string subject, string message, IDbTransaction tran = null)
        {
            BaseErrorResponse response = new BaseErrorResponse();
            try
            {
                MailSettings mailSettings = await _dbContext.GetAsync<MailSettings>(1, tran);
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(mailSettings.EmailAddress, mailSettings.Name)
                };
                mailMessage.To.Add(new MailAddress(email));
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = MailPriority.High;
                //AlternateView item = ContentToAlternateView(message);
                //mailMessage.AlternateViews.Add(item);
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    smtpClient.Host = mailSettings.SMTPHost;
                    smtpClient.Port = mailSettings.Port;
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(mailSettings.EmailAddress, mailSettings.Password);
                    smtpClient.Send(mailMessage);
                }

                return response;
            }
            catch
            {
                return response;
            }
        }


    }
}

