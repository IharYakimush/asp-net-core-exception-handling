namespace Community.AspNetCore.ExceptionHandling
{
    public enum ResponseAlreadyStartedBehaviour
    {
        /// <summary>
        /// Re Throw exception if response already strated
        /// </summary>
        ReThrow = 0,


        /// <summary>
        /// Skip current handler and go to next handler in the chain
        /// </summary>
        GoToNextHandler = 1
    }
}