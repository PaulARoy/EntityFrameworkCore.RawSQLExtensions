using NUnit.Framework;
using EntityFrameworkCore.RawSQLExtensions.Extensions;
using System;

namespace EntityFrameworkCore.RawSQLExtensions.Tests.Extensions
{
    public class SimpleClass
    {
    }

    [TestFixture]
    public class TypeExtensionsTests
    {
        [Test]
        [TestCase(typeof(sbyte), true)]
        [TestCase(typeof(byte), true)]
        [TestCase(typeof(short), true)]
        [TestCase(typeof(ushort), true)]
        [TestCase(typeof(int), true)]
        [TestCase(typeof(uint), true)]
        [TestCase(typeof(long), true)]
        [TestCase(typeof(ulong), true)]
        [TestCase(typeof(char), true)]
        [TestCase(typeof(float), true)]
        [TestCase(typeof(bool), true)]
        [TestCase(typeof(string), true)]
        [TestCase(typeof(DateTime), true)]
        [TestCase(typeof(DateTimeOffset), true)]
        [TestCase(typeof(Guid), true)]
        [TestCase(typeof(byte[]), true)]
        [TestCase(typeof(char[]), true)]
        [TestCase(typeof(TimeSpan), true)]
        [TestCase(typeof(sbyte?), true)]
        [TestCase(typeof(byte?), true)]
        [TestCase(typeof(short?), true)]
        [TestCase(typeof(ushort?), true)]
        [TestCase(typeof(int?), true)]
        [TestCase(typeof(uint?), true)]
        [TestCase(typeof(long?), true)]
        [TestCase(typeof(ulong?), true)]
        [TestCase(typeof(char?), true)]
        [TestCase(typeof(float?), true)]
        [TestCase(typeof(bool?), true)]
        [TestCase(typeof(DateTime?), true)]
        [TestCase(typeof(DateTimeOffset?), true)]
        [TestCase(typeof(TimeSpan?), true)]
        [TestCase(typeof(Guid?), true)]
        [TestCase(typeof(SimpleClass), false)]
        [TestCase(typeof(object), false)]
        public void ExpectedSimpleType(Type type, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, type.IsSqlSimpleType());
        }
    }
}
