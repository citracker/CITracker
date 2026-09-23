using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using System.Net;

namespace CITracker.Helpers
{
    public class Mailer
    {
        ILogger<Mailer> _log;
        IOptions<KeyValues> _config;
        IOptions<ADKeyValues> _adconfig;
        Shared.DTO.ResponseHandler<EmailDTO> _resp;
        IPathProvider _path;

        public Mailer(IOptions<KeyValues> con, IOptions<ADKeyValues> dcon, IPathProvider pat, ILogger<Mailer> log)
        {
            _config = con;
            _path = pat;
            _log = log;
            _adconfig = dcon;
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

        public string PopulatePaymentFailedBody(string name, decimal amount, int attempt, string hostedUrl)
        {
            string str = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(_path.MapPath("Templates/paymentfailed.html")))
                {
                    str = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Error Occurred at {nameof(PopulatePaymentFailedBody)} - {JsonConvert.SerializeObject(e.StackTrace)}");
            }
            return str.Replace("{{imgbase}}", _config.Value.ImageBaseUrl).Replace("{{firstname}}", name).Replace("{{amount}}", amount.ToString("C",
    System.Globalization.CultureInfo.GetCultureInfo("en-US"))).Replace("{{attempt}}", attempt.ToString()).Replace("{{url}}", hostedUrl).Replace("{{year}}", DateTime.UtcNow.Year.ToString());
        }

        public string PopulateTrialEndingBody(string name, DateTime date, string plan, decimal amount)
        {
            string str = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(_path.MapPath("Templates/trialending.html")))
                {
                    str = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Error Occurred at {nameof(PopulateTrialEndingBody)} - {JsonConvert.SerializeObject(e.StackTrace)}");
            }
            return str.Replace("{{imgbase}}", _config.Value.ImageBaseUrl).Replace("{{firstname}}", name).Replace("{{date}}", date.ToString("dddd d MMMM yyyy")).Replace("{{amount}}", amount.ToString("C",
    System.Globalization.CultureInfo.GetCultureInfo("en-US"))).Replace("{{plan}}", plan).Replace("{{year}}", DateTime.UtcNow.Year.ToString());
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

        public string PopulateProjectUpdateBody(ProjectUpdateNotificationDTO pid)
        {
            string str = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(_path.MapPath("Templates/projectupdate.html")))
                {
                    str = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Error Occurred at {nameof(PopulateProjectUpdateBody)} - {JsonConvert.SerializeObject(e.StackTrace)}");
            }
            return str.Replace("{{imgbase}}", _config.Value.ImageBaseUrl).Replace("{{projectname}}", pid.ProjectName).Replace("{{update}}", pid.Update).Replace("{{updateagent}}", pid.UpdateAgent).Replace("{{year}}", DateTime.UtcNow.Year.ToString());
        }


        public async Task<Shared.DTO.ResponseHandler<EmailDTO>> sendEmail(string recepientEmail, string subject, string displayName, string body, List<ReplyTo> replies = null, bool replyto = false)
        {
            try
            {
                var credential = new ClientSecretCredential(_adconfig.Value.MailerTenantID, _adconfig.Value.MailerClientID, _adconfig.Value.MailerClientSecret);
                var graphClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });

                var message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody { ContentType = BodyType.Html, Content = body },
                    ToRecipients = new List<Recipient>
                    {
                        new Recipient { EmailAddress = new EmailAddress { Address = recepientEmail } }
                    },
                    BccRecipients = new List<Recipient>
                    {
                        new Recipient { EmailAddress = new EmailAddress { Address = _config.Value.Bcc } }
                    }
                };

                await graphClient.Users[_config.Value.AutoSender].SendMail.PostAsync(new SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                });
            }
            catch (Exception e)
            {
                _resp = new Shared.DTO.ResponseHandler<EmailDTO>
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


        public async Task<Shared.DTO.ResponseHandler<EmailDTO>> sendEmail(List<string> recepientEmail, string subject, string displayName, string body, List<ReplyTo> replies = null, bool replyto = false)
        {
            try
            {
                var credential = new ClientSecretCredential(_adconfig.Value.MailerTenantID, _adconfig.Value.MailerClientID, _adconfig.Value.MailerClientSecret);
                var graphClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });

                var reci = new List<Recipient>();
                foreach (var i in recepientEmail)
                {
                    reci.Add(new Recipient { EmailAddress = new EmailAddress { Address = i } });
                }

                var message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody { ContentType = BodyType.Html, Content = body },
                    ToRecipients = reci,
                    BccRecipients = new List<Recipient>
                    {
                        new Recipient { EmailAddress = new EmailAddress { Address = _config.Value.Bcc } }
                    }
                };

                await graphClient.Users[_config.Value.AutoSender].SendMail.PostAsync(new SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                });
            }
            catch (Exception e)
            {
                _resp = new Shared.DTO.ResponseHandler<EmailDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = e.Message,
                    Error = e,
                    SingleResult = new EmailDTO
                    {
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
