using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using NJsonSchema;
using NJsonSchema.Generation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Threax.Extensions.Configuration.SchemaBinder
{
    /// <summary>
    /// This class wraps a IConfiguration and keeps track of the objects that are bound from it.
    /// This information can then be used to generate a json schema for the app's settings, which
    /// provide nice intellisense in some editors.
    /// </summary>
    public class SchemaConfigurationBinder : IConfiguration
    {
        private IConfiguration config;
        private Dictionary<String, Type> configObjects = new Dictionary<String, Type>();

        public SchemaConfigurationBinder(IConfiguration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Bind the object instace to the section named section. This will also record
        /// the type of instance and will add that type with the section name to the schema generated
        /// by CreateSchema();
        /// </summary>
        /// <param name="section">The name of the section.</param>
        /// <param name="instance">The object instance to bind to section.</param>
        public void Bind(String section, Object instance)
        {
            configObjects[section] = instance.GetType();
            config.Bind(section, instance);
        }

        /// <summary>
        /// Define a section to be typed by type. This can be used if the Bind function would not be called on
        /// startup for the given section and you want to define it.
        /// </summary>
        /// <param name="section">The name of the section.</param>
        /// <param name="type">The type to use.</param>
        public void Define(String section, Type type)
        {
            configObjects[section] = type;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return config.GetChildren();
        }

        public IChangeToken GetReloadToken()
        {
            return config.GetReloadToken();
        }

        /// <summary>
        /// Get a config section if one needs to be passed on. Since no object is provided this section
        /// will be added to the schema as a plain object.
        /// </summary>
        /// <param name="key">The name of the section.</param>
        /// <returns></returns>
        public IConfigurationSection GetSection(String key)
        {
            configObjects[key] = typeof(Object);
            return config.GetSection(key);
        }

        /// <summary>
        /// Create a json schema from all discovered config object types.
        /// </summary>
        /// <returns>The json schema as a string.</returns>
        public async Task<String> CreateSchema()
        {
            var settings = new JsonSchemaGeneratorSettings()
            {
                FlattenInheritanceHierarchy = true,
                DefaultEnumHandling = EnumHandling.String
            };
            var generator = new JsonSchemaGenerator(settings);

            var schema = new JsonSchema4();
            foreach (var itemKey in configObjects.Keys)
            {
                var item = configObjects[itemKey];
                if (item == typeof(Object))
                {
                    schema.Properties.Add(itemKey, new JsonProperty()
                    {
                        Type = JsonObjectType.Null | JsonObjectType.Object
                    });
                }
                else
                {
                    var itemSchema = await generator.GenerateAsync(item);
                    schema.Properties.Add(itemKey, new JsonProperty()
                    {
                        Reference = itemSchema
                    });
                    schema.Definitions.Add(itemKey, itemSchema);
                }
            }
            return schema.ToJson();
        }

        public string this[string key]
        {
            get
            {
                return config[key];
            }
            set
            {
                config[key] = value;
            }
        }
    }
}
