using MySimpleDictionaryBlazorApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MysimpleDictionaryBlazorApp.Tests.ModelTests
{
    public class MySimpleDictionaryTests
    {
        public static readonly IEnumerable<object[]> Add_ValidInputs = new[]
        {
            new object[] { "first", "First number"},
            new object[] { "second", "Second number" },
            new object[] { "third", "Third number" }
        };

        [Theory]
        [MemberData(nameof(Add_ValidInputs))]
        public void Add_DifferentInputs_ShouldBeEqual(string key, string value)
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>();

            //Act
            myDictionary.Add(key, value);

            //Assert
            Assert.Equal(value, myDictionary[key]);
        }

        [Fact]
        public void Add_OneInputToResize_ShouldBeEqual()
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>(0);

            //Act
            myDictionary.Add("novi element", "resize");

            //Assert
            Assert.Equal(3, myDictionary.Capacity);
        }

        [Theory]
        [InlineData("prvi")]
        [InlineData("drugi")]
        public void ContainsKey_ValidKey_ShouldBeTrue(string key)
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>()
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };

            //Act
            bool result = myDictionary.ContainsKey(key);

            //Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("cetvrti")]
        [InlineData("prv")]
        public void ContainsKey_InvalidKey_ShouldBeFalse(string key)
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>()
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };

            //Act
            bool result = myDictionary.ContainsKey(key);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsKey_KeyIsNull_ThrowsArgumentNullException()
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>();

            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => myDictionary.ContainsKey(null!));
        }

        [Theory]
        [InlineData("prvi element")]
        [InlineData("drugi element")]
        public void ContainsValue_ValidValue_ShouldBeTrue(string value)
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>()
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };

            //Act
            bool result = myDictionary.ContainsValue(value);

            //Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("prvi el")]
        [InlineData("drugi el")]
        public void ContainsValue_InvalidValue_ShouldBeFalse(string value)
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>()
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };

            //Act
            bool result = myDictionary.ContainsValue(value);

            //Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("prvi")]
        [InlineData("drugi")]
        public void Remove_ValidKey_ShouldBeTrue(string key)
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>()
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };

            //Act
            bool result = myDictionary.Remove(key);

            //Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("prvi", 0, 1, 2, 3)]
        [InlineData("drugi", 1, 1, 2, 3)]
        [InlineData("treci", 2, 1, 2, 3)]
        public void Remove_ValidKey_ShouldBeEqual(string key, int freeList, int freeCount, int count, int totalNumberOfEntries)
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>()
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };

            //Act
            bool result = myDictionary.Remove(key);

            //Assert
            Assert.Equal((freeList, freeCount, count, totalNumberOfEntries), (myDictionary.FreeList, myDictionary.FreeCount, myDictionary.Count, myDictionary.TotalNumberOfEntries));
        }

        [Theory]
        [InlineData("prvi", -1, 0, 3, 3, 0)]
        [InlineData("drugi", -1, 0, 3, 3, 1)]
        [InlineData("treci", -1, 0, 3, 3, 2)]
        public void Add_AddAfterRemove_ShouldBeEqual(string keyToRemove, int freeList, int freeCount, int count, int totalNumberOfEntries, int entriesIndex)
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>()
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };
            myDictionary.Remove(keyToRemove);

            //Act
            myDictionary.Add("cetvrti", "cetvrti element");

            //Assert
            Assert.Equal((freeList, freeCount, count, totalNumberOfEntries, "cetvrti"), (myDictionary.FreeList, myDictionary.FreeCount, myDictionary.Count, myDictionary.TotalNumberOfEntries, myDictionary.Entries[entriesIndex].Key));
        }

        [Fact]
        public void Remove_ReturnValue_ShouldBeEqueal()
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>()
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };
            string value;

            //Act
            bool result = myDictionary.Remove("prvi", out value);

            //Assert
            Assert.Equal("prvi element", value);
        }

        [Fact]
        public void Clear_ClearDictionary_ShouldBeEqual()
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>()
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };

            //Act
            myDictionary.Clear();

            //Assert
            Assert.Equal((-1, 0, 0, 0, 0, 11), (myDictionary.FreeList, myDictionary.FreeCount, myDictionary.LoadFactor, myDictionary.NumberOfEntries, myDictionary.TotalNumberOfEntries, myDictionary.Capacity));
        }

        [Fact]
        public void Clear_ClearAfterAddAndRemove_ShouldBeEqual()
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>()
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };
            myDictionary.Add("cetvrti", "cetvrti element");
            myDictionary.Remove("drugi");
            myDictionary.Add("peti", "peti element");

            //Act
            myDictionary.Clear();

            //Assert
            Assert.Equal((-1, 0, 0, 0, 0, 11), (myDictionary.FreeList, myDictionary.FreeCount, myDictionary.LoadFactor, myDictionary.NumberOfEntries, myDictionary.TotalNumberOfEntries, myDictionary.Capacity));
        }

        [Fact]
        public void ContainsKey_CustomComparer_ShouldBeTrue()
        {
            //Arrange
            MySimpleDictionary<string, string> myDictionary = new MySimpleDictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
            {
                {"prvi", "prvi element" },
                {"drugi", "drugi element" },
                {"treci", "treci element" }
            };

            //Act
            bool result = myDictionary.ContainsKey("pRVI");

            //Assert
            Assert.True(result);
        }
    }
}
