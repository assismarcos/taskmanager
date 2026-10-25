using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace TaskManager.Api.Http;

public static class JsonOptionsSetup
{
    public static void Configure(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());

        // A body missing a non-nullable member (dueDate, status) is a 400, not a silent default.
        options.RespectRequiredConstructorParameters = true;
        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { MakeNullableMembersOptional } };
    }

    // Nullable members (description, create status) are optional in the API contract.
    private static void MakeNullableMembersOptional(JsonTypeInfo typeInfo)
    {
        var nullability = new NullabilityInfoContext();
        foreach (var property in typeInfo.Properties)
        {
            if (property.IsRequired
                && property.AttributeProvider is PropertyInfo info
                && nullability.Create(info).WriteState == NullabilityState.Nullable)
            {
                property.IsRequired = false;
            }
        }
    }
}
