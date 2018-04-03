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
    public static class IConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddJsonFileWithIncludes(this IConfigurationBuilder builder, string path)
        {
            foreach(var include in FindIncludes(path))
            {

            }

            builder.AddJsonFile(path);

            return builder;
        }

        public static IConfigurationBuilder AddJsonFileWithIncludes(this IConfigurationBuilder builder, string path, bool optional)
        {
            builder.AddJsonFile(path, optional);

            return builder;
        }

        public static IConfigurationBuilder AddJsonFileWithIncludes(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            builder.AddJsonFile(path, optional, reloadOnChange);

            return builder;
        }

        public static IConfigurationBuilder AddJsonFileWithIncludes(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
        {
            builder.AddJsonFile(provider, path, optional, reloadOnChange);

            return builder;
        }

        //public static IConfigurationBuilder AddJsonFileWithIncludes(this IConfigurationBuilder builder, Action<JsonConfigurationSource> configureSource)
        //{

        //}

        private static IEnumerable<String> FindIncludes(string path)
        {
            var fullPath = Path.GetFullPath(path);
            //Load the file to see if there are any includes
            if (!File.Exists(fullPath))
            {
                yield break;
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
                
            }
        }
    }
}
