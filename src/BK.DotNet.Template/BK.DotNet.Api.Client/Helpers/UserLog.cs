using System;
using System.Collections.Generic;
using System.Text;

namespace BK.DotNet.Api.Client
{
    public class UserLog
    {
        public UserLog(string userID, string userEmail, string requestID, string traceIdentifier)
        {
            UserID = userID;
            UserEmail = userEmail;
            RequestID = requestID;
            TraceIdentifier = traceIdentifier;
        }

        public UserLog(string userID, string userEmail)
            : this(userID, userEmail, Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        {

        }

        public string UserID { get; set; }
        public string UserEmail { get; set; }
        public string RequestID { get; set; }
        public string TraceIdentifier { get; set; }
    }
}
