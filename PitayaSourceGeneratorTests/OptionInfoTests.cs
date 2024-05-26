using CLIParserSourceGeneratorTests.Fakes;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CLIParserSourceGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIParserSourceGeneratorTests
{
    [TestClass]
    public class OptionInfoTests
    {
        [TestMethod]
        public void CreateTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue");

            var optionInfo = OptionInfo.Create(parameter);

            Assert.IsNotNull(optionInfo);
            Assert.AreEqual("--a-value", optionInfo.OptionName);
            Assert.AreEqual("aValue", optionInfo.PropertyName);
            Assert.AreEqual(parameter, optionInfo.Parameter);
            Assert.IsFalse(optionInfo.IsArrayLike);
            Assert.IsNull(optionInfo.BackingListName);
            Assert.IsNotNull(optionInfo.ValueSetPropertyName);
        }

        [TestMethod]
        public void GenerateOptionPropertiesTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue");
            var optionInfo = OptionInfo.Create(parameter);
            var properties = optionInfo.GenerateOptionProperties();
            Assert.AreEqual(2, properties.Count);
            Assert.AreEqual("public int aValue { get; set; }", properties[0].ToFullString());
            Assert.AreEqual("public bool _aValueValueSet { get; set; }", properties[1].ToFullString());
        }

        [TestMethod]
        public void GenerateOptionProperties_Optional_Test()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", hasDefaultValue: true, defaultValue: 42);
            var optionInfo = OptionInfo.Create(parameter);
            var properties = optionInfo.GenerateOptionProperties();
            Assert.AreEqual(1, properties.Count);
            Assert.AreEqual("public int aValue { get; set; } = 42;", properties[0].ToFullString());
        }

        [TestMethod]
        public void ValueSetPropertyTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue");
            var optionInfo = OptionInfo.Create(parameter);
            var property = optionInfo.ValueSetProperty();
            Assert.IsNotNull(property);
            Assert.AreEqual("public bool _aValueValueSet { get; set; }", property.ToFullString());
        }

        [TestMethod]
        public void ListBackingArrayPropertyTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", type: FakeParameterInfo.ArrayTypeSymbol(null));
            var optionInfo = OptionInfo.Create(parameter);
            var property = optionInfo.ListBackingProperty();
            Assert.IsNotNull(property);
            Assert.AreEqual("public List<int> _aValueBackingList = new();", property.ToFullString());
        }

        [TestMethod]
        public void BasicArrayPropertyTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", typeName: "string[]");
            var optionInfo = OptionInfo.Create(parameter);
            var property = optionInfo.BasicArrayProperty();
            Assert.IsNotNull(property);
            Assert.AreEqual("public string[] aValue { get { return this._aValueBackingList.ToArray(); } }", property.ToFullString());
        }

        [TestMethod]
        public void BasicListPropertyTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", typeName: "System.Collections.Generic.List<string>");
            var optionInfo = OptionInfo.Create(parameter);
            var property = optionInfo.BasicArrayProperty();
            Assert.IsNotNull(property);
            Assert.AreEqual("public System.Collections.Generic.List<string> aValue { get { return this._aValueBackingList; } }", property.ToFullString());
        }

        [TestMethod]
        public void GetArrayConversionMethodTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", typeName: "string[]");
            var optionInfo = OptionInfo.Create(parameter);
            var conversionMethod = optionInfo.GetArrayConversionMethod();
            Assert.IsNotNull(conversionMethod);
            Assert.AreEqual(".ToArray()", conversionMethod);
        }

        [TestMethod]
        public void GetListConversionMethodTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", typeName: "System.Collections.Generic.List<string>");
            var optionInfo = OptionInfo.Create(parameter);
            var conversionMethod = optionInfo.GetArrayConversionMethod();
            Assert.IsNotNull(conversionMethod);
            Assert.AreEqual("", conversionMethod);
        }

        [TestMethod]
        public void BasicPropertyTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue");
            var optionInfo = OptionInfo.Create(parameter);
            var property = optionInfo.BasicProperty();
            Assert.IsNotNull(property);
            Assert.AreEqual("public int aValue { get; set; }", property.ToFullString());
        }

        [TestMethod]
        public void BasicProperty_String_Test()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", typeName: "string");
            var optionInfo = OptionInfo.Create(parameter);
            var property = optionInfo.BasicProperty();
            Assert.IsNotNull(property);
            Assert.AreEqual("public string aValue { get; set; }", property.ToFullString());
        }

        [TestMethod]
        public void BasicProperty_Nullable_Test()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", nullableAnnotation: NullableAnnotation.Annotated);
            var optionInfo = OptionInfo.Create(parameter);
            var property = optionInfo.BasicProperty();
            Assert.IsNotNull(property);
            Assert.AreEqual("public int? aValue { get; set; }", property.ToFullString());
        }

        [TestMethod]
        public void GetDefaultValueTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", hasDefaultValue: true, defaultValue: 42);
            var optionInfo = OptionInfo.Create(parameter);
            var defaultValue = optionInfo.GetDefaultValue();
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual("42", defaultValue.ToString());
        }

        [TestMethod]
        public void GetDefaultValue_String_Test()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", typeName: "string", hasDefaultValue: true, defaultValue: "bob");
            var optionInfo = OptionInfo.Create(parameter);
            var defaultValue = optionInfo.GetDefaultValue();
            Assert.IsNotNull(defaultValue);
            Assert.AreEqual("\"bob\"", defaultValue.ToString());
        }

        [TestMethod]
        public void GenerateSwitchSectionTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue");
            var optionInfo = OptionInfo.Create(parameter);
            var switchSection = optionInfo.GenerateSwitchSection();
            Assert.IsNotNull(switchSection);
            string expected = """
                case "--a-value":
                    options.aValue = int.Parse(args[i]);
                    options._aValueValueSet = true;
                    break;
                """;
            Assert.AreEqual(expected, switchSection.NormalizeWhitespace().ToFullString());
        }

        [TestMethod]
        public void GenerateSwitchSection_String_Test()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", typeName: "string");
            var optionInfo = OptionInfo.Create(parameter);
            var switchSection = optionInfo.GenerateSwitchSection();
            Assert.IsNotNull(switchSection);
            string expected = """
                case "--a-value":
                    options.aValue = args[i];
                    options._aValueValueSet = true;
                    break;
                """;
            Assert.AreEqual(expected, switchSection.NormalizeWhitespace().ToFullString());
        }

        [TestMethod]
        public void GenerateSwitchSection_Uri_Test()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", typeName: "System.Uri");
            var optionInfo = OptionInfo.Create(parameter);
            var switchSection = optionInfo.GenerateSwitchSection();
            Assert.IsNotNull(switchSection);
            string expected = """
                case "--a-value":
                    options.aValue = new Uri(args[i]);
                    options._aValueValueSet = true;
                    break;
                """;
            Assert.AreEqual(expected, switchSection.NormalizeWhitespace().ToFullString());
        }

        [TestMethod]
        public void GenerateSwitchSection_FileInfo_Test()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", typeName: "System.IO.FileInfo", members: [ FakeParameterInfo.Constructor ]);
            var optionInfo = OptionInfo.Create(parameter);
            var switchSection = optionInfo.GenerateSwitchSection();
            Assert.IsNotNull(switchSection);
            string expected = """
                case "--a-value":
                    options.aValue = new System.IO.FileInfo(args[i]);
                    options._aValueValueSet = true;
                    break;
                """;
            Assert.AreEqual(expected, switchSection.NormalizeWhitespace().ToFullString());
        }

        [TestMethod]
        public void GenerateSwitchSection_Optional_Test()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", typeName: "uint", hasDefaultValue: true, defaultValue: 1);
            var optionInfo = OptionInfo.Create(parameter);
            var switchSection = optionInfo.GenerateSwitchSection();
            Assert.IsNotNull(switchSection);
            string expected = """
                case "--a-value":
                    options.aValue = uint.Parse(args[i]);
                    break;
                """;
            Assert.AreEqual(expected, switchSection.NormalizeWhitespace().ToFullString());
        }

        [TestMethod]
        public void GenerateSetValueAccessTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue");
            var optionInfo = OptionInfo.Create(parameter);
            var setValueAccess = optionInfo.GenerateSetValueAccess();
            Assert.IsNotNull(setValueAccess);
            Assert.AreEqual("options._aValueValueSet", setValueAccess.ToFullString());
        }

        [TestMethod]
        public void GenerateSetValueAccess_NotRequired_Test()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue", hasDefaultValue: true, defaultValue: 42);
            var optionInfo = OptionInfo.Create(parameter);
            var setValueAccess = optionInfo.GenerateSetValueAccess();
            Assert.IsNull(setValueAccess);
        }

        [TestMethod]
        public void GenerateSpecificMissingOptionCheckTest()
        {
            var parameter = FakeParameterInfo.Create(parameterName: "aValue");
            var optionInfo = OptionInfo.Create(parameter);
            var specificMissingOptionCheck = optionInfo.GenerateSpecificMissingOptionCheck();
            Assert.IsNotNull(specificMissingOptionCheck);
            string expected = """
                if (!options._aValueValueSet)
                {
                    missing.Add("--a-value");
                }
                """;
            Assert.AreEqual(expected, specificMissingOptionCheck.NormalizeWhitespace().ToFullString());
        }
    }
}
