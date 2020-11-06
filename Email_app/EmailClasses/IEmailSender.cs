using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Email_app
{
    public interface IEmailSender
    {
        void SendMail(MailInfo mailInfo, ref List<ManualResetEvent> events);

    }
}
