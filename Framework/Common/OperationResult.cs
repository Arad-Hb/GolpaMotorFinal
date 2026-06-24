using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common
{
    
    public class OperationResult(string? operationName)
    {
       
        public long? RecordID { get; set; }
        public string? OperationName { get; private set; } = operationName;
        public DateTime OperationDate { get; private set; } = DateTime.Now;
        public bool Success { get; private set; } = false;
        public string Message { get; private set; }

        public OperationResult ToSuccess(string message)
        {
            this.Success = true;
            this.Message = message;
            return this;
        }
        public OperationResult ToSuccess(string message,long recordId)
        {
            this.RecordID = recordId;
            this.Success = true;
            this.Message = message;
            return this;
        }
        public OperationResult ToFailed(string Message)
        {
            this.Success = false;
            this.Message = Message;
            return this;
        }
        public OperationResult ToFailed(string Message, long recordId)
        {
            this.RecordID = recordId;
            this.Success = false;
            this.Message = Message;
            return this;
        }
    }
}
