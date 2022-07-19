using EntityFrameworkCore.RawSQLExtensions.Extensions;
using FakeItEasy;
using NUnit.Framework;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
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

    public class NotMappedPropertyClass
    {
        public string firstProperty { get; set; }

        [NotMapped]
        public string secondProperty { get; set; }
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

            var obj = dbReader.MapObject<T>(null);

            A.CallTo(() => dbReader.GetValue(0)).MustHaveHappened();
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

            var obj = dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>());

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

            var obj = dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>());

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

            var obj = dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>());

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

            var obj = dbReader.MapObject<ComplexClassWithAdditionalProperty>(dbReader.GetSchema<ComplexClassWithAdditionalProperty>());

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

            Assert.Throws<System.ArgumentException>(() => dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>()));
        }

        [Test]
        public void MapObjectMapSkipNotMappedProperties()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn(nameof(NotMappedPropertyClass.firstProperty).ToLower(), 0),
                    new CustomDbColumn(nameof(NotMappedPropertyClass.secondProperty).ToLower(), 0),
                });

            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
            A.CallTo(() => dbReader.GetValue(0)).Returns("string");

            var obj = dbReader.MapObject<NotMappedPropertyClass>(dbReader.GetSchema<NotMappedPropertyClass>());
            Assert.AreEqual("string", obj.firstProperty);
            Assert.AreEqual(null, obj.secondProperty);
        }

        #endregion

        #region "ToListAsync, FirstOrDefaultAsync, SingleOrDefaultAsync"

        // TODO

        #endregion

        #region "Not Mapped Property"

        [Test]
        public void GetSchemaFromNotMappedClass()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn(nameof(NotMappedPropertyClass.firstProperty)),
                    new CustomDbColumn(nameof(NotMappedPropertyClass.secondProperty)),
                });
            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);

            var schema = dbReader.GetSchema<NotMappedPropertyClass>();
            Assert.IsTrue(schema.ContainsKey(nameof(NotMappedPropertyClass.firstProperty).ToLower()));
            Assert.IsFalse(schema.ContainsKey(nameof(NotMappedPropertyClass.secondProperty).ToLower()));
            Assert.AreEqual(schema.Keys.Count, 1);
        }

        #endregion

        #endregion

        [Test]
        public void  ReadValueFromTulp()
        {
            var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
            var dataColumn = new ReadOnlyCollection<DbColumn>(
                new DbColumn[] {
                    new CustomDbColumn("id",0),
                    new CustomDbColumn("index", 1),
                    new CustomDbColumn("dt", 2),
                });

            A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
            A.CallTo(() => dbReader.GetValue(0)).Returns("string");
            A.CallTo(() => dbReader.GetValue(1)).Returns(1);
            var dtx = DateTime.Now;
            A.CallTo(() => dbReader.GetValue(2)).Returns(dtx);
           var obj= dbReader.MapObject<(string id, int index, DateTime dt)>( dbReader.GetSchema< (string id, int index, DateTime dt)>());
            Assert.AreEqual("string", obj.id);
            Assert.AreEqual(1, obj.index);
            Assert.AreEqual(dtx, obj.dt);
        }
    }
}
