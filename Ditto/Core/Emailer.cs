// -----------------------------------------------------------------------
// <copyright file="Emailer.cs" company="Dylan Addison">
//     Copyright (c) Dylan Addison. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Ditto.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Mail;

    using Ditto.Model;

    using log4net;
    using log4net.Appender;
    using log4net.Core;

    public class Emailer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Emailer));

        public string SmtpServer { get; set; }
        
        public string Sender { get; set; }

        public List<string> Recipients { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public Emailer(JobDetails jobDetails, string body)
        {
            this.SmtpServer = jobDetails.Smtp;
            this.Sender = jobDetails.Sender;
            this.Recipients = jobDetails.Recipients;
            this.Subject = jobDetails.Subject;
            this.Body = body;
        }

        public void Send()
        {
            if (!IsValid()) { return ; }

            var smtpClient = new SmtpClient(this.SmtpServer);
            var emailMessage = new MailMessage
            {
                From = new MailAddress(this.Sender),
                Subject = this.Subject,
                Body =  this.Body,
                IsBodyHtml = false     
            };

            var errorLog = this.GetLogPath();

            emailMessage.Attachments.Add(new Attachment(errorLog));

            foreach (var recipient in Recipients)
            {
                emailMessage.To.Add(recipient);
            }

            try
            {
                smtpClient.Send(emailMessage);
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }

        }

        private bool IsValid()
        {
            var isValid = true;

            if (string.IsNullOrWhiteSpace(this.Sender) || this.Sender == "")
            {
                Log.Error("Sender is blank or white space");
                isValid = false;
            }
            else if (this.Recipients.Count < 1)
            {
                Log.Warn("Missing recipients, requires at least 1");
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(this.Subject) || this.Subject == "")
            {
                Log.Warn("Subject is blank or white space");
                isValid = false;
            }

            return isValid;
        }

        private string GetLogPath()
        {
            var path = string.Empty;
            var logRepository = LogManager.GetRepository();

            var appenders = logRepository.GetAppenders();

            foreach (var appender in appenders)
            {
                if (appender.GetType() == typeof(FileAppender) && appender.Name == "FileAppenderErrors")
                {
                    var errorAppender = (FileAppender) appender;
                    path = errorAppender.File;

                }
            }

            return path;
        }
    }
}