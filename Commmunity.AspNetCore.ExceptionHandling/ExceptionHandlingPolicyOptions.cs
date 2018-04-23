using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public class ExceptionHandlingPolicyOptions : IOptions<ExceptionHandlingPolicyOptions>
    {
        public ExceptionHandlingPolicyOptions Value => this;

        private readonly OrderedDictionary handlers = new OrderedDictionary();

        public ExceptionHandlingPolicyOptions EnsureException(Type exceptionType, int index = -1)
        {
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new ArgumentOutOfRangeException(nameof(exceptionType), exceptionType,
                    $"Exception type should implement {typeof(Exception).Name}");
            }

            if (handlers.Contains(exceptionType))
            {
                if (index >= 0)
                {
                    object values = handlers[exceptionType];
                    handlers.Remove(exceptionType);
                    handlers.Insert(index, exceptionType, values);
                }               
            }
            else
            {
                if (index < 0)
                {
                    handlers.Add(exceptionType, new List<Type>());
                }
                else
                {
                    handlers.Insert(index, exceptionType, new List<Type>());
                }
            }

            return this;
        }

        public ExceptionHandlingPolicyOptions RemoveException(Type exceptionType)
        {
            if (this.handlers.Contains(exceptionType))
            {
                this.handlers.Remove(exceptionType);
            }

            return this;
        }

        public ExceptionHandlingPolicyOptions EnsureHandler(Type exceptionType, Type handlerType, int index = -1)
        {
            if (!typeof(IExceptionHandler).IsAssignableFrom(handlerType))
            {
                throw new ArgumentOutOfRangeException(nameof(handlerType), handlerType,
                    $"Handler type should implement {typeof(IExceptionHandler).Name}");
            }

            this.EnsureException(exceptionType);

            List<Type> list = this.handlers[exceptionType] as List<Type>;

            if (list.Any(type => type == handlerType))
            {
                if (index >= 0)
                {
                    list.Remove(handlerType);
                    list.Insert(index, handlerType);
                }
            }
            else
            {
                if (index < 0)
                {
                    list.Add(handlerType);
                }
                else
                {
                    list.Insert(index, handlerType);
                }
            }

            return this;
        }

        public ExceptionHandlingPolicyOptions RemoveHandler(Type exceptionType, Type handlerType)
        {
            if (this.handlers.Contains(exceptionType))
            {
                List<Type> list = this.handlers[exceptionType] as List<Type>;

                if (list.Contains(handlerType))
                {
                    list.Remove(handlerType);
                }
            }

            return this;
        }

        public ExceptionHandlingPolicyOptions ClearExceptions()
        {
            this.handlers.Clear();
            return this;
        }

        public ExceptionHandlingPolicyOptions ClearHandlers(Type exceptionType)
        {
            if (this.handlers.Contains(exceptionType))
            {
                List<Type> list = this.handlers[exceptionType] as List<Type>;

                list.Clear();
            }

            return this;
        }

        internal IEnumerable<Type> GetHandlersInternal(Type exceptionType)
        {
            if (this.handlers.Contains(exceptionType))
            {
                return this.handlers[exceptionType] as List<Type>;
            }

            return Enumerable.Empty<Type>();
        }

        internal IEnumerable<Type> GetExceptionsInternal()
        {
            return this.handlers.Keys.OfType<Type>();
        }
    }    
}