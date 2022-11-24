using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AspNetCore.Simple.Sdk.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AspNetCore.Simple.Sdk.ApplicationInsight
{
    public static class AddLoggingHelperExtension
    {
        public static void AddLoggingHelper(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<LoggingHelper>();
        }
    }


    // ToDo: Check complete function here, which problem this class should be solved
    public class LoggingHelper
    {
        public IImmutableDictionary<string, string?> GetProperties(object data, string prefix = "")
        {
            var dictionary = new Dictionary<string, string?>();

            switch (data)
            {
                case null:
                    return dictionary.ToImmutableDictionary();
                case Dictionary<string, string?> d:
                    return d.ToImmutableDictionary();
                case Exception e:
                    dictionary.Add("Exception", e.GetType().Name);
                    FillExceptionProperties(dictionary, e, e.GetType().Name);
                    break;
                default:
                    {
                        var jToken = JToken.FromObject(data);

                        if (jToken is JObject jObject)
                        {
                            // Just objects are supported, no scalar values or Arrays
                            foreach (var property in jObject)
                            {
                                // Mapps just the first level of properties. All other properties will be handled as Json string
                                dictionary.Add(prefix + property.Key, SerializeForLogging(property.Value));
                            }
                        }
                        else
                        {
                            // direct format
                            dictionary.Add("data", jToken.ToString());
                        }

                        break;
                    }
            }

            return dictionary.ToImmutableDictionary();
        }

        private string SerializeForLogging(JToken? token)
        {
            if (token != null)
            {
                return token is JValue jValue ? jValue.ToString(CultureInfo.InvariantCulture) : JsonConvert.SerializeObject(token);
            }

            return string.Empty;
        }

        private void FillExceptionProperties(IDictionary<string, string?> dict, Exception e, string exceptionKey)
        {
            dict.Add($"{exceptionKey}.ExceptionDetails", e.ToString());

            foreach (var dataKey in e.Data.Keys)
            {
                var value = e.Data[dataKey];
                if (value is not null)
                {
                    dict.Add($"{exceptionKey}.Data.{dataKey}", value.ToString());
                }
            }

            foreach (var property in e.GetType().GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public).Where(p => p.Name != "Data"))
            {
                var propertyValue = property.GetValue(e);

                if (propertyValue != null)
                {
                    dict.Add($"{exceptionKey}.{property.Name}", propertyValue.ToString());
                }
            }

            if (e.InnerException != null)
            {
                FillExceptionProperties(dict, e.InnerException, $"{exceptionKey}.InnerException");
            }
        }
    }
}
