using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Domain;

public enum LogCategory
{
    Log = 0,
    Result = 1,
    Exception = 2,
    HttpRequest = 3,
    HttpResponse = 4
}
