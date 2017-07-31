using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.Qwiq.Tests.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Qwiq.Identity.Soap
{
    public interface ISerializeTests
    {
        void Serialize(Stream stream, object instance);
        TEntity Deserialize<TEntity>(Stream stream);
    }

    public class FormatterSerializeTests<TFormatter> : ISerializeTests
        where TFormatter : IFormatter, new()
    {
        private readonly TFormatter _formatter;

        public FormatterSerializeTests()
        {
            _formatter = new TFormatter();
        }

        public void Serialize(Stream targetStream, object instance)
        {
            _formatter.Serialize(targetStream, RuntimeHelpers.GetObjectValue(instance));
        }

        public TEntity Deserialize<TEntity>(Stream stream)
        {
            var ret = _formatter.Deserialize(stream);
            return (TEntity)ret;
        }
    }

    public class JsonFormatterSierializeTests : ISerializeTests
    {
        private readonly JsonSerializerSettings _settings;

        public JsonFormatterSierializeTests()
        {
            _settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects
            };
        }

        public void Serialize(Stream ms, object instance)
        {
            var writer = new StreamWriter(ms);

            writer.WriteLine(JsonConvert.SerializeObject(instance, _settings));

            writer.Flush();
        }

        public TEntity Deserialize<TEntity>(Stream stream)
        {
            var sr = new StreamReader(stream);
            var myStr = sr.ReadToEnd();
            return JsonConvert.DeserializeObject<TEntity>(myStr, _settings);
        }
    }

    public abstract class TeamFoundationIdentitySerializationContextSpecification : TimedContextSpecification
    {
        public ISerializeTests Formatter { get; set; }
        private string _input;
        public ITeamFoundationIdentity IdentityManagementServiceIdentity { get; private set; }
        public ITeamFoundationIdentity SerializedIdentity { get; private set; }
        protected IIdentityManagementService Instance { get; private set; }
        protected bool EnableExceptionProxy { get; set; }

        [TestMethod]
        [TestCategory("localOnly")]
        [TestCategory("SOAP")]
        public void The_Identity_Is_Equal_to_Origin_after_SerializationDeserialization_Cycle()
        {
            SerializedIdentity.ShouldEqual(IdentityManagementServiceIdentity, Comparer.TeamFoundationIdentity);
        }

        [TestMethod]
        [TestCategory("localOnly")]
        [TestCategory("SOAP")]
        public void Properties_are_Equal()
        {
            var collection = SerializedIdentity.GetProperties();
            var expected = IdentityManagementServiceIdentity.GetProperties();

            var source = new Dictionary<string, object>(Comparer.OrdinalIgnoreCase);
            var noContain = new List<KeyValuePair<string, object>>();
            foreach (var i in collection)
            {
                source.Add(i.Key, i.Value);
            }

            foreach (var item in expected)
            {
                if (!source.ContainsKey(item.Key)) noContain.Add(item);
                else
                {
                    var v = source[item.Key];
                    if (GenericComparer<object>.Default.Equals(item.Value, v))
                    {
                        source.Remove(item.Key);
                    }
                }
            }

            if (noContain.Any() || source.Any())
            {
                var message = $"Should contain only: {expected.EachToUsefulString()} \r\nentire list: {collection.EachToUsefulString()}";

                if (noContain.Any()) message += "\ndoes not contain: " + noContain.EachToUsefulString();

                if (source.Any()) message += "\ndoes contain but shouldn't: " + source.EachToUsefulString();

                throw new AssertFailedException(message);
            }
        }

        [TestMethod]
        [TestCategory("localOnly")]
        [TestCategory("SOAP")]
        public void Members_are_Equal()
        {
            SerializedIdentity.Members.ShouldContainOnly(IdentityManagementServiceIdentity.Members);
        }

        [TestMethod]
        [TestCategory("localOnly")]
        [TestCategory("SOAP")]
        public void MemberOf_are_Equal()
        {
            SerializedIdentity.MemberOf.ShouldContainOnly(IdentityManagementServiceIdentity.MemberOf);
        }

        [TestMethod]
        [TestCategory("localOnly")]
        [TestCategory("SOAP")]
        public void Property_located_different_case()
        {
            var pv1 = SerializedIdentity.GetProperty("account");
            var pv2 = IdentityManagementServiceIdentity.GetProperty("account");

            pv1.ShouldEqual(pv2);

            // This is important as depending on how the items are serialized then deserialized, the names of the keys may differ in case
        }

        public override void Given()
        {
            var wis = TimedAction(() => IntegrationSettings.CreateSoapStore(), "SOAP", "WIS Create");
            wis.Configuration.ProxyCreationEnabled = EnableExceptionProxy;

            Instance = TimedAction(() => wis.GetIdentityManagementService(), "SOAP", "IMS Create");

            _input = "rimuri@microsoft.com";

        }

        public override void When()
        {
            IdentityManagementServiceIdentity = Instance.ReadIdentity(IdentitySearchFactor.AccountName, _input, MembershipQuery.Expanded);
            SerializedIdentity = TestObjectSerialization(IdentityManagementServiceIdentity);
        }

        private T Deserialize<T>(Stream obj)
        {
            var ret = Formatter.Deserialize<T>(obj);

            if (obj is FileStream)
            {
                obj.Close();
            }

            return (T)ret;
        }

        private MemoryStream SerializeToMemoryStream<T>(T instance)
        {
            var targetStream = new MemoryStream();
            Formatter.Serialize(targetStream, RuntimeHelpers.GetObjectValue(instance));

            Debug.WriteLine($"Serialized size: {targetStream.Position}");

            targetStream.Position = 0;
            var sr = new StreamReader(targetStream);

                var myStr = sr.ReadToEnd();
                Debug.WriteLine(myStr);

            targetStream.Position = 0;

            return targetStream;
        }

        private T TestObjectSerialization<T>(T instance) where T : ITeamFoundationIdentity
        {
            using (var tmp = SerializeToMemoryStream(RuntimeHelpers.GetObjectValue(instance)))
            {
                return Deserialize<T>(tmp);
            }
        }
    }

    [TestClass]
    [Ignore]
    public class
        Given_a_SOAP_TeamFoundationIdentity_With_Proxy_and_BinaryFormatter :
            TeamFoundationIdentitySerializationContextSpecification
    {
        public override void Given()
        {
            EnableExceptionProxy = true;
            Formatter = new FormatterSerializeTests<BinaryFormatter>();
            base.Given();
        }
    }

    [TestClass]
    [Ignore]
    public class
        Given_a_SOAP_TeamFoundationIdentity_With_Proxy_and_SoapFormatter :
            TeamFoundationIdentitySerializationContextSpecification
    {
        public override void Given()
        {
            EnableExceptionProxy = true;
            Formatter = new FormatterSerializeTests<SoapFormatter>();
            base.Given();
        }
    }

    [TestClass]
    [Ignore]
    public class
        Given_a_SOAP_TeamFoundationIdentity_With_Proxy_and_JsonFormatter :
            TeamFoundationIdentitySerializationContextSpecification
    {
        public override void Given()
        {
            EnableExceptionProxy = true;
            Formatter = new JsonFormatterSierializeTests();
            base.Given();
        }
    }

    [TestClass]
    public class
        Given_a_SOAP_TeamFoundationIdentity_Without_Proxy_and_BinaryFormatter :
            TeamFoundationIdentitySerializationContextSpecification
    {
        public override void Given()
        {
            EnableExceptionProxy = false;
            Formatter = new FormatterSerializeTests<BinaryFormatter>();
            base.Given();
        }
    }

    [TestClass]
    [Ignore]
    public class
        Given_a_SOAP_TeamFoundationIdentity_Without_Proxy_and_SoapFormatter :
            TeamFoundationIdentitySerializationContextSpecification
    {
        public override void Given()
        {
            EnableExceptionProxy = false;
            Formatter = new FormatterSerializeTests<SoapFormatter>();
            base.Given();
        }
    }

    [TestClass]
    public class
        Given_a_SOAP_TeamFoundationIdentity_Without_Proxy_and_JsonFormatter :
            TeamFoundationIdentitySerializationContextSpecification
    {
        public override void Given()
        {
            EnableExceptionProxy = false;
            Formatter = new JsonFormatterSierializeTests();
            base.Given();
        }
    }
}