using Microsoft.EntityFrameworkCore;
using org.cchmc.{{cookiecutter.namespace}}.data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace org.cchmc.{{cookiecutter.namespace}}.unittests.RepositoryTests
{
    public sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Flag { get; set; }
    }

    public sealed class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Objects { get; set; }
    }

    [TestClass]
    public sealed class BaseRepositoryUnitTests
    {
        private BaseRepository<TestEntity, TestContext> _repo;
        private TestContext _context;

        [TestInitialize]
        public void Init()
        {
            Cleanup();
            var options = new DbContextOptionsBuilder<TestContext>()
                                .UseInMemoryDatabase(databaseName: "Fake db")
                                .Options;
            _context = new TestContext(options);
            _repo = new BaseRepository<TestEntity, TestContext>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Database.EnsureDeleted();
        }

        public void LoadData()
        {
            _context.Objects.AddRange(new List<TestEntity>()
            {
                new()
                {
                    Id = 1,
                    Name = "test",
                    Flag = false
                },
                new()
                {
                    Id = 2,
                    Name = "something",
                    Flag = true
                },
                new()
                {
                    Id = 3,
                    Name = "with space",
                    Flag = false
                }
            });
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task GetAsync_FindsResult()
        {
            LoadData();
            var result = await _repo.GetAsync(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("test", result.Name);
            Assert.IsFalse(result.Flag);
        }

        [TestMethod]
        public async Task GetAsync_NoResult()
        {
            LoadData();

            var result = await _repo.GetAsync(4);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetByConditionAsync_FindsResult()
        {
            LoadData();

            var result = await _repo.GetByConditionAsync(x => x.Flag);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Id);
        }

        [TestMethod]
        public async Task GetByConditionAsync_NoResult()
        {
            LoadData();

            var result = await _repo.GetByConditionAsync(x => x.Name == "no such name");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAllAsync_FindsResults()
        {
            LoadData();

            var result = await _repo.GetAllAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetAllAsync_NoResults()
        {
            var result = await _repo.GetAllAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task AddAsync_Success()
        {
            var x = new TestEntity()
            {
                Flag = true,
                Name = "rmaergtadmrnknygasdf"
            };

            var result = await _repo.AddAsync(x);

            Assert.IsNotNull(result);
            Assert.AreEqual(x.Name, result.Name);
            Assert.IsTrue(result.Flag);
            Assert.AreEqual(1, _context.Objects.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_Success()
        {
            LoadData();

            var x = await _repo.GetAsync(2);
            var newName = "jsrt8brgtujdsfhgkj";
            x.Flag = false;
            x.Name = newName;

            var result = await _repo.UpdateAsync(x);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Flag);
            Assert.AreEqual(newName, result.Name);
            Assert.AreEqual(3, _context.Objects.Count());
        }

        [TestMethod]
        public async Task DeleteAsync_Success()
        {
            LoadData();

            await _repo.DeleteAsync(2);

            Assert.AreEqual(2, _context.Objects.Count());
            Assert.AreEqual(0, _context.Objects.Count(x => x.Id == 2));
        }

        [TestMethod]
        public async Task DeleteAsync_NoSuchId()
        {
            LoadData();

            await _repo.DeleteAsync(4);

            Assert.AreEqual(3, _context.Objects.Count());
        }

        [TestMethod]
        public async Task ExistsAsync_Success()
        {
            LoadData();

            var result = await _repo.ExistsAsync(x => x.Flag);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task GetQueryable()
        {
            LoadData();

            var result = await _repo.GetQueryable().Where(p => p.Id % 2 == 1).ToListAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }
    }
}
