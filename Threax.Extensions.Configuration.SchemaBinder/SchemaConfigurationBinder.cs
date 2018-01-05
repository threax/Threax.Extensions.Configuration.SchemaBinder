﻿using Microsoft.Extensions.Configuration;
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
    public class SchemaConfigurationBinder
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
        /// Get a config section if one needs to be passed on. Since no object is provided this section
        /// will be added to the schema as a plain object.
        /// </summary>
        /// <param name="section">The name of the section.</param>
        /// <returns></returns>
        public IConfigurationSection GetSection(String section)
        {
            configObjects[section] = typeof(Object);
            return config.GetSection(section);
        }

        /// <summary>
        /// Create a json schema from all discovered config object types.
        /// </summary>
        /// <returns>The json schema as a string.</returns>
        public async Task<String> CreateSchema()
        {
            var settings = new JsonSchemaGeneratorSettings();
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
    }
}
