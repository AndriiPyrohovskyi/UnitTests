using CryptoDataDLL;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Windows.Forms;
namespace UnitTests
{
    public class CryptoDataParserTests
    {
        private ICryptoDataParser _parser;
        private CryptoData _cryptoData;
        private DataGridView _dataGridView;

        public CryptoDataParserTests()
        {
            _parser = new CryptoDataParser();
            _cryptoData = new CryptoData(_parser);
            _dataGridView = new DataGridView();
        }

        // Група 1: Тести для ParseInt
        [Theory]
        [InlineData("96 dollarov baksov", 0)]
        [InlineData("42", 42)]
        [InlineData(null, 0)]
        [InlineData("-100", -100)]
        [InlineData("42.6", 0)]
        public void ParseInt_ShouldReturnCorrectValue(string value, int expected)
        {
            int result = _parser.ParseInt(value);
            Assert.Equal(expected, result);
        }
        [Theory]
        [InlineData("123,45", 123.45)]
        [InlineData("abc", 0.0)]
        [InlineData(null, 0.0)]
        [InlineData("-100,5", -100.5)]
        [InlineData("1e3", 1000.0)]
        public void ParseDouble_ShouldReturnCorrectValue(string value, double expected)
        {
            double result = _parser.ParseDouble(value);
            Assert.Equal(expected, result);
        }
        public class IsMatchTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new string?[] { "BTC", "Bitcoin", "90000", "10", "1000000", "300000", null, null, null, null, null, null, null, null, null, null, null },
                    new int[] { 0, 1, 2, 3, 4, 5 },
                    new int[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 },
                    true
                };
                yield return new object[]
                {
                    new string?[] { "BTC", "Bitcoin", "90000", "10", "1000000", "300000", "135", "135", "124", null, null, null, null, null, null, null, null },
                    new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                    new int[] { 9, 10, 11, 12, 13, 14, 15, 16 },
                    true
                };
                yield return new object[]
                {
                    new string?[] { null, "Bitcoin", "90000", "10", "1000000", "300000", "135", "135", "124", null, null, null, null, null, null, null, null },
                    new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                    new int[] { 9, 10, 11, 12, 13, 14, 15, 16 },
                    false
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(IsMatchTestData))]
        public void IsMatch_ShouldReturnCorrectResult(string?[] values, int[] nonEmpty, int[] empty, bool expected)
        {
            bool result = _parser.IsMatch(values, nonEmpty, empty);
            Assert.Equal(expected, result);
        }
        public static IEnumerable<object[]> GetDetermineCryptoTypeTestData()
        {
            var row1 = CreateDataGridViewRow(new object?[] { "BTC", "Bitcoin", "90000", "10", "1000000", "300000", null, null, null, null, null, null, null, null, null, null, null });
            var values1 = row1.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString()).ToArray();

            var row2 = CreateDataGridViewRow(new object?[] { "USDT", "Tether", "1", "0", "5000000", "5000000", "Stable", "USD", "USD", null, null, null, null, null, null, null, null });
            var values2 = row2.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString()).ToArray();

            yield return new object[] { values1, row1, typeof(CryptoCurrency) };
            yield return new object[] { values2, row2, typeof(StableCoin) };
        }

        [Theory]
        [MemberData(nameof(GetDetermineCryptoTypeTestData))]
        public void DetermineCryptoType_ShouldReturnCorrectType(string[] values, DataGridViewRow row, Type expectedType)
        {
            var result = _parser.DetermineCryptoType(values, row);
            Assert.IsType(expectedType, result);
        }
        [Fact]
        public void Add_ShouldAddCryptoCurrency()
        {
            var crypto = new CryptoCurrency("ETH", "Ethereum", 3000, 5, 500000, 150000);
            _cryptoData.add(crypto);
            Assert.Contains(crypto, _cryptoData.ListOfCrypto);
            Assert.Single(_cryptoData.ListOfCrypto);
            Assert.Same(crypto, _cryptoData.ListOfCrypto[0]);
        }
        [Fact]
        public void Add_ShouldRaiseCryptoAddedEvent()
        {
            var crypto = new CryptoCurrency("ETH", "Ethereum", 3000, 5, 500000, 150000);
            CryptoCurrency addedCrypto = null;

            _cryptoData.CryptoAdded += (c) => addedCrypto = c;

            _cryptoData.add(crypto);

            Assert.NotNull(addedCrypto);
            Assert.Equal(crypto, addedCrypto);
        }
        [Fact]
        public void RefreshDataGrid_ShouldClearAndPopulateDataGrid()
        {
            var crypto = new CryptoCurrency("BTC", "Bitcoin", 90000, 10, 1000000, 300000);
            _cryptoData.add(crypto);
            _cryptoData._dataGrid = new DataGridView();
            _cryptoData._dataGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn(),
                new DataGridViewTextBoxColumn(),
                new DataGridViewTextBoxColumn(),
                new DataGridViewTextBoxColumn(),
                new DataGridViewTextBoxColumn(),
                new DataGridViewTextBoxColumn(),
            });
            _cryptoData.RefreshDataGrid();
            Assert.Equal(1, _cryptoData._dataGrid.Rows.Count-1);
            Assert.Equal("BTC", _cryptoData._dataGrid.Rows[0].Cells[0].Value);
        }
        [Fact]
        public void AllocateList_ShouldPopulateListOfCrypto()
        {
            _cryptoData._dataGrid = new DataGridView();
            _cryptoData._dataGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn(), 
                new DataGridViewTextBoxColumn(), 
                new DataGridViewTextBoxColumn(), 
                new DataGridViewTextBoxColumn(), 
                new DataGridViewTextBoxColumn(), 
                new DataGridViewTextBoxColumn(), 
            });
            CryptoCurrency BTC = new CryptoCurrency("BTC", "Bitcoin", 90000, 10, 1000000, 300000);
            CryptoCurrency ETH = new CryptoCurrency("ETH", "Ethereum", 3000, 5, 500000, 150000);
            _cryptoData.ListOfCrypto.Add(BTC);
            _cryptoData.ListOfCrypto.Add(ETH);
            _cryptoData._dataGrid.Rows.Add(BTC);
            _cryptoData._dataGrid.Rows.Add(ETH);
            _cryptoData.AllocateList();
            Assert.Equal(2, _cryptoData.ListOfCrypto.Count);
            Assert.Equal("BTC", _cryptoData.ListOfCrypto[0].ShortName);
            Assert.Equal("ETH", _cryptoData.ListOfCrypto[1].ShortName);
        }
        [Fact]
        public void StableCoin_Validate_ShouldReturnTrueForZeroPriceChange()
        {
            var stableCoin = new StableCoin { PriceChange24H = 0 };
            bool result = stableCoin.Validate();
            Assert.True(result);
        }

        [Fact]
        public void StableCoin_Validate_ShouldReturnFalseForNonZeroPriceChange()
        {
            var stableCoin = new StableCoin { PriceChange24H = 5 };
            bool result = stableCoin.Validate();
            Assert.False(result);
        }
        [Fact]
        public void ParseInt_ShouldHandleNull()
        {
            int result = _parser.ParseInt(null);
            Assert.Equal(0, result);
        }

        [Fact]
        public void ParseDouble_ShouldHandleNull()
        {
            double result = _parser.ParseDouble(null);
            Assert.Equal(0.0, result);
        }
        [Fact]
        public void DetermineCryptoType_ShouldReturnNullForUnknownType()
        {
            var row = CreateDataGridViewRow(new object?[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null });
            var values = row.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString()).ToArray();

            var result = _parser.DetermineCryptoType(values, row);

            Assert.Null(result);
        }
        [Fact]
        public void CryptoCurrency_Properties_ShouldSetAndGetCorrectly()
        {
            var crypto = new CryptoCurrency
            {
                ShortName = "BTC",
                FullName = "Bitcoin",
                LastPrice = 90000,
                PriceChange24H = 10,
                Capitalization = 1000000,
                VolumeTrading = 300000
            };

            Assert.Equal("BTC", crypto.ShortName);
            Assert.Equal("Bitcoin", crypto.FullName);
            Assert.Equal(90000, crypto.LastPrice);
            Assert.Equal(10, crypto.PriceChange24H);
            Assert.Equal(1000000, crypto.Capitalization);
            Assert.Equal(300000, crypto.VolumeTrading);
        }
        private static DataGridViewRow CreateDataGridViewRow(object[] values)
        {
            var row = new DataGridViewRow();
            foreach (var value in values)
            {
                row.Cells.Add(new DataGridViewTextBoxCell { Value = value });
            }
            return row;
        }
    }
}
