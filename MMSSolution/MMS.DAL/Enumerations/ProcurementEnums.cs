namespace MMS.DAL.Enumerations
{
    /// <summary>
    /// Whether competitors upload one combined file or split technical+financial.
    /// BRD §5.11 gate: in single-file mode, prices are opened alongside technical.
    /// </summary>
    public enum ProcurementAttachmentModeDbEnum
    {
        SingleFile = 1,
        TwoFiles = 2
    }

    /// <summary>
    /// BRD §5.11 lists the competitor-attachment categories explicitly.
    /// Combined is used only in SingleFile mode; Technical/Financial in TwoFiles mode.
    /// </summary>
    public enum CompetitorAttachmentCategoryDbEnum
    {
        Technical = 1,
        Financial = 2,
        Combined = 3,
        Credentials = 4,
        BankGuarantee = 5,
        SmeCertificate = 6
    }

    /// <summary>
    /// Lifecycle states for a procurement project. These map onto the three
    /// committee types (Opening → Examination → Qualification) per §5.11.
    /// The workflow engine drives the actual transitions — these are just legacy
    /// status markers for the main project entity (Bid.StatusId equivalent).
    /// </summary>
    public enum ProcurementProjectStatusDbEnum
    {
        Draft = 1,
        PendingOpening = 2,
        OpeningDone = 3,
        ExaminationInProgress = 4,
        ExaminationApproved = 5,
        FinancialOpening = 6,
        QualificationInProgress = 7,
        QualificationDone = 8,
        FinalReportPending = 9,
        Awarded = 10,
        Closed = 11,
        Cancelled = 12
    }
}
