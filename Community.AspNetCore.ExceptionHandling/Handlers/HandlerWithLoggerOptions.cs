namespace Community.AspNetCore.ExceptionHandling.Handlers
{
    public class HandlerWithLoggerOptions 
    {
        private string _loggerCategory = null;

        public string LoggerCategory
        {
            get => _loggerCategory ?? Const.Category;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this._loggerCategory = value;
                }
            }
        }
    }
}