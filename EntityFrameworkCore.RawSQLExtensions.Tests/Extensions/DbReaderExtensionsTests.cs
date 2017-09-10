using EntityFrameworkCore.RawSQLExtensions.Extensions;
using FakeItEasy;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Reflection;

namespace EntityFrameworkCore.RawSQLExtensions.Tests.Extensions
{
    public class CustomDbColumn : DbColumn
    {
        public CustomDbColumn(string columnName, int? columnOrdinal = null)
        {
            this.ColumnName = columnName;
            this.ColumnOrdinal = columnOrdinal;
        }
    }

    public class ComplexClass
    {
        public string lowercase { get; set; }
        public int mIxEdCaSe { get; set; }
        public bool UPPERCASE { get; set; }
    }

    public class ComplexClassWithAdditionalProperty : ComplexClass
    {
        public string AdditionalProperty { get; set; }
    }

    [TestFixture]
    public class DbReaderExtensionsTests
    {
        #region "Simple Types"

        #region "Schema"

        [Test]
        [TestCase((bool)true)]
        [TestCase((sbyte)1)]
        [TestCase((byte)2)]
        [TestCase((short)3)]
        [TestCase((ushort)4)]
        [TestCase((int)5)]
        [TestCase((uint)6)]
        [TestCase((long)7)]
        [TestCase((ulong)8)]
        [TestCase((char)'9')]
        [TestCase((float)10)]
        [TestCase((string)"11")]
        public void GetSchemaFromSimpleTypeDoesntThrow<T>(T instance)
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var schema = dbReader.GetSchema<T>();
            Assert.AreEqual(schema.Keys.Count, 0);
        }

        #endregion

        #region "MapObject"

        [Test]
        [TestCase((bool)true)]
        [TestCase((sbyte)1)]
        [TestCase((byte)2)]
        [TestCase((short)3)]
        [TestCase((ushort)4)]
        [TestCase((int)5)]
        [TestCase((uint)6)]
        [TestCase((long)7)]
        [TestCase((ulong)8)]
        [TestCase((char)'9')]
        [TestCase((float)10)]
        [TestCase((string)"11")]
        public void MapObjectCallGetValueOnSimpleType<T>(T instance)
        {
            var dbReader = A.Fake<DbDataReader>();
            A.CallTo(() => dbReader.GetValue(0)).Returns(instance);

            var obj = dbReader.MapObject<T>(null, null);

            A.CallTo(() => dbReader.GetValue(0)).MustHaveHappened(Repeated.Exactly.Once);
            Assert.AreEqual(instance, obj);
        }

        #endregion

        #region "ToListAsync, FirstOrDefaultAsync, SingleOrDefaultAsync"

        // TODO

        #endregion

        #endregion


        #region "Complex Type"

        #region "Schema"

