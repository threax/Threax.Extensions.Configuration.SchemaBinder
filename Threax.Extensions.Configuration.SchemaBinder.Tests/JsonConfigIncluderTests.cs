using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Threax.AspNetCore.Tests;
using Xunit;

namespace Threax.Extensions.Configuration.SchemaBinder.Tests
{
    public class JsonConfigIncluderTests
    {
        private String TestFilePath => Path.Combine(FileUtils.TestFileDirectory, "IncludeTestFiles");

        private Mockup mockup = new Mockup();
        private List<JsonConfigurationSource> sources = new List<JsonConfigurationSource>();

        public JsonConfigIncluderTests()
        {
            mockup.Add<IConfigurationBuilder>(s =>
            {
                var mock = new Mock<IConfigurationBuilder>();

                mock.Setup(i => i.Add(It.IsAny<IConfigurationSource>())).Callback<IConfigurationSource>((cs) =>
                {
                    var jsonCs = cs as JsonConfigurationSource;
                    if (jsonCs != null)
                    {
                        sources.Add(jsonCs);
                    }
                }).Returns(() => mock.Object);

                return mock.Object;
            });
        }

        [Fact]
        public void SimpleInclude()
        {
            var builder = mockup.Get<IConfigurationBuilder>();
            builder.AddJsonFileWithInclude(Path.Combine(TestFilePath, "main.json"));
            Assert.Equal(2, sources.Count);
            Assert.Equal("include.json", sources[0].Path);
            Assert.Equal("main.json", sources[1].Path);
        }

        [Fact]
        public void SimpleIncludeOptional()
        {
            var builder = mockup.Get<IConfigurationBuilder>();
            builder.AddJsonFileWithInclude(Path.Combine(TestFilePath, "main.json"), true);
            Assert.Equal(2, sources.Count);
            Assert.Equal("include.json", sources[0].Path);
            Assert.Equal("main.json", sources[1].Path);
        }

        [Fact]
        public void SimpleIncludeOptionalReloadOnChange()
        {
            var builder = mockup.Get<IConfigurationBuilder>();
            builder.AddJsonFileWithInclude(Path.Combine(TestFilePath, "main.json"), true, false);
            Assert.Equal(2, sources.Count);
            Assert.Equal("include.json", sources[0].Path);
            Assert.Equal("main.json", sources[1].Path);
        }

        [Fact]
        public void NoIncludes()
        {
            var builder = mockup.Get<IConfigurationBuilder>();
            builder.AddJsonFileWithInclude(Path.Combine(TestFilePath, "noinclude.json"), true, false);
            Assert.Single(sources);
            Assert.Equal("noinclude.json", sources[0].Path);
        }

        [Fact]
        public void Empty()
        {
            var builder = mockup.Get<IConfigurationBuilder>();
            Assert.Throws<JsonReaderException>(() => builder.AddJsonFileWithInclude(Path.Combine(TestFilePath, "empty.json"), true, false));
        }

        [Fact]
        public void NotFound()
        {
            var builder = mockup.Get<IConfigurationBuilder>();
            Assert.Throws<FileNotFoundException>(() => builder.AddJsonFileWithInclude(Path.Combine(TestFilePath, "notfound.json"), true, false));
        }

        [Fact]
        public void Nested()
        {
            var builder = mockup.Get<IConfigurationBuilder>();
            builder.AddJsonFileWithInclude(Path.Combine(TestFilePath, "Subfolder/AnotherNesting/nested.json"), true, false);
            Assert.Equal(3, sources.Count);
            Assert.Equal("include.json", sources[0].Path);
            Assert.Equal("insubfolder.json", sources[1].Path);
            Assert.Equal("nested.json", sources[2].Path);
        }
    }
}
