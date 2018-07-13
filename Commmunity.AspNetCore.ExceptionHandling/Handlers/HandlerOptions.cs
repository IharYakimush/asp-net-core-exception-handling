namespace Commmunity.AspNetCore.ExceptionHandling.Handlers
{
    public class HandlerOptions
    {
        public RequestStartedBehaviour RequestStartedBehaviour { get; set; } = RequestStartedBehaviour.ReThrow;
    }
}