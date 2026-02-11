using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace Cmf.CLI.Core.Repository.Credentials
{
    public class CredentialConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ICredential);
        }

        public override void WriteJson(JsonWriter writer,
            object? value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        protected string GetPropertyName(JsonSerializer serializer, string propertyName)
        {
            if (serializer.ContractResolver is DefaultContractResolver contractResolver)
            {
                return contractResolver.GetResolvedPropertyName(propertyName);
            } 
            else
            {
                return propertyName;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var credential = default(ICredential);

            var jsonObject = JObject.Load(reader);
            // Resolve the property name dynamically to respect the NamingStrategy (if configured) used to serialize/deserialize
            var authTypeName = GetPropertyName(serializer, nameof(ICredential.AuthType));

            var authType = (string?)jsonObject[authTypeName];

            if (string.Equals(authType, AuthType.Basic.ToString(), StringComparison.InvariantCultureIgnoreCase)) credential = new BasicCredential();
            else if (string.Equals(authType, AuthType.Bearer.ToString(), StringComparison.InvariantCultureIgnoreCase)) credential = new BearerCredential();
            else
            {
                throw new Exception($"Invalid \"authType\" property value of \"{authType}\" when attempting to deserialize {reader.Path}, expected one of: {AuthType.Basic.ToString()}, {AuthType.Bearer.ToString()}.");
            }

            // We need to create a new reader here, instead of using the local `reader` variable,
            // because that instance has already moved to the end of the object, when we did `JOBject.Load`
            serializer.Populate(jsonObject.CreateReader(), credential);

            return credential;
        }
    }
}
