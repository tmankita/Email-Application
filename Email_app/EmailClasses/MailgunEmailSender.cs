using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Email_app
{
    public class MailgunEmailSender : IEmailSender
    {
        /// <summary>
        /// MailgunEmailSender responsible to send emails.
        /// </summary>
        private readonly HttpClient mailgunHttpClient;
        private readonly MailConfigSection mailConfigSection;

        public MailgunEmailSender(HttpClient mailgunHttpClient,
            MailConfigSection mailConfigSection)
        {
            this.mailgunHttpClient = mailgunHttpClient;
            this.mailConfigSection = mailConfigSection;
        }

        public  void SendMail(MailInfo mailInfo, ref List<ManualResetEvent> events)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(mailConfigSection.From), "from");
            content.Add(new StringContent(mailInfo.To), "to");
            content.Add(new StringContent(mailInfo.Subject), "subject");
            content.Add(new StringContent(mailInfo.Body), "text");

            var resetEvent = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(async o =>
            {
                var response =  await mailgunHttpClient.PostAsync($"v3/{mailConfigSection.Domain}/messages", content);
                resetEvent.Set();
            });
            events.Add(resetEvent);

        }
    }
}
