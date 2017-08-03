using Coalesce.Domain;
using Coalesce.Web.Api;
using Coalesce.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Models;
using Xunit;

namespace Coalesce.Web.Tests
{
    [Collection("Database collection")]
    public class PersonControllerTests : IClassFixture<DatabaseFixtureLocalDb>
    {
        private PersonController _pc;

        public PersonControllerTests(DatabaseFixtureLocalDb dbFixture)
        {
            DbFixture = dbFixture;
            _pc = new PersonController();
            _pc.Db = DbFixture.Db;

        }
        private DatabaseFixtureLocalDb DbFixture { get; set; }

        //[Fact]
        //public async void ListGeneral()
        //{
        //    Assert.NotNull(_pc.DataSource);

        //    var result = await _pc.List();
        //    Assert.Equal(25, result.List.Count());
        //    Assert.Equal(100, result.TotalCount);
        //    Assert.Equal(4, result.PageCount);
        //    var person = result.List.First();
        //    Assert.Equal(person.FirstName, "Joseph");

        //    var first = await _pc.Get(result.List.First().PersonId.ToString());
        //    Assert.Equal(1, first.CompanyId);

        //}
        //[Fact]
        //public async void ListPaging()
        //{
        //    var result = await _pc.List(pageSize: 5, page: 2);
        //    Assert.Equal(result.List.Count(), 5);
        //    Assert.Equal(result.Page, 2);
        //    Assert.Equal(result.TotalCount, 100);
        //    Assert.Equal(result.PageCount, 20);
        //}

        //[Fact]
        //public async void ListOrderByFirst()
        //{
        //    var result = await _pc.List(orderBy: "FirstName");
        //    var person = result.List.First();
        //    Assert.Equal("Aaron", person.FirstName);
        //    Assert.Equal(25, result.List.Count());
        //}

        //[Fact]
        //public async void ListOrderByLast()
        //{
        //    var result = await _pc.List(orderBy: "LastName");
        //    var person = result.List.First();
        //    Assert.Equal("Arianna", person.FirstName);
        //    Assert.Equal(25, result.List.Count());
        //}
        //[Fact]
        //public async void ListOrderByLastAndFirst()
        //{
        //    var result = await _pc.List(orderBy: "LastName, FirstName Desc, PersonId Asc");
        //    var person = result.List.First();
        //    Assert.Equal("Leslie", person.FirstName);
        //    Assert.Equal(25, result.List.Count());
        //}


        [Fact]
        public async void ListIncludesDefault()
        {
            var result = await _pc.CustomList(personId: "1");
            var person = result.List.Cast<PersonDtoGen>().First();
            Assert.NotNull(person.Company);
            // GenFu's company names change.
            Assert.Equal("La Pocatière", person.Company.City);
            Assert.Equal(1, result.List.Count());
        }

        //[Fact]
        //public async void ListIncludesNone()
        //{
        //    using (var db = DbFixture.FreshDb())
        //    {
        //        _pc.Db = db;
        //        var result = await _pc.List(includes: "none");
        //        var person = result.List.First();
        //        Assert.Null(person.Company);
        //        Assert.Equal(25, result.List.Count());
        //        _pc.Db = DbFixture.Db;
        //    }
        //}

        //[Fact]
        //public async void ListByFirstName()
        //{
        //    var result = await _pc.List(firstName: "Austin");
        //    Assert.Equal(2, result.List.Count());
        //}

        //[Fact]
        //public async void ListById()
        //{
        //    var result = await _pc.List(personId: "1");
        //    Assert.Equal(1, result.List.Count());
        //    var person = result.List.First();
        //    Assert.Equal("Joseph", person.FirstName);
        //}

        [Fact]
        public async void Count()
        {
            var result = await _pc.Count();
            Assert.Equal(100, result);
        }

        [Fact]
        public async void CountByFirstName()
        {
            var result = await _pc.Count(firstName: "Austin");
            Assert.Equal(2, result);
        }

