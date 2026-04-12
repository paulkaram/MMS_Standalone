using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.AuditLogs;

/// <summary>
/// Activity log entity for audit trail.
/// DCC Compliance (NCA DCC-1:2022 Section 2-4): Enhanced with IP address, device info, and session tracking.
/// </summary>
public partial class ActivityLog
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public int UserId { get; set; }

    public int OperationId { get; set; }

    public int? ProcessInstanceId { get; set; }

    public int? CommentId { get; set; }

    public int? LetterId { get; set; }

    public int? RecordId { get; set; }

    public string ActionName { get; set; } = null!;

    public string ControllerName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? AdditionalInfo { get; set; }

    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// DCC 2-4: Client IP address for security audit trail
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// DCC 2-4: User agent string for device identification
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// DCC 2-4: Session identifier for tracking user sessions
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// DCC 2-4: Device fingerprint for enhanced security tracking
    /// </summary>
    public string? DeviceInfo { get; set; }

    public virtual Operation Operation { get; set; } = null!;
}
