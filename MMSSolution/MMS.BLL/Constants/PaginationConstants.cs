namespace MMS.BLL.Constants
{
    public static class PaginationConstants
    {
        /// <summary>
        /// Default number of items per page
        /// </summary>
        public const int DefaultPageSize = 10;

        /// <summary>
        /// Maximum allowed page size to prevent DoS attacks
        /// </summary>
        public const int MaxPageSize = 100;

        /// <summary>
        /// Minimum page number
        /// </summary>
        public const int MinPage = 1;

        /// <summary>
        /// Default count for list operations (e.g., upcoming meetings, recent activities)
        /// </summary>
        public const int DefaultListCount = 10;

        /// <summary>
        /// Validates and normalizes pagination parameters
        /// </summary>
        /// <param name="page">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>Validated (page, pageSize) tuple</returns>
        public static (int page, int pageSize) ValidatePagination(int page, int pageSize)
        {
            // Ensure page is at least 1
            if (page < MinPage) page = MinPage;

            // Ensure pageSize is within bounds
            if (pageSize < 1) pageSize = DefaultPageSize;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            return (page, pageSize);
        }
    }
}
