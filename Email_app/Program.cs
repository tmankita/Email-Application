using System;
using System.Collections.Generic;
using System.Threading;

namespace Email_app
{
    class Program
    {
        
        static void Main(string[] args)
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            ApplicationManager app = new ApplicationManager(resetEvent);
            Thread thr = new Thread(new ThreadStart(app.begin));

            thr.Start();
            string input;
            string error;
            while (true) {
                Console.WriteLine("Enter an Email (for quit type: exit):");
                input = Console.ReadLine().Trim();
                app.insert_email(input);
                if (input == "exit") { break; }
                Console.WriteLine("Your joke is on the way!");
                error = app.GetError();
                while (error != "")
                {
                    Console.WriteLine(error);
                    error = app.GetError();
                } 

            }
            resetEvent.WaitOne();
        }
    }
}
