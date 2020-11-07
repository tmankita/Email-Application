using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Email_app;
using Xunit;
using Xunit.Abstractions;

namespace UnitTest
{


    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test1()
        {
            //Insert your email addresses (destination addresses).
            List<string> emails = new List<string> { };
            if (emails.Count == 0)
                return;

            ApplicationManager app = new ApplicationManager();
            //The second parameter is the count of jokes each email address gets.
            app.begin_test(emails, 30);
        }
        [Fact]
        public void Test2()
        {

            ManualResetEvent resetEvent = new ManualResetEvent(false);
            ApplicationManager app = new ApplicationManager(resetEvent);
            Thread thr = new Thread(new ThreadStart(app.begin));
            thr.Start();
            int n = 50;
            for (int i = 0; i < n; i++)
            {
                //Modify the line and insert your email
                //app.insert_email("insert-your-email");
            }
            app.insert_email("exit");
            resetEvent.WaitOne();
        }
    }
}
