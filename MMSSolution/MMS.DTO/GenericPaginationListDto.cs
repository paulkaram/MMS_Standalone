namespace MMS.DTO
{
	public class GenericPaginationListDto<T>
	{
		public List<T> Data { get; set; }
		public int Total { get; set; }

        public GenericPaginationListDto()
        {
            Data = new();
        }

        public GenericPaginationListDto(int total, List<T> data)
        {
            Total = total;
			Data = data != null ? data : new();
        }
    }
}
