using System;
using System.Text.Json.Serialization;
using AspNetCore.Simple.Sdk.MediatR;

namespace AspNetCore.Simple.Sdk.Caching
{
    public interface ICachableQuery<out TResponse> : IQuery<TResponse> where TResponse : CachableObject
    {
        public bool UseCache { get; }

        public TimeSpan CacheTime { get; }

    }

    public record CachableQuery<TResponse>([property: JsonIgnore] TimeSpan CacheTime,
                                           [property: JsonIgnore] bool UseCache) : ICachableQuery<TResponse> where TResponse : CachableObject;
}
