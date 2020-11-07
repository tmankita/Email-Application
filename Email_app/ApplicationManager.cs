using System;
using System.Collections.Concurrent;
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
        private ManualResetEvent main_event;
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
        private Startup service;
        private DadJokeGenerator jokesFactory;
        private MailgunEmailSender sender;
        private ConcurrentQueue<string> input_emails;



        public ApplicationManager(ManualResetEvent _main_event = null)
        {
            main_event = _main_event;
            clients = new HashSet<string>();
            current_waiting_count = 0;
            current_addresses = 0;
            jokes_count = 0;
            sends_count = 0;
            events = new List<ManualResetEvent>();
            events_to_delete = new List<ManualResetEvent>();
            updated = false;
            waiting_queue = new Queue<string>();
            service = new Startup();
            jokesFactory = new DadJokeGenerator(service.GetDadJokeClientObject());
            sender = new MailgunEmailSender(service.GetMailClientObject(), service.GetMailConfigSection());
            input_emails = new ConcurrentQueue<string>();
        }

        public void insert_email(string email)
        {
            input_emails.Enqueue(email);
        }
        public string GetError()
        {
            return sender.GetError();
        }

        public void begin() {

            string email;
            while (true)
            {     
                while (!input_emails.TryDequeue(out email)) {
                    //No Emails in the queue, Go to sleep for 100 milliseconds.
                    //When the thread is wakes up it will send jokes to all the emails in the queue.
                    Thread.Sleep(100);
                }
                if (!handle_email(email)) { break; }

            }
            WaitAll();
            main_event.Set();
        }

        private int serve(string email)
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
                    return 0;
                }
                MailInfo mail_info = new MailInfo { To = email, Subject = "Your Daddy joke, have a fun!", Body = joke.Joke };
                sender.SendMail(mail_info, ref events);
                return 1;
            }
            catch (FormatException)
            {
                sender.InsertError(email + " isn't a valid Email address.");
                return 0;
            }

        }

        private bool handle_email(string _email)
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
                    serve(Email);
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

            Email = _email;

            if (Email == "exit")
            {
                return false;
            }

            sends_count += serve(Email);
            return true;
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
            for (int i = 0; i < n; i++)
            {
                foreach (string email in Email_list) {
                    handle_email(email);
                }
            }
            WaitAll();
        }
    }
}
