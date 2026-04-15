namespace MMS.DAL.Enumerations
{
    /// <summary>
    /// Bid-item classification (§5.11 procurement taxonomy). Distinct from
    /// `CommitteeItemType` which is the agenda-items taxonomy (§5.7).
    /// Seeded in the `BidItemType` table; used by both bid clauses (§5.6)
    /// and procurement competitor-attachment categorization.
    /// </summary>
    public enum BidItemTypeDbEnum
    {
        General = 1,
        Technical = 2,
        Financial = 3,
        Administrative = 4,
        Legal = 5
    }
}
