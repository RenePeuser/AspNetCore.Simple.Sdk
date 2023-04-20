using System;
using AspNetCore.Simple.Sdk.ErrorHandling;

namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    public interface IJsonSerializer
    {
        /// <summary>
        /// Serialize an object to a json string. It will throw an exception if it was not successful
        /// </summary>
        /// <typeparam name="T">The generic <see cref="T"/></typeparam>
        /// <param name="source">The object which have to serialized</param>
        /// <returns>The json string of the <see cref="source"/></returns>
        /// <exception>Throws a <see cref="ProblemDetailsException"/> if anything went wrong /></exception>
        string Serialize<T>(T source);

        /// <summary>
        /// Serialize a json string into an expected object of <see cref="T"/> or if it not possible <see cref="defaultValue"/> will be returned. It will NOT throw an exception if it was not successful
        /// </summary>
        /// <typeparam name="T">The generic <see cref="T"/></typeparam>
        /// <param name="source">The object which have to serialized</param>
        /// <param name="defaultValue">A custom default value which you can set optional if something went wrong</param>
        /// <returns>The json string of the <see cref="string"/> or default</returns>
        string? SerializeOrDefault<T>(T source, string? defaultValue = default);

        /// <summary>
        /// Deserialize a json string into an expected object of <see cref="T"/>. It will throw an exception if it was not successful
        /// </summary>
        /// <typeparam name="T">The generic <see cref="T"/></typeparam>
        /// <param name="json">The json string which have to be deserialized into type of <see cref="T"/></param>
        /// <returns>The json string of the <see cref="json"/></returns>
        /// <exception>Throws a <see cref="ProblemDetailsException"/> if anything went wrong /></exception>
        T Deserialize<T>(string json);

        /// <summary>
        /// Deserialize a json string into an expected object of <see cref="T"/> or if it not possible <see cref="defaultValue"/> will be returned. It will NOT throw an exception if it was not successful
        /// </summary>
        /// <typeparam name="T">The generic <see cref="T"/></typeparam>
        /// <param name="json">The json string which have to be deserialized into type of <see cref="T"/></param>
        /// <param name="defaultValue">A custom default value which you can set optional if something went wrong</param>
        /// <returns>The json string of the <see cref="json"/></returns>
        T? DeserializeOrDefault<T>(string json, T? defaultValue = default);

        /// <summary>
        /// Deserialize a json string into an expected object of <see cref="responseType"/>. It will throw an exception if it was not successful
        /// </summary>
        /// <param name="json">The json string which have to be deserialized into type of <see cref="T"/></param>
        /// <param name="responseType">The <see cref="Type"/> of the expected <see cref="Type"/> you expect.</param>
        /// <exception>Throws a <see cref="ProblemDetailsException"/> if anything went wrong /></exception>
        object Deserialize<T>(string json, Type responseType);

        /// <summary>
        /// Deserialize a json string into an expected object of <see cref="responseType"/> or if it not possible <see cref="defaultValue"/> will be returned. It will NOT throw an exception if it was not successful
        /// </summary>
        /// <param name="json">The json string which have to be deserialized into type of <see cref="responseType"/></param>
        /// <param name="responseType"></param>
        /// <param name="defaultValue">A custom default value which you can set optional if something went wrong</param>
        /// <returns>The json string of the <see cref="json"/></returns>
        object? DeserializeOrDefault(string json, Type responseType, object? defaultValue = default);
    }
}
