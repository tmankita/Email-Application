using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Text.Json;
using System.Collections.Concurrent;


namespace Email_app
{
    public class DadJokeGenerator : IDadJokeGenerator
    {
        /// <summary>
        /// DadJokeGenerator is responsible for getting dad jokes from ``https://icanhazdadjoke.com/ ``API and get the joke for the client( Email address).
        /// Fields:
        /// - jokesWarehouse - is a Concurrent Queue that store all the jokes we get from ``https://icanhazdadjoke.com/ ``API.
        /// - diary -  is a mapping between the email address and a list of jokes. His role is to track the jokes each email received.
        /// </summary>
        private ConcurrentQueue<string> jokesWarehouse;
        private Dictionary<string, List<string>> diary;
        private readonly HttpClient dadJokeClient;
        private readonly string RandomJokeUrl = "/";


        public DadJokeGenerator(HttpClient dadJokeClient)
        {
            this.dadJokeClient = dadJokeClient;
            this.jokesWarehouse = new ConcurrentQueue<string>();
            this.diary = new Dictionary<string, List<string>>();

        }

        public void SeedRandomJoke(ref List<ManualResetEvent> events, int seed_count)
        {
            for (int i = 0; i < seed_count; i++)
            {
                var resetEvent = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(Task), resetEvent);
                events.Add(resetEvent);
            }
        }

        public DadJoke GetRandomJoke()
        {
            string joke;
            jokesWarehouse.TryDequeue(out joke);
            DadJoke dady_joke = JsonSerializer.Deserialize<DadJoke>(joke, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            jokesWarehouse.Enqueue(joke);
            return dady_joke;
        }

        public DadJoke GetRandomJokeFor(string email)
        {
            if (diary.ContainsKey(email))
            {
                List<string> l = diary[email];
                long n = jokesWarehouse.Count;
                long i = 0;
                while (i<n)
                {
                    DadJoke joke = GetRandomJoke();
                    if (!l.Contains(joke.Id))
                    {
                        diary[email].Add(joke.Id);
                        return joke;
                    }
                    i += 1;
                }
            }else
            {
                List<string> l = new List<string>();
                DadJoke joke = GetRandomJoke();
                l.Add(joke.Id);
                diary.Add(email, l);
                return joke;
            }
            return null;
        }

        public async void Task(object state) {
            var response = await dadJokeClient.GetStringAsync(RandomJokeUrl).ConfigureAwait(false);
            jokesWarehouse.Enqueue(response);
            ((ManualResetEvent)state).Set();
        }


    }
}