        [Test]
        public void GetSchemaFromComplexClass()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn("lowercase"),
                    new CustomDbColumn("mixedcase"),
                    new CustomDbColumn("uppercase"),
                });
            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);

            var schema = dbReader.GetSchema<ComplexClass>();
            Assert.IsTrue(schema.ContainsKey("lowercase"));
            Assert.IsTrue(schema.ContainsKey("mixedcase"));
            Assert.IsTrue(schema.ContainsKey("uppercase"));
            Assert.AreEqual(schema.Keys.Count, 3);
        }

        [Test]
        public void GetSchemaFromComplexClassWithAdditionalColumnWithoutProperty()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn("lowercase"),
                    new CustomDbColumn("mixedcase"),
                    new CustomDbColumn("uppercase"),
                    new CustomDbColumn("additionalcolumn"),
                });
            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);

            var schema = dbReader.GetSchema<ComplexClass>();
            Assert.IsTrue(schema.ContainsKey("lowercase"));
            Assert.IsTrue(schema.ContainsKey("mixedcase"));
            Assert.IsTrue(schema.ContainsKey("uppercase"));

            Assert.IsFalse(schema.ContainsKey("additionalcolumn"));
            Assert.AreEqual(schema.Keys.Count, 3);
        }

        [Test]
        public void GetSchemaFromComplexClassWithAdditionalPropertyWithoutColumn()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn("lowercase"),
                    new CustomDbColumn("mixedcase"),
                    new CustomDbColumn("uppercase"),
                });
            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);

            var schema = dbReader.GetSchema<ComplexClassWithAdditionalProperty>();
            Assert.IsTrue(schema.ContainsKey("lowercase"));
            Assert.IsTrue(schema.ContainsKey("mixedcase"));
            Assert.IsTrue(schema.ContainsKey("uppercase"));

            Assert.IsFalse(schema.ContainsKey("aditionalproperty"));
            Assert.AreEqual(schema.Keys.Count, 3);
        }

        #endregion

        #region "MapObject"

        [Test]
        public void MapObjectMapComplexTypeWithCorrectOrder()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn("lowercase", 0),
                    new CustomDbColumn("mixedcase", 1),
                    new CustomDbColumn("uppercase", 2),
                });

            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
            A.CallTo(() => dbReader.GetValue(0)).Returns("string");
            A.CallTo(() => dbReader.GetValue(1)).Returns(12);
            A.CallTo(() => dbReader.GetValue(2)).Returns(true);

            var obj = dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>(), typeof(ComplexClass).GetRuntimeProperties());

            Assert.AreEqual("string", obj.lowercase);
            Assert.AreEqual(12, obj.mIxEdCaSe);
            Assert.AreEqual(true, obj.UPPERCASE);
        }

        [Test]
        public void MapObjectMapComplexTypeWithInCorrectOrder()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn("lowercase", 2),
                    new CustomDbColumn("mixedcase", 1),
                    new CustomDbColumn("uppercase", 0),
                });

            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
            A.CallTo(() => dbReader.GetValue(2)).Returns("string");
            A.CallTo(() => dbReader.GetValue(1)).Returns(12);
            A.CallTo(() => dbReader.GetValue(0)).Returns(true);

            var obj = dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>(), typeof(ComplexClass).GetRuntimeProperties());

            Assert.AreEqual("string", obj.lowercase);
            Assert.AreEqual(12, obj.mIxEdCaSe);
            Assert.AreEqual(true, obj.UPPERCASE);
        }

        [Test]
        public void MapObjectMapComplexTypeWithAdditionalColumn()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn("lowercase", 0),
                    new CustomDbColumn("mixedcase", 1),
                    new CustomDbColumn("uppercase", 2),
                    new CustomDbColumn("additionalfield", 3),
                });

            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
            A.CallTo(() => dbReader.GetValue(0)).Returns("string");
            A.CallTo(() => dbReader.GetValue(1)).Returns(12);
            A.CallTo(() => dbReader.GetValue(2)).Returns(true);
            A.CallTo(() => dbReader.GetValue(3)).Returns("additional");

            var obj = dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>(), typeof(ComplexClass).GetRuntimeProperties());

            Assert.AreEqual("string", obj.lowercase);
            Assert.AreEqual(12, obj.mIxEdCaSe);
            Assert.AreEqual(true, obj.UPPERCASE);
        }

        [Test]
        public void MapObjectMapComplexTypeWithAdditionalProperty()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn("lowercase", 0),
                    new CustomDbColumn("mixedcase", 1),
                    new CustomDbColumn("uppercase", 2),
                });

            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
            A.CallTo(() => dbReader.GetValue(0)).Returns("string");
            A.CallTo(() => dbReader.GetValue(1)).Returns(12);
            A.CallTo(() => dbReader.GetValue(2)).Returns(true);

            var obj = dbReader.MapObject<ComplexClassWithAdditionalProperty>(dbReader.GetSchema<ComplexClassWithAdditionalProperty>(),
                                                                             typeof(ComplexClassWithAdditionalProperty).GetRuntimeProperties());

            Assert.AreEqual("string", obj.lowercase);
            Assert.AreEqual(12, obj.mIxEdCaSe);
            Assert.AreEqual(true, obj.UPPERCASE);
            Assert.AreEqual(null, obj.AdditionalProperty);
        }

        [Test]
        public void MapObjectMapComplexTypeThrowsWithTypeMismatch()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn("lowercase", 0),
                    new CustomDbColumn("mixedcase", 1),
                    new CustomDbColumn("uppercase", 2),
                });

            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
            A.CallTo(() => dbReader.GetValue(0)).Returns("string");
            A.CallTo(() => dbReader.GetValue(1)).Returns("12");
            A.CallTo(() => dbReader.GetValue(2)).Returns(true);

            Assert.Throws<System.ArgumentException>(() => dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>(), 
                                                                                           typeof(ComplexClass).GetRuntimeProperties()));
        }

        #endregion

        #region "ToListAsync, FirstOrDefaultAsync, SingleOrDefaultAsync"

        // TODO

        #endregion

        #endregion

    }
}
