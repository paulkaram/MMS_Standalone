using MMS.DTO.CommitteeItems;

namespace MMS.DTO.Bids
{
    public class BidDetailDto : BidDto
    {
        public List<BidStakeholderDto> Stakeholders { get; set; } = new();
        public List<BidStatusHistoryDto> History { get; set; } = new();
        public List<CommitteeItemDto> Items { get; set; } = new();
    }
}
