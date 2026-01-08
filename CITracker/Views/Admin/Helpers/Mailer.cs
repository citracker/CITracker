using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;

namespace CITracker.Helpers
{
    public class Mailer
    {
        ILogger<Mailer> _log;
        IOptions<KeyValues> _config;
        ResponseHandler<EmailDTO> _resp;
        IPathProvider _path;

        public Mailer(IOptions<KeyValues> con, IPathProvider pat, ILogger<Mailer> log)
        {
            _config = con;
            _path = pat;
            _log = log;
        }

        public string PopulateOTPBody(string name, string c1, string c2, string c3, string c4, string c5, string c6)
        {
            string str = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(_path.MapPath("Templates/otp.html")))
                {
                    str = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Error Occurred at {nameof(PopulateOTPBody)} - {JsonConvert.SerializeObject(e.StackTrace)}");
            }
            return str.Replace("{{imgbase}}", _config.Value.ImageBaseUrl).Replace("{{firstname}}", name).Replace("{{c1}}", c1).Replace("{{c2}}", c2).Replace("{{c3}}", c3).Replace("{{c4}}", c4).Replace("{{c5}}", c5).Replace("{{c6}}", c6).Replace("{{year}}", DateTime.UtcNow.Year.ToString());
        }

        public string PopulateRegistrationBody(string name)
        {
            string str = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(_path.MapPath("Templates/registration.html")))
                {
                    str = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Error Occurred at {nameof(PopulateRegistrationBody)} - {JsonConvert.SerializeObject(e.StackTrace)}");
            }
            return str.Replace("{{imgbase}}", _config.Value.ImageBaseUrl).Replace("{{firstname}}", name).Replace("{{year}}", DateTime.UtcNow.Year.ToString());
        }

        public string PopulateContactReceiptBody(EmailDTO email)
        {
            string str = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(_path.MapPath("Templates/contactreceipt.html")))
                {
                    str = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Error Occurred at {nameof(PopulateContactReceiptBody)} - {JsonConvert.SerializeObject(e.StackTrace)}");
            }
            return str.Replace("{{imgbase}}", _config.Value.ImageBaseUrl).Replace("{{firstname}}", email.Name).Replace("{{subject}}", email.Subject).Replace("{{year}}", DateTime.UtcNow.Year.ToString());
        }

        public string PopulateContactBody(EmailDTO email)
        {
            string str = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(_path.MapPath("Templates/contact.html")))
                {
                    str = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Error Occurred at {nameof(PopulateContactBody)} - {JsonConvert.SerializeObject(e.StackTrace)}");
            }
            return str.Replace("{{imgbase}}", _config.Value.ImageBaseUrl).Replace("{{name}}", email.Name).Replace("{{email}}", email.Email).Replace("{{subject}}", email.Subject).Replace("{{message}}", email.Message).Replace("{{year}}", DateTime.UtcNow.Year.ToString());
        }


        public ResponseHandler<EmailDTO> sendEmail(string recepientEmail, string subject, string displayName, string body, List<ReplyTo> replies = null, bool replyto = false)
        {
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    string address = _config.Value.Username;
                    message.From = new MailAddress(address, displayName);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    message.To.Add(recepientEmail);
                    if (replyto)
                    {
                        message.ReplyToList.Add(new MailAddress(replies.ElementAt(0).EmailAddress, replies.ElementAt(0).Name));
                    }
                    SmtpClient client1 = new SmtpClient
                    {
                        Host = _config.Value.Host,
                        EnableSsl = Convert.ToBoolean(_config.Value.EnableSsl)
                    };
                    client1.UseDefaultCredentials = false;
                    //client1.Timeout = 10000;
                    NetworkCredential credential = new NetworkCredential
                    {
                        UserName = _config.Value.Username,
                        Password = _config.Value.Password
                    };
                    client1.Credentials = credential;
                    client1.Port = _config.Value.Port;
                    client1.Send(message);
                    client1.Dispose();

                    _resp = new ResponseHandler<EmailDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Email sent successfully. Kindly verify your email",
                        SingleResult = new EmailDTO
                        {
                            Email = recepientEmail,
                            Subject = subject,
                            Name = displayName
                        }
                    };
                    _log.LogInformation($"Response{nameof(sendEmail)} - {JsonConvert.SerializeObject(_resp)}");
                }
            }
            catch (Exception e)
            {
                _resp = new ResponseHandler<EmailDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = e.Message,
                    Error = e,
                    SingleResult = new EmailDTO
                    {
                        Email = recepientEmail,
                        Subject = subject,
                        Name = displayName
                    }
                };

                _log.LogError($"Error Occurred at {nameof(sendEmail)} - {JsonConvert.SerializeObject(_resp)}");
            }
            return _resp;
        }
    }
}
