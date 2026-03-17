using GropMng.Core;

namespace GropMng.Tests.Core
{
    /// <summary>
    /// Unit tests for the PagedList<T> class which provides pagination functionality.
    /// Tests cover metadata calculation, page navigation, edge cases, and different constructor overloads.
    /// </summary>
    public class PagedListTests
    {
        #region IQueryable Constructor Tests

        /// <summary>
        /// Verifies that the PagedList constructor correctly calculates pagination metadata
        /// (TotalCount, TotalPages, PageIndex, PageSize) when initialized with an IQueryable source.
        /// </summary>
        [Fact]
        public void Constructor_WithIQueryable_CalculatesMetadataCorrectly()
        {
            // Arrange
            var source = Enumerable.Range(1, 50).AsQueryable();
            var pageIndex = 0;
            var pageSize = 10;

            // Act
            var result = new PagedList<int>(source, pageIndex, pageSize);

            // Assert
            Assert.Equal(50, result.TotalCount);
            Assert.Equal(5, result.TotalPages);
            Assert.Equal(0, result.PageIndex);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(10, result.Count);
            Assert.False(result.HasPreviousPage);
            Assert.True(result.HasNextPage);
        }

        /// <summary>
        /// Tests that the PagedList returns the correct subset of items for a specific page.
        /// Verifies that Skip/Take logic correctly extracts items 21-30 for page index 2.
        /// </summary>
        [Fact]
        public void Constructor_WithIQueryable_ReturnsCorrectPageItems()
        {
            // Arrange
            var source = Enumerable.Range(1, 50).AsQueryable();
            var pageIndex = 2; // Page 3 (0-based: 0=page1, 1=page2, 2=page3)
            var pageSize = 10;

            // Act
            var result = new PagedList<int>(source, pageIndex, pageSize);

            // Assert
            Assert.Equal(10, result.Count);
            Assert.Equal(21, result[0]); // First item of page 3
            Assert.Equal(30, result[9]); // Last item of page 3
        }

        /// <summary>
        /// Tests handling of partial pages at the end of the dataset.
        /// With 47 items and page size 10, the last page (index 4) should contain only 7 items.
        /// </summary>
        [Fact]
        public void Constructor_WithIQueryable_LastPage_HandlesPartialPage()
        {
            // Arrange
            var source = Enumerable.Range(1, 47).AsQueryable();
            var pageIndex = 4; // Last page (5th page, 0-based index 4)
            var pageSize = 10;

            // Act
            var result = new PagedList<int>(source, pageIndex, pageSize);

            // Assert
            Assert.Equal(47, result.TotalCount);
            Assert.Equal(5, result.TotalPages);
            Assert.Equal(7, result.Count); // Only 7 items on last page
            Assert.Equal(41, result[0]);
            Assert.Equal(47, result[6]);
            Assert.True(result.HasPreviousPage);
            Assert.False(result.HasNextPage);
        }

        #endregion

        #region Page Navigation Flag Tests

        /// <summary>
        /// Verifies that HasPreviousPage=false and HasNextPage=true on the first page.
        /// </summary>
        [Fact]
        public void Constructor_WithIQueryable_FirstPage_HasCorrectFlags()
        {
            // Arrange
            var source = Enumerable.Range(1, 30).AsQueryable();

            // Act
            var result = new PagedList<int>(source, pageIndex: 0, pageSize: 10);

            // Assert
            Assert.False(result.HasPreviousPage);
            Assert.True(result.HasNextPage);
        }

        /// <summary>
        /// Verifies that both HasPreviousPage=true and HasNextPage=true on a middle page.
        /// </summary>
        [Fact]
        public void Constructor_WithIQueryable_MiddlePage_HasCorrectFlags()
        {
            // Arrange
            var source = Enumerable.Range(1, 30).AsQueryable();

            // Act
            var result = new PagedList<int>(source, pageIndex: 1, pageSize: 10);

            // Assert
            Assert.True(result.HasPreviousPage);
            Assert.True(result.HasNextPage);
        }

        /// <summary>
        /// Verifies that HasPreviousPage=true and HasNextPage=false on the last page.
        /// </summary>
        [Fact]
        public void Constructor_WithIQueryable_LastPage_HasCorrectFlags()
        {
            // Arrange
            var source = Enumerable.Range(1, 30).AsQueryable();

            // Act
            var result = new PagedList<int>(source, pageIndex: 2, pageSize: 10);

            // Assert
            Assert.True(result.HasPreviousPage);
            Assert.False(result.HasNextPage);
        }

