using System;
using System.Text.Json;

namespace AspNetCore.Simple.Sdk.Serializer.Json
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class ShowJsonOnErrorAttribute : Attribute
    {
    }

    public interface IJsonSerializer
    {
        /// <summary>
        /// Converts the provided value into a <see cref="string"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <returns>A <see cref="string"/> representation of the value.</returns>
        /// <param name="source">The source to convert.</param>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter"/>
        /// for <typeparamref name="T"/> or its serializable members.
        /// </exception>
        string Serialize<T>(T source);

        /// <summary>
        /// Converts the provided value into a <see cref="string"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value to serialize.</typeparam>
        /// <returns>A <see cref="string"/> representation of the value.</returns>
        /// <param name="source">The source to convert.</param>
        /// <param name="defaultValue">A optional default value if result will be <see langword="null"/> you can define alternative return value..</param>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter"/>
        /// for <typeparamref name="T"/> or its serializable members.
        /// </exception>
        string? SerializeOrDefault<T>(T source, string? defaultValue = default);

        /// <summary>
        /// Parses the text representing a single JSON value into a <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON value into.</typeparam>
        /// <returns>A <typeparamref name="T"/> representation of the JSON value.</returns>
        /// <param name="json">JSON text to parse.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="json"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="JsonException">
        /// The JSON is invalid.
        ///
        /// -or-
        ///
        /// <typeparamref name="T" /> is not compatible with the JSON.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single JSON value.</exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter"/>
        /// for <typeparamref name="T"/> or its serializable members.
        /// </exception>
        T Deserialize<T>(string json);

        /// <summary>
        /// Parses the text representing a single JSON value into a <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON value into.</typeparam>
        /// <returns>A <typeparamref name="T"/> representation of the JSON value.</returns>
        /// <param name="json">JSON text to parse.</param>
        /// <param name="defaultValue">A optional default value if result will be <see langword="null"/> you can define alternative return value..</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="json"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="JsonException">
        /// The JSON is invalid.
        ///
        /// -or-
        ///
        /// <typeparamref name="T" /> is not compatible with the JSON.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single JSON value.</exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter"/>
        /// for <typeparamref name="T"/> or its serializable members.
        /// </exception>
        T? DeserializeOrDefault<T>(string json, T? defaultValue = default);

        /// <summary>
        /// Parses the text representing a single JSON value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the JSON value.</returns>
        /// <param name="json">JSON text to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="json"/> or <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="JsonException">
        /// The JSON is invalid.
        ///
        /// -or-
        ///
        /// <paramref name="returnType"/> is not compatible with the JSON.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single JSON value.</exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        object Deserialize<T>(string json, Type returnType);

        /// <summary>
        /// Parses the text representing a single JSON value into a <paramref name="returnType"/>.
        /// </summary>
        /// <returns>A <paramref name="returnType"/> representation of the JSON value.</returns>
        /// <param name="json">JSON text to parse.</param>
        /// <param name="returnType">The type of the object to convert to and return.</param>
        /// <param name="defaultValue">A optional default value if result will be <see langword="null"/> you can define alternative return value..</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="json"/> or <paramref name="returnType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="JsonException">
        /// The JSON is invalid.
        ///
        /// -or-
        ///
        /// <paramref name="returnType"/> is not compatible with the JSON.
        ///
        /// -or-
        ///
        /// There is remaining data in the string beyond a single JSON value.</exception>
        /// <exception cref="NotSupportedException">
        /// There is no compatible <see cref="System.Text.Json.Serialization.JsonConverter"/>
        /// for <paramref name="returnType"/> or its serializable members.
        /// </exception>
        object? DeserializeOrDefault(string json, Type returnType, object? defaultValue = default);
    }
}
