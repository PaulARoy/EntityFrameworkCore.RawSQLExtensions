using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using EntityFrameworkCore.RawSQLExtensions.Extensions;
using EntityFrameworkCore.RawSQLExtensions.Options;
using FakeItEasy;
using NUnit.Framework;

namespace EntityFrameworkCore.RawSQLExtensions.Tests.Extensions;

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
#pragma warning disable IDE1006 // Naming Styles
    public string lowercase { get; set; }
    public int mIxEdCaSe { get; set; }
    public bool UPPERCASE { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}

public class ComplexClassWithAdditionalProperty : ComplexClass
{
    public string AdditionalProperty { get; set; }
}

public class NotMappedPropertyClass
{
    public string FirstProperty { get; set; }

    [NotMapped]
    public string SecondProperty { get; set; }
}

public class GuidParsingClass
{
    public Guid GuidProperty { get; set; }
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
        Assert.That(schema.Keys.Count, Is.Zero);
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
        Assert.That(instance, Is.EqualTo(obj));
    }

    [Test]
    public void MapObjectWithGuidProperty()
    {
        RawSQLExtensionsOptions.AllowStringGuidConversion = true;

        var dbReader = A.Fake<DbDataReader>();
        var guid = Guid.NewGuid();
        A.CallTo(() => dbReader.GetValue(0)).Returns(guid.ToString());

        var obj = dbReader.MapObject<Guid>(null);

        A.CallTo(() => dbReader.GetValue(0)).MustHaveHappened();
        Assert.That(guid, Is.EqualTo(obj));
    }

    [Test]
    public void MapObjectWithGuidPropertyWithoutOptionThrows()
    {
        RawSQLExtensionsOptions.AllowStringGuidConversion = false;

        var dbReader = A.Fake<DbDataReader>();
        var guid = Guid.NewGuid();
        A.CallTo(() => dbReader.GetValue(0)).Returns(guid.ToString());

        Assert.Throws<InvalidCastException>(() => dbReader.MapObject<Guid>(null));
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
            [new CustomDbColumn("lowercase"), new CustomDbColumn("mixedcase"), new CustomDbColumn("uppercase"),]
        );
        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);

        var schema = dbReader.GetSchema<ComplexClass>();
        Assert.That(schema.ContainsKey("lowercase"), Is.True);
        Assert.That(schema.ContainsKey("mixedcase"), Is.True);
        Assert.That(schema.ContainsKey("uppercase"), Is.True);
        Assert.That(schema.Keys.Count, Is.EqualTo(3));
    }

    [Test]
    public void GetSchemaFromComplexClassWithAdditionalColumnWithoutProperty()
    {
        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [
                new CustomDbColumn("lowercase"),
                new CustomDbColumn("mixedcase"),
                new CustomDbColumn("uppercase"),
                new CustomDbColumn("additionalcolumn"),
            ]
        );
        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);

        var schema = dbReader.GetSchema<ComplexClass>();
        Assert.That(schema.ContainsKey("lowercase"), Is.True);
        Assert.That(schema.ContainsKey("mixedcase"), Is.True);
        Assert.That(schema.ContainsKey("uppercase"), Is.True);

        Assert.That(schema.ContainsKey("additionalcolumn"), Is.False);
        Assert.That(schema.Keys.Count, Is.EqualTo(3));
    }

    [Test]
    public void GetSchemaFromComplexClassWithAdditionalPropertyWithoutColumn()
    {
        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [new CustomDbColumn("lowercase"), new CustomDbColumn("mixedcase"), new CustomDbColumn("uppercase"),]
        );
        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);

        var schema = dbReader.GetSchema<ComplexClassWithAdditionalProperty>();
        Assert.That(schema.ContainsKey("lowercase"), Is.True);
        Assert.That(schema.ContainsKey("mixedcase"), Is.True);
        Assert.That(schema.ContainsKey("uppercase"), Is.True);

        Assert.That(schema.ContainsKey("aditionalproperty"), Is.False);
        Assert.That(schema.Keys.Count, Is.EqualTo(3));
    }

    #endregion

    #region "MapObject"

    [Test]
    public void MapObjectMapComplexTypeWithCorrectOrder()
    {
        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [
                new CustomDbColumn("lowercase", 0),
                new CustomDbColumn("mixedcase", 1),
                new CustomDbColumn("uppercase", 2),
            ]
        );

        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
        A.CallTo(() => dbReader.GetValue(0)).Returns("string");
        A.CallTo(() => dbReader.GetValue(1)).Returns(12);
        A.CallTo(() => dbReader.GetValue(2)).Returns(true);

        var obj = dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>());

        Assert.That(obj.lowercase, Is.EqualTo("string"));
        Assert.That(obj.mIxEdCaSe, Is.EqualTo(12));
        Assert.That(obj.UPPERCASE, Is.True);
    }

    [Test]
    public void MapObjectMapComplexTypeWithInCorrectOrder()
    {
        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [
                new CustomDbColumn("lowercase", 2),
                new CustomDbColumn("mixedcase", 1),
                new CustomDbColumn("uppercase", 0),
            ]
        );

        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
        A.CallTo(() => dbReader.GetValue(2)).Returns("string");
        A.CallTo(() => dbReader.GetValue(1)).Returns(12);
        A.CallTo(() => dbReader.GetValue(0)).Returns(true);

        var obj = dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>());

        Assert.That(obj.lowercase, Is.EqualTo("string"));
        Assert.That(obj.mIxEdCaSe, Is.EqualTo(12));
        Assert.That(obj.UPPERCASE, Is.True);
    }

    [Test]
    public void MapObjectMapComplexTypeWithAdditionalColumn()
    {
        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [
                new CustomDbColumn("lowercase", 0),
                new CustomDbColumn("mixedcase", 1),
                new CustomDbColumn("uppercase", 2),
                new CustomDbColumn("additionalfield", 3),
            ]
        );

        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
        A.CallTo(() => dbReader.GetValue(0)).Returns("string");
        A.CallTo(() => dbReader.GetValue(1)).Returns(12);
        A.CallTo(() => dbReader.GetValue(2)).Returns(true);
        A.CallTo(() => dbReader.GetValue(3)).Returns("additional");

        var obj = dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>());

        Assert.That(obj.lowercase, Is.EqualTo("string"));
        Assert.That(obj.mIxEdCaSe, Is.EqualTo(12));
        Assert.That(obj.UPPERCASE, Is.True);
    }

    [Test]
    public void MapObjectMapComplexTypeWithAdditionalProperty()
    {
        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [
                new CustomDbColumn("lowercase", 0),
                new CustomDbColumn("mixedcase", 1),
                new CustomDbColumn("uppercase", 2),
            ]
        );

        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
        A.CallTo(() => dbReader.GetValue(0)).Returns("string");
        A.CallTo(() => dbReader.GetValue(1)).Returns(12);
        A.CallTo(() => dbReader.GetValue(2)).Returns(true);

        var obj = dbReader.MapObject<ComplexClassWithAdditionalProperty>(
            dbReader.GetSchema<ComplexClassWithAdditionalProperty>()
        );

        Assert.That(obj.lowercase, Is.EqualTo("string"));
        Assert.That(obj.mIxEdCaSe, Is.EqualTo(12));
        Assert.That(obj.UPPERCASE, Is.True);
        Assert.That(obj.AdditionalProperty, Is.Null);
    }

    [Test]
    public void MapObjectMapComplexTypeThrowsWithTypeMismatch()
    {
        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [
                new CustomDbColumn("lowercase", 0),
                new CustomDbColumn("mixedcase", 1),
                new CustomDbColumn("uppercase", 2),
            ]
        );

        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
        A.CallTo(() => dbReader.GetValue(0)).Returns("string");
        A.CallTo(() => dbReader.GetValue(1)).Returns("12");
        A.CallTo(() => dbReader.GetValue(2)).Returns(true);

        Assert.Throws<ArgumentException>(() => dbReader.MapObject<ComplexClass>(dbReader.GetSchema<ComplexClass>()));
    }

    [Test]
    public void MapObjectMapSkipNotMappedProperties()
    {
        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [
                new CustomDbColumn(nameof(NotMappedPropertyClass.FirstProperty).ToLower(), 0),
                new CustomDbColumn(nameof(NotMappedPropertyClass.SecondProperty).ToLower(), 0),
            ]
        );

        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
        A.CallTo(() => dbReader.GetValue(0)).Returns("string");

        var obj = dbReader.MapObject<NotMappedPropertyClass>(dbReader.GetSchema<NotMappedPropertyClass>());
        Assert.That(obj.FirstProperty, Is.EqualTo("string"));
        Assert.That(obj.SecondProperty, Is.Null);
    }

    [Test]
    public void MapClassWithGuidProperty()
    {
        RawSQLExtensionsOptions.AllowStringGuidConversion = true;

        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var guid = Guid.NewGuid();
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [new CustomDbColumn(nameof(GuidParsingClass.GuidProperty).ToLower(), 0),]
        );

        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
        A.CallTo(() => dbReader.GetValue(0)).Returns(guid.ToString());

        var obj = dbReader.MapObject<GuidParsingClass>(dbReader.GetSchema<GuidParsingClass>());

        A.CallTo(() => dbReader.GetValue(0)).MustHaveHappened();
        Assert.That(obj.GuidProperty, Is.EqualTo(guid));
    }

    [Test]
    public void MapClassWithGuidPropertyWithoutOptionThrows()
    {
        RawSQLExtensionsOptions.AllowStringGuidConversion = false;

        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var guid = Guid.NewGuid();
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [new CustomDbColumn(nameof(GuidParsingClass.GuidProperty).ToLower(), 0),]
        );

        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
        A.CallTo(() => dbReader.GetValue(0)).Returns(guid.ToString());

        Assert.Throws<ArgumentException>(
            () => dbReader.MapObject<GuidParsingClass>(dbReader.GetSchema<GuidParsingClass>())
        );
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
            [
                new CustomDbColumn(nameof(NotMappedPropertyClass.FirstProperty)),
                new CustomDbColumn(nameof(NotMappedPropertyClass.SecondProperty)),
            ]
        );
        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);

        var schema = dbReader.GetSchema<NotMappedPropertyClass>();
        Assert.That(schema.ContainsKey(nameof(NotMappedPropertyClass.FirstProperty).ToLower()), Is.True);
        Assert.That(schema.ContainsKey(nameof(NotMappedPropertyClass.SecondProperty).ToLower()), Is.False);
        Assert.That(schema.Keys.Count, Is.EqualTo(1));
    }

    #endregion

    #endregion

    [Test]
    public void ReadValueFromTulp()
    {
        var dbReader = A.Fake<DbDataReader>(opts => opts.Implements<IDbColumnSchemaGenerator>());
        var dataColumn = new ReadOnlyCollection<DbColumn>(
            [new CustomDbColumn("id", 0), new CustomDbColumn("index", 1), new CustomDbColumn("dt", 2),]
        );

        A.CallTo(() => ((IDbColumnSchemaGenerator)dbReader).GetColumnSchema()).Returns(dataColumn);
        A.CallTo(() => dbReader.GetValue(0)).Returns("string");
        A.CallTo(() => dbReader.GetValue(1)).Returns(1);
        var dtx = DateTime.Now;
        A.CallTo(() => dbReader.GetValue(2)).Returns(dtx);
        var (id, index, dt) = dbReader.MapObject<(string id, int index, DateTime dt)>(
            dbReader.GetSchema<(string id, int index, DateTime dt)>()
        );
        Assert.That(id, Is.EqualTo("string"));
        Assert.That(index, Is.EqualTo(1));
        Assert.That(dt, Is.EqualTo(dtx));
    }
}
