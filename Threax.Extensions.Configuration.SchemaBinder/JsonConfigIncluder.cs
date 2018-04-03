using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Extensions.Configuration
{
    public static class JsonConfigIncluder
    {
        /// <summary>
        /// Add a json configuration file that supports an Include property in the top level object. This Include property
        /// will be used to load additional json config files relative to the current file. Included files will be added to the
        /// builder before the passed in file allowing it to override settings from included files. If a file is included by a config
        /// file and isn't found a FileNotFound exception will be thrown.
        /// </summary>
        /// <param name="builder">The Microsoft.Extensions.Configuration.IConfigurationBuilder to add to.</param>
        /// <param name="path">Path relative to the base path stored in Microsoft.Extensions.Configuration.IConfigurationBuilder.Properties of builder.</param>
        /// <returns>The Microsoft.Extensions.Configuration.IConfigurationBuilder.</returns>
        public static IConfigurationBuilder AddJsonFileWithInclude(this IConfigurationBuilder builder, string path)
        {
            foreach(var include in FindIncludes(path, true))
            {
                builder.AddJsonFileWithInclude(include, false);
            }

            builder.AddJsonFile(path);

            return builder;
        }


        /// <summary>
        /// Add a json configuration file that supports an Include property in the top level object. This Include property
        /// will be used to load additional json config files relative to the current file. Included files will be added to the
        /// builder before the passed in file allowing it to override settings from included files. If a file is included by a config
        /// file and isn't found a FileNotFound exception will be thrown.
        /// </summary>
        /// <param name="builder">The Microsoft.Extensions.Configuration.IConfigurationBuilder to add to.</param>
        /// <param name="path">Path relative to the base path stored in Microsoft.Extensions.Configuration.IConfigurationBuilder.Properties of builder.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <returns>The Microsoft.Extensions.Configuration.IConfigurationBuilder.</returns>
        public static IConfigurationBuilder AddJsonFileWithInclude(this IConfigurationBuilder builder, string path, bool optional)
        {
            foreach (var include in FindIncludes(path, optional))
            {
                builder.AddJsonFileWithInclude(include, false);
            }

            builder.AddJsonFile(path, optional);

            return builder;
        }

        /// <summary>
        /// Add a json configuration file that supports an Include property in the top level object. This Include property
        /// will be used to load additional json config files relative to the current file. Included files will be added to the
        /// builder before the passed in file allowing it to override settings from included files. If a file is included by a config
        /// file and isn't found a FileNotFoundException will be thrown.
        /// </summary>
        /// <param name="builder">The Microsoft.Extensions.Configuration.IConfigurationBuilder to add to.</param>
        /// <param name="path">Path relative to the base path stored in Microsoft.Extensions.Configuration.IConfigurationBuilder.Properties of builder.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes. Will apply to included files.</param>
        /// <returns>The Microsoft.Extensions.Configuration.IConfigurationBuilder.</returns>
        public static IConfigurationBuilder AddJsonFileWithInclude(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            foreach (var include in FindIncludes(path, optional))
            {
                builder.AddJsonFileWithInclude(include, false, reloadOnChange);
            }

            builder.AddJsonFile(path, optional, reloadOnChange);

            return builder;
        }

        private static IEnumerable<String> FindIncludes(string path, bool optional)
        {
            var fullPath = Path.GetFullPath(path);
            var rootFolder = Path.GetDirectoryName(fullPath);
            //Load the file to see if there are any includes
            if (!File.Exists(fullPath))
            {
                if (optional)
                {
                    yield break;
                }
                throw new FileNotFoundException("Cannot find include file.", fullPath);
            }

            JObject jObj;
            using (var reader = new StreamReader(File.OpenRead(fullPath)))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    jObj = JToken.ReadFrom(jsonReader) as JObject;
                }
            }

            if(jObj == null)
            {
                yield break;
            }

            var includes = jObj["Include"] as JArray;
            if(includes == null)
            {
                yield break;
            }

            foreach(var include in includes)
            {
                var value = include as JValue;
                if (value != null && value.Type == JTokenType.String)
                {
                    yield return Path.Combine(rootFolder, value.Value<String>());
                }
            }
        }
    }
}
