using System;
using System.Linq;
using System.Runtime.Serialization;
using Extensions.Pack;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    /// <summary>A schema-and-parameter filter that replaces, in the generated Json, each <see langword="enum"/>-based definition
    /// with a <see langword="string"/>-based definition plus "x-extensible-enum" list (a list of the enum member names.)<br />
    /// As a consequence, names instead of numbers will be used in the client code generators' IntelliSense and validations.
    /// Unlike <see cref="EnumSchemaFilter "/>, when the C# enum's members grow, this will not signify a breaking change
    /// for consuming clients.<br />
    /// See also <a href="https://opensource.zalando.com/restful-api-guidelines/#112">guideline #112</a> by Zalando, who invented this mechanism.
    /// </summary>
    public class ExtensibleEnumFilter : ISchemaFilter, IParameterFilter
    {
        void ISchemaFilter.Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            RedefineSchemaIfEnumOrNullableEnumType(context.Type, model);
        }


        void IParameterFilter.Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            RedefineSchemaIfEnumOrNullableEnumType(context.ParameterInfo?.ParameterType, parameter.Schema);
        }

        private static void RedefineSchemaIfEnumOrNullableEnumType(Type? type, OpenApiSchema schema)
        {
            if (type is null)
            {
                return;
            }

            if (!type.IsEnum)
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType is not { IsEnum: true })
                {
                    return;
                }
                type = underlyingType;
            }

            schema.Enum.Clear();
            // If the schema belongs to an ISchemaFilter filter, its Reference is already null (because not needed, since the schema "is just itself").
            // If the schema belongs to an IParameterFilter, its Reference must be made null, or else Swashbuckle would ignore the "string" Type
            // and "x-extensible-enum" extension that we define later on.
            schema.Reference = null;
            schema.Type = "string";
            schema.Format = null;

            var openApiStrings = Enum
                .GetNames(type)
                .Select(name => new
                {
                    OriginalName = name,
                    AttributedName = type.GetMember(name)[0].GetCustomAttributes(typeof(EnumMemberAttribute), false).OfType<EnumMemberAttribute>().FirstOrDefault()?.Value
                })
                .Select(nameSpec => nameSpec.AttributedName.IsNullOrWhiteSpace() ? nameSpec.OriginalName : nameSpec.AttributedName)
                .Select(resolvedName => new OpenApiString(resolvedName));

            var openApiArray = new OpenApiArray();
            openApiArray.AddRange(openApiStrings);
            schema.Extensions.Clear();
            schema.Extensions.Add("x-extensible-enum", openApiArray);
        }
    }
}
