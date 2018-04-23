using Microsoft.Extensions.Logging;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public static class Events
    {
        public static readonly EventId HandlerError = new EventId(100, "HandlerExecutionError");
        public static readonly EventId PolicyNotFound = new EventId(101, "PolicyForExceptionNotRegistered");
        public static readonly EventId HandlersNotFound = new EventId(102, "HandlersCollectionEmpty");
        public static readonly EventId HandlerNotCreated = new EventId(103, "HandlersCanNotBeCreated");
    }
}