        [Fact]
        public async void GetById()
        {
            var person = await _pc.Get(1.ToString());
            Assert.Equal("Joseph", person.FirstName);
        }

        [Fact]
        public async void Update()
        {
            // Get the item
            var person = await _pc.Get(1.ToString());
            Assert.Equal("Joseph", person.FirstName);
            // Change the item and save it.
            person.FirstName = "Sweet";
            var result = _pc.Save(person);
            // Make sure it saved.
            Assert.True(result.Result.WasSuccessful);
            Assert.Equal("Sweet", result.Result.Object.FirstName);
            // Get the new item.
            person = await _pc.Get(1.ToString());
            Assert.Equal("Sweet", person.FirstName);
            // Set it back and save it.
            person.FirstName = "Joseph";
            result = _pc.Save(person);
            // Make sure it saved.
            Assert.True(result.Result.WasSuccessful);
            Assert.Equal("Joseph", result.Result.Object.FirstName);
            // Get it again and make sure it stayed saved.
            person = await _pc.Get(1.ToString());
            Assert.Equal("Joseph", person.FirstName);
        }

        [Fact]
        public async void ListByWhere()
        {
            var result = await _pc.List(null, null, null, null, null, "personId = 1 || personid = 2", null, null, null, null, null, null, null, null, null, null, null);
            Assert.Equal(2, result.List.Count());
            var person = result.List.First();
            Assert.Equal("Joseph", person.FirstName);
        }


        [Fact]
        public async void ListSearch()
        {
            var result = await _pc.List(null, "lastName", null, null, null, null, null, "a", null, null, null, null, null, null, null, null, null);
            Assert.Equal(16, result.List.Count());
            var person = result.List.First();
            Assert.Equal("Arianna", person.FirstName);
        }

        [Fact]
        public void PropertyValues()
        {
            var result = _pc.PropertyValues("FirstName");
            Assert.Equal(20, result.Count());
            result = _pc.PropertyValues("FirstName", 1, "a");
            Assert.Equal(13, result.Count());

        }


        [Fact]
        public void StaticFunctionAdd()
        {
            var result1 = _pc.Add(1, 2);
            Assert.Equal(3, result1.Object);
        }

        [Fact]
        public void StaticFunctionGetUser()
        {
            var result2 = _pc.GetUserPublic();
            Assert.Equal("Unknown", result2.Object);
        }


       [Fact]
        public async void InstanceFunction()
        {
            var result = _pc.Rename(1, "-test");
            Assert.Equal("Joseph-test", result.Object.FirstName);
            // Get the new item.
            var person = await _pc.Get(1.ToString());
            Assert.Equal("Joseph-test", person.FirstName);
            // Set it back and save it.
            person.FirstName = "Joseph";
            var result2 = _pc.Save(person);
            // Make sure it saved.
            Assert.True(result2.Result.WasSuccessful);
            Assert.Equal("Joseph", result2.Result.Object.FirstName);
        }


        [Fact]
        public void StaticWithDb()
        {
            var result = _pc.NamesStartingWithPublic("a");
            Assert.Equal(16, ((List<string>)result.Object).Count());
        }


       [Fact]
        public void Collection()
        {
            var result = _pc.BorCPeople();
            Assert.Equal(14, result.Object.Count());
        }



        [Fact]
        public async void ListOfBorC()
        {
            var thing = new ListResult
            {
                List = new List<PersonDtoGen> { new PersonDtoGen { FirstName = "bob" } },
                Message = "test",
                Page = 1,
                PageCount = 1,
                PageSize = 1,
                TotalCount = 1,
                WasSuccessful = false,
            };

            var generic = new GenericListResult<Person, PersonDtoGen>(thing);

            var result = await _pc.List(null, null, null, null, null, null, "BorCPeople", null, "1", null, null, null, null, null, null, null, null);
            Assert.Equal(0, result.List.Count());
        }

        [Fact]
        public async void CountOfBorC()
        {
            var result = await _pc.Count(null, "BorCPeople");
            Assert.Equal(14, result);
        }
    }
}
