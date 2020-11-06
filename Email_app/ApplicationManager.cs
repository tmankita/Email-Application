using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;

namespace Email_app
{
    public class ApplicationManager
    {
        /// <summary>
        /// ApplicationMangaer manage which email address gets the joke.
        /// When an email address receives all the jokes the joke factory has, it will wait until the factory produces more.
        /// Fields:
        /// - waiting_queue -  is a queue that stores all the email addresses that already got the jokes we have in the joke factory.
        /// - clients -  is a hash table that store all the  costumers (email addresses that asked for joke)
        /// </summary>
        private HashSet<string> clients;
        private Queue<string> waiting_queue;
        private List<ManualResetEvent> events;
        private List<ManualResetEvent> events_to_delete;
        private bool updated;
        private const int seed_count = 10;
        private const int max_waiting_emails = 10;
        private long current_addresses;
        private long current_waiting_count;
        private long jokes_count;
        private long sends_count;
        private string email_for_testing;

        public ApplicationManager()
        {
            clients = new HashSet<string>();
            current_waiting_count = 0;
            current_addresses = 0;
            jokes_count = 0;
            sends_count = 0;
            events = new List<ManualResetEvent>();
            events_to_delete = new List<ManualResetEvent>();
            updated = false;
            waiting_queue = new Queue<string>();
        }

        public void begin() {
            Startup service = new Startup();
            DadJokeGenerator jokesFactory = new DadJokeGenerator(service.GetDadJokeClientObject());
            MailgunEmailSender sender = new MailgunEmailSender(service.GetMailClientObject(), service.GetMailConfigSection());
            string option = "Console";
            while (mainLoop(ref service, ref jokesFactory, ref sender, ref option)) { }
            WaitAll();

        }

        private void serve(string email,ref DadJokeGenerator jokesFactory, ref MailgunEmailSender sender)
        {
            try
            {
                MailAddress m = new MailAddress(email);
                if (!clients.Contains(email))
                {
                    clients.Add(email);
                    current_addresses += 1;
                }
                DadJoke joke = jokesFactory.GetRandomJokeFor(email);
                if (joke == null)
                {
                    waiting_queue.Enqueue(email);
                    return;
                }
                MailInfo mail_info = new MailInfo { To = email, Subject = "Your Daddy joke, have a fun!", Body = joke.Joke };
                sender.SendMail(mail_info, ref events);
                Console.WriteLine("Your joke is on the way!");

            }
            catch (FormatException)
            {
                Console.WriteLine(email + " isn't a valid Email address.");
            }

        }
        public bool mainLoop(ref Startup service, ref DadJokeGenerator jokesFactory, ref MailgunEmailSender sender, ref string option)
        {
            string Email = "";
            //If ``updated = true`` then The joke factory produced more jokes.
            if (updated)
            {
                long pending_emails = waiting_queue.Count;
                //We will try to send jokes to the waiting Email addresses.
                while (pending_emails > 0)
                {
                    Email = waiting_queue.Dequeue();
                    serve(Email, ref jokesFactory, ref sender);
                    if (!waiting_queue.Contains(Email))
                    {
                        current_waiting_count -= 1;
                    }
                    pending_emails -= 1;
                }
                updated = false;
            }
   
            // If this case is true, we lack jokes, and as a result, we will ask for more jokes.
            if (current_waiting_count > max_waiting_emails ||
                (current_addresses <= max_waiting_emails && sends_count > jokes_count) ||
                jokes_count == 0)
            {
                jokesFactory.SeedRandomJoke(ref events, seed_count);
                jokes_count += sends_count;
                updated = true;
                WaitAll();
            }

            Email = get_email_address(option);

            if (Email == "exit")
            {
                return false;
            }
            serve(Email, ref jokesFactory, ref sender);
            sends_count += 1;
            return true;
        }

        private string get_email_address(string option)
        {
            switch (option)
            {
                case "Testing":
                    return email_for_testing;
                case "Console":
                    Console.WriteLine("Enter an Email (for quit type: exit):");
                    return Console.ReadLine().Trim();

            }
            return "";

        }

        private void WaitAll()
        {
            double events_count = events.Count;
            if (events_count < 64)
            {
                WaitHandle.WaitAll(events.ToArray());
                return;
            }
            double times = Math.Ceiling(events_count / 64);
            for (int i = 0; i < times; i++)
            {
                for (int j = 0; j < 64; j++)
                {
                    if (events.Count == 0)
                        break;
                    events_to_delete.Add(events[0]);
                    events.RemoveAt(0);
                }
                WaitHandle.WaitAll(events_to_delete.ToArray());
                events_to_delete.Clear();
            }



        }
        //======================================UnitTesting============================================================================

        public void begin_test(List<string> Email_list, int n)
        {
            Startup service = new Startup();
            DadJokeGenerator jokesFactory = new DadJokeGenerator(service.GetDadJokeClientObject());
            MailgunEmailSender sender = new MailgunEmailSender(service.GetMailClientObject(), service.GetMailConfigSection());
            string option = "Testing";
            for (int i = 0; i < n; i++)
            {
                foreach (string email in Email_list) {
                    this.email_for_testing = email;
                    mainLoop(ref service, ref jokesFactory, ref sender, ref option);
                }
            }
            WaitAll();
        }
    }
}
