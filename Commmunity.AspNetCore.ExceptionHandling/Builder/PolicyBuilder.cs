using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Commmunity.AspNetCore.ExceptionHandling.Builder
{
    public class PolicyBuilder : IExceptionPolicyBuilder, IServiceCollection
    {
        public IServiceCollection Services { get; }

        public ExceptionHandlingPolicyOptions Options { get; } = new ExceptionHandlingPolicyOptions();

        public PolicyBuilder(IServiceCollection services)
        {
            this.Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return Services.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Services).GetEnumerator();
        }

        public void Add(ServiceDescriptor item)
        {
            Services.Add(item);
        }

        public void Clear()
        {
            Services.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return Services.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            Services.CopyTo(array, arrayIndex);
        }

        public bool Remove(ServiceDescriptor item)
        {
            return Services.Remove(item);
        }

        public int Count => Services.Count;

        public bool IsReadOnly => Services.IsReadOnly;

        public int IndexOf(ServiceDescriptor item)
        {
            return Services.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            Services.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Services.RemoveAt(index);
        }

        public ServiceDescriptor this[int index]
        {
            get => Services[index];
            set => Services[index] = value;
        }
    }
}