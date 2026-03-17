using GropMng.Core;
using GropMng.Web.Models.Common;

namespace GropMng.Tests.Models
{
    /// <summary>
    /// Unit tests for the PaginationModel class which serves as a lightweight DTO
    /// for pagination UI rendering. Tests cover the factory method, computed properties,
    /// and navigation logic.
    /// </summary>
    public class PaginationModelTests
    {
        #region Factory Method Tests

        /// <summary>
        /// Tests the FromPagedList factory method to ensure it correctly maps
        /// PagedList metadata to PaginationModel properties.
        /// </summary>
        [Fact]
        public void FromPagedList_CreatesCorrectPaginationModel()
        {
            // Arrange
            var source = Enumerable.Range(1, 50).AsQueryable();
            var pagedList = new PagedList<int>(source, pageIndex: 2, pageSize: 10);
            var currentPage = 3; // 1-based UI page number (pageIndex: 2 = page: 3)

            // Act
            var result = PaginationModel.FromPagedList(pagedList, currentPage);

            // Assert
            Assert.Equal(3, result.CurrentPage);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(50, result.TotalItems);
            Assert.Equal(5, result.TotalPages);
        }

        #endregion

        #region HasPreviousPage Tests

        /// <summary>
        /// Verifies that HasPreviousPage returns false when on the first page.
        /// Used to disable/hide the "Previous" button in UI.
        /// </summary>
        [Fact]
        public void HasPreviousPage_FirstPage_ReturnsFalse()
        {
            // Arrange
            var model = new PaginationModel
            {
                CurrentPage = 1,
                TotalPages = 5
            };

            // Act & Assert
            Assert.False(model.HasPreviousPage);
        }

        /// <summary>
        /// Verifies that HasPreviousPage returns true when on any page after the first.
        /// Used to enable the "Previous" button in UI.
        /// </summary>
        [Fact]
        public void HasPreviousPage_SecondPage_ReturnsTrue()
        {
            // Arrange
            var model = new PaginationModel
            {
                CurrentPage = 2,
                TotalPages = 5
            };

            // Act & Assert
            Assert.True(model.HasPreviousPage);
        }

        #endregion

        #region HasNextPage Tests

        /// <summary>
        /// Verifies that HasNextPage returns false when on the last page.
        /// Used to disable/hide the "Next" button in UI.
        /// </summary>
        [Fact]
        public void HasNextPage_LastPage_ReturnsFalse()
        {
            // Arrange
            var model = new PaginationModel
            {
                CurrentPage = 5,
                TotalPages = 5
            };

            // Act & Assert
            Assert.False(model.HasNextPage);
        }

        /// <summary>
        /// Verifies that HasNextPage returns true when not on the last page.
        /// Used to enable the "Next" button in UI.
        /// </summary>
        [Fact]
        public void HasNextPage_FirstPage_ReturnsTrue()
        {
            // Arrange
            var model = new PaginationModel
            {
                CurrentPage = 1,
                TotalPages = 5
            };

            // Act & Assert
            Assert.True(model.HasNextPage);
        }

        #endregion

        #region Navigation Properties Tests

        /// <summary>
        /// Tests the PreviousPage computed property which returns CurrentPage - 1.
        /// Used for generating the "Previous Page" link URL.
        /// </summary>
        [Fact]
        public void PreviousPage_ReturnsCorrectValue()
        {
            // Arrange
            var model = new PaginationModel
            {
                CurrentPage = 3
            };

            // Act & Assert
            Assert.Equal(2, model.PreviousPage);
        }

        /// <summary>
        /// Tests the NextPage computed property which returns CurrentPage + 1.
        /// Used for generating the "Next Page" link URL.
        /// </summary>
        [Fact]
        public void NextPage_ReturnsCorrectValue()
        {
            // Arrange
            var model = new PaginationModel
            {
                CurrentPage = 3
            };

            // Act & Assert
            Assert.Equal(4, model.NextPage);
        }

        #endregion

        #region Edge Cases and Validation Tests

        /// <summary>
        /// Tests that FromPagedList throws ArgumentNullException when pagedList is null.
        /// Ensures proper defensive programming for the factory method.
        /// </summary>
        [Fact]
        public void FromPagedList_WithNullPagedList_ThrowsArgumentNullException()
        {
            // Arrange
            IPagedList<int>? pagedList = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                PaginationModel.FromPagedList(pagedList!));
        }

        /// <summary>
        /// Tests that FromPagedList defaults to page 1 when currentPage parameter is omitted.
        /// This ensures sensible defaults for the factory method.
        /// </summary>
        [Fact]
        public void FromPagedList_DefaultCurrentPage_UsesOne()
        {
            // Arrange
            var source = Enumerable.Range(1, 20).AsQueryable();
            var pagedList = new PagedList<int>(source, pageIndex: 0, pageSize: 10);

            // Act
            var result = PaginationModel.FromPagedList(pagedList);

            // Assert
            Assert.Equal(1, result.CurrentPage);
        }

        #endregion
    }
}
