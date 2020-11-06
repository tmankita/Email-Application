using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Email_app
{
    public class Startup
    {
        /// <summary>
        /// Startup responsible for initaite and configure all the API connections. 
        /// </summary>

        private static readonly HttpClient mailClient = new HttpClient();
        private static readonly HttpClient dadJokeClient = new HttpClient();

        private readonly MailConfigSection mailConfigSection;
        private readonly JokeConfigSection jokeConfigSection;

        public Startup()
        {
            this.mailConfigSection = new MailConfigSection
            {
                Base = "https://api.eu.mailgun.net",
                From = "Console.App@viaMailgun.com",
                MailgunKey = "APi-key",
                Domain = "mg.brash.io"

            };
            this.jokeConfigSection = new JokeConfigSection
            {
                Base = "https://icanhazdadjoke.com/",
                UserAgent = "ConsoleApp (tmankita@gmail.com)",
                Accept = "application/json"
            };

            Configure();
        }
        public void Configure() {

            mailClient.BaseAddress = new Uri(mailConfigSection.Base);
            mailClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{mailConfigSection.MailgunKey}")));
            dadJokeClient.BaseAddress = new Uri(jokeConfigSection.Base);
            dadJokeClient.DefaultRequestHeaders.UserAgent.TryParseAdd(jokeConfigSection.UserAgent);
            dadJokeClient.DefaultRequestHeaders.Accept.ParseAdd(jokeConfigSection.Accept);
        }
        public HttpClient GetMailClientObject() {
            return mailClient;
        }
        public MailConfigSection GetMailConfigSection() {
            return mailConfigSection;
        }
        public HttpClient GetDadJokeClientObject()
        {
            return dadJokeClient;
        }
    }

}
