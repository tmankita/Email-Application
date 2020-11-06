using System;
using System.Collections.Generic;
using System.Threading;

namespace Email_app
{
    public interface IDadJokeGenerator
    {
        public void SeedRandomJoke(ref List<ManualResetEvent> events, int seed_count);
        public DadJoke GetRandomJoke();
    }
}
