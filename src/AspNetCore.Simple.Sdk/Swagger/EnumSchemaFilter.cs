using System;
using System.Linq;
using System.Runtime.Serialization;
using Extensions.Pack;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspNetCore.Simple.Sdk.Swagger
{
    /// <summary>A schema filter that removes, in the generated Json, the numeric member values from the <see langword="enum"/>s definitions and
    /// and replaces them with the <see langword="string"/> member names. However, the data type of the referencing parameters and properties
    /// remains numeric (usually <see langword="int"/>).<br />
    /// <br />
    /// As a consequence, names instead of numbers will be used in the client code generators' IntelliSense and validations.
    /// </summary>
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                foreach (var enumName in Enum.GetNames(context.Type))
                {
                    var memberInfo = context.Type.GetMember(enumName).FirstOrDefault(m => m.DeclaringType == context.Type);
                    var enumMemberAttribute = memberInfo?.GetCustomAttributes(typeof(EnumMemberAttribute), false).OfType<EnumMemberAttribute>().FirstOrDefault();
                    var label = enumMemberAttribute == null || enumMemberAttribute.Value.IsNullOrWhiteSpace()
                        ? enumName
                        : enumMemberAttribute.Value;
                    schema.Enum.Add(new OpenApiString(label));
                }
            }
        }
    }
}