        /// <summary>
        /// Tests the edge case where all data fits on a single page.
        /// Both navigation flags should be false, and TotalPages should equal 1.
        /// </summary>
        [Fact]
        public void Constructor_WithIQueryable_SinglePage_HasCorrectFlags()
        {
            // Arrange
            var source = Enumerable.Range(1, 5).AsQueryable();

            // Act
            var result = new PagedList<int>(source, pageIndex: 0, pageSize: 10);

            // Assert
            Assert.False(result.HasPreviousPage);
            Assert.False(result.HasNextPage);
            Assert.Equal(1, result.TotalPages);
        }

        /// <summary>
        /// Tests the edge case of an empty data source.
        /// Should result in TotalCount=0, TotalPages=0, and an empty collection.
        /// </summary>
        [Fact]
        public void Constructor_WithIQueryable_EmptySource_HandlesCorrectly()
        {
            // Arrange
            var source = Enumerable.Empty<int>().AsQueryable();

            // Act
            var result = new PagedList<int>(source, pageIndex: 0, pageSize: 10);

            // Assert
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.TotalPages); // Empty source results in 0 pages
            Assert.Empty(result);
            Assert.False(result.HasPreviousPage);
            Assert.False(result.HasNextPage);
        }

        #endregion

        #region IList Constructor Tests

        /// <summary>
        /// Tests the IList<T> constructor overload to ensure it calculates metadata correctly.
        /// This constructor is used when data is already materialized in memory.
        /// </summary>
        [Fact]
        public void Constructor_WithIList_CalculatesMetadataCorrectly()
        {
            // Arrange
            var source = Enumerable.Range(1, 50).ToList();
            var pageIndex = 1;
            var pageSize = 10;

            // Act
            var result = new PagedList<int>(source, pageIndex, pageSize);

            // Assert
            Assert.Equal(50, result.TotalCount);
            Assert.Equal(5, result.TotalPages);
            Assert.Equal(10, result.Count);
            Assert.Equal(11, result[0]); // First item of page 2
        }

        #endregion

        #region IEnumerable Constructor Tests

        /// <summary>
        /// Tests the IEnumerable<T> constructor that accepts a pre-calculated totalCount.
        /// Useful for scenarios where total count is known from a separate query (e.g., efficient SQL COUNT).
        /// </summary>
        [Fact]
        public void Constructor_WithIEnumerableAndTotalCount_UsesProvidedTotal()
        {
            // Arrange
            var source = Enumerable.Range(1, 10); // Only 10 items loaded
            var pageIndex = 0;
            var pageSize = 10;
            var totalCount = 100; // But actual total in database is 100

            // Act
            var result = new PagedList<int>(source, pageIndex, pageSize, totalCount);

            // Assert
            Assert.Equal(100, result.TotalCount);
            Assert.Equal(10, result.TotalPages);
            Assert.Equal(10, result.Count); // Only loaded 10 items
        }

        #endregion

        #region Special Features Tests

        /// <summary>
        /// Tests the getOnlyTotalCount feature which loads only metadata without items.
        /// Useful for displaying total count without fetching actual data (performance optimization).
        /// </summary>
        [Fact]
        public void Constructor_WithGetOnlyTotalCount_DoesNotLoadItems()
        {
            // Arrange
            var source = Enumerable.Range(1, 50).AsQueryable();

            // Act
            var result = new PagedList<int>(source, pageIndex: 0, pageSize: 10, getOnlyTotalCount: true);

            // Assert
            Assert.Equal(50, result.TotalCount);
            Assert.Equal(5, result.TotalPages);
            Assert.Empty(result); // No items loaded
        }

        /// <summary>
        /// Parameterized test to verify TotalPages calculation formula across different scenarios.
        /// Formula: TotalPages = ceiling(TotalCount / PageSize)
        /// </summary>
        [Theory]
        [InlineData(10, 3, 4)]  // 10 items, page size 3 -> ceil(10/3) = 4 pages
        [InlineData(20, 5, 4)]  // 20 items, page size 5 -> ceil(20/5) = 4 pages
        [InlineData(25, 10, 3)] // 25 items, page size 10 -> ceil(25/10) = 3 pages
        [InlineData(1, 10, 1)]  // 1 item, page size 10 -> ceil(1/10) = 1 page
        public void Constructor_CalculatesTotalPages_Correctly(int totalItems, int pageSize, int expectedPages)
        {
            // Arrange
            var source = Enumerable.Range(1, totalItems).AsQueryable();

            // Act
            var result = new PagedList<int>(source, pageIndex: 0, pageSize: pageSize);

            // Assert
            Assert.Equal(expectedPages, result.TotalPages);
        }

        #endregion
    }
}
