using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace VX_ACE_IT_CORE.MVC.Model.Configuration.Options
{
    public class JsonFieldsCollector
    {
        private readonly Dictionary<string, JValue> _fields;

        public JsonFieldsCollector(JToken token)
        {
            _fields = new Dictionary<string, JValue>();
            CollectFields(token);
        }

        private void CollectFields(JToken jToken)
        {
            while (true)
            {
                switch (jToken.Type)
                {
                    case JTokenType.Object:
                        foreach (var child in jToken.Children<JProperty>()) CollectFields(child);
                        break;
                    case JTokenType.Array:
                        foreach (var child in jToken.Children()) CollectFields(child);
                        break;
                    case JTokenType.Property:
                        jToken = ((JProperty) jToken).Value;
                        continue;
                    default:
                        _fields.Add(jToken.Path, (JValue) jToken);
                        break;
                }
                break;
            }
        }

        public IEnumerable<KeyValuePair<string, JValue>> GetAllFields() => _fields;
    }
}