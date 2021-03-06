//#define WriteTestFiles
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Threading.Tasks;
using Threax.AspNetCore.Tests;
using Xunit;

namespace Threax.Extensions.Configuration.SchemaBinder.Tests
{
    public class SchemaConfigurationBinderTests
    {
        private Mockup mockup = new Mockup();

#if WriteTestFiles
        private bool WriteTestFiles = true;
#else
        private bool WriteTestFiles = false;
#endif

        public SchemaConfigurationBinderTests()
        {
            var builder = new ConfigurationBuilder();
            mockup.Add<IConfiguration>(s =>
            {
                var mock = new Mock<IConfiguration>();
                mock.Setup(i => i.GetSection(It.IsAny<String>())).Returns(s.Get<IConfigurationSection>());
                mock.Setup(i => i[It.IsAny<String>()]).Returns("Hi");
                return mock.Object;
            });
            mockup.Add<SchemaConfigurationBinder>(s => new SchemaConfigurationBinder(s.Get<IConfiguration>()));
        }

        [Fact]
        public async Task EmptySchema()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "EmptySchema.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "EmptySchema.json"), json);
        }

        [Fact]
        public async Task OneObject()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Bind("AppConfig", new AppSettings());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "OneObject.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "OneObject.json"), json);
        }

        [Fact]
        public async Task TwoObjects()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Bind("AppConfig", new AppSettings());
            binder.Bind("ClientConfig", new ClientSettings());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "TwoObjects.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "TwoObjects.json"), json);
        }

        [Fact]
        public async Task WithComments()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Bind("AppConfig", new WithCommentsSettings());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "WithComments.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "WithComments.json"), json);
        }

        [Fact]
        public async Task ConfigSection()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.GetSection("GetSectionTest");
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "ConfigSection.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "ConfigSection.json"), json);
        }

        [Fact]
        public async Task ConfigSectionAndObject()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.GetSection("GetSectionTest");
            binder.Bind("AppConfig", new AppSettings());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "ConfigSectionAndObject.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "ConfigSectionAndObject.json"), json);
        }

        [Fact]
        public void GetValueTest()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            Assert.Equal("Hi", binder["GetValueTest"]);
            Assert.Equal("Hi", binder["TheyAllReturnHi"]);
        }

        [Fact]
        public async Task DefineTest()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Define("AppConfig", typeof(AppSettings));
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "DefineTest.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "DefineTest.json"), json);
        }

        [Fact]
        public async Task Inheritance()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Bind("AppConfig", new Subclass());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "Inheritance.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "Inheritance.json"), json);
        }

        [Fact]
        public async Task Enum()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Bind("AppConfig", new EnumTest());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "Enum.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "Enum.json"), json);
        }

        [Fact]
        public async Task OnePropTwoObjects()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Bind("ClientConfig", new AppSettings());
            binder.Bind("ClientConfig", new ClientSettings());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "OnePropTwoObjects.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "OnePropTwoObjects.json"), json);
        }

        [Fact]
        public async Task OnePropTwoObjectsEnum()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Bind("ClientConfig", new EnumTest());
            binder.Bind("ClientConfig", new ClientSettings());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "OnePropTwoObjectsEnum.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "OnePropTwoObjectsEnum.json"), json);
        }

        [Fact]
        public async Task OnePropTwoObjectsEnumSame()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Bind("ClientConfig", new EnumTest());
            binder.Bind("ClientConfig", new EnumTest());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "OnePropTwoObjectsEnumSame.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "OnePropTwoObjectsEnumSame.json"), json);
        }

        [Fact]
        public async Task OnePropTwoObjectsSame()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Bind("ClientConfig", new ClientSettings());
            binder.Bind("ClientConfig", new ClientSettings());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "OnePropTwoObjectsSame.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "OnePropTwoObjectsSame.json"), json);
        }

        [Fact]
        public async Task OnePropTwoObjectsDefine()
        {
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.Define("ClientConfig", typeof(AppSettings));
            binder.Bind("ClientConfig", new ClientSettings());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "OnePropTwoObjectsDefine.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "OnePropTwoObjectsDefine.json"), json);
        }

        [Fact]
        public async Task OnePropTwoObjectsWithObject()
        {
            //This test ensures that combining with an object returns a plain object definition.
            var binder = mockup.Get<SchemaConfigurationBinder>();
            binder.GetSection("ClientConfig");
            binder.Bind("ClientConfig", new ClientSettings());
            var json = await binder.CreateSchema();
            if (WriteTestFiles)
            {
                FileUtils.WriteTestFile(this.GetType(), "OnePropTwoObjectsWithObject.json", json);
            }
            Assert.Equal(FileUtils.ReadTestFile(this.GetType(), "OnePropTwoObjectsWithObject.json"), json);
        }
    }

    class AppSettings
    {
        public String ConnectionString { get; set; }

        public int AnotherAppSetting { get; set; }
    }

    class ClientSettings
    {
        public String ServiceUrl { get; set; }

        public String SomeOtherConfig { get; set; }
    }

    class WithCommentsSettings
    {
        /// <summary>
        /// This has a description.
        /// </summary>
        public String Test { get; set; }
    }

    class Superclass
    {
        public int SuperclassProp { get; set; }
    }

    class Subclass : Superclass
    {
        public int SubclassProp { get; set; }
    }

    enum TestEnum
    {
        Value1,
        Value2
    }

    class EnumTest
    {
        public TestEnum TestEnum { get; set; }
    }
}
