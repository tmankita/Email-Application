using System;
using System.Collections.Concurrent;
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
        private ConcurrentQueue<string> errors;

        public MailgunEmailSender(HttpClient mailgunHttpClient,
            MailConfigSection mailConfigSection)
        {
            this.mailgunHttpClient = mailgunHttpClient;
            this.mailConfigSection = mailConfigSection;
            this.errors = new ConcurrentQueue<string>();
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
                
                if (!response.IsSuccessStatusCode)
                {
                    InsertError("ERROR: Can't send joke to:\" "+ mailInfo.To + " \". The reason is: "+ response.ReasonPhrase +".");
                }
                resetEvent.Set();
            });
            events.Add(resetEvent);

        }

        public string GetError()
        {
            string error;
            if (errors.TryDequeue(out error))
            {
                return error;
            }
            return "";
             
        }

        public void InsertError(string error)
        {
            errors.Enqueue(error);
        }
    }
}
