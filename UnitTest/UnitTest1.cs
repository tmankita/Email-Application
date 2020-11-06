using System;
using System.Collections.Generic;
using Email_app;
using Xunit;

namespace UnitTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            //Insert your email addresses (destination addresses).
            List<string> emails = new List<string> {};
            if (emails.Count == 0)
                return;
            ApplicationManager app = new ApplicationManager();
            //The second parameter is the count of jokes each email address gets.
            app.begin_test(emails,30);

        }
    }
}
