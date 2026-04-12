namespace MMS.DTO
{
    public record AttachmentActionDto
    {
        public bool Sign { get; set; }
        public bool RemoveSign { get; set; }
        public bool Barcode { get; set; }
        public bool Stamps { get; set; }
        public bool Annotations { get; set; }

        public string ToQueryString()
        {
            string query = "";
            if (Sign)
            {
                query += "s=1&";
            }
            if (Barcode)
            {
                query += "b=1&";
            }
            if (Stamps)
            {
                query += "st=1&";
            }
            if (Annotations)
            {
                query += "an=1&";
            }
			if (RemoveSign)
			{
				query += "rm=1&";
			}
			return query;
        }

        public static AttachmentActionDto LoadFromQuery(string query)
        {
            if(string.IsNullOrEmpty(query))
            {
				return new AttachmentActionDto();
			}

            return new AttachmentActionDto()
            {
                Annotations = query.Contains("an=1&"),
				Stamps = query.Contains("st=1&"),
				Barcode = query.Contains("b=1&"),
                Sign = query.Contains("s=1&"),
                RemoveSign = query.Contains("rm=1&")
            };
		}
    }
}
