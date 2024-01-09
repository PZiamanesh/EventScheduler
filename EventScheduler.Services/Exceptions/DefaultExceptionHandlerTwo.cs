using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EventScheduler.Services.Exceptions
{
    public class DefaultExceptionHandlerTwo : Exception, IAppExceptionHandler
    {
        public HttpStatusCode? StatusCode { get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(new
            {
                StatusCode = StatusCode,
                ErrorCode = ErrorCode,
                Message = Message
            });
        }
    }
}
