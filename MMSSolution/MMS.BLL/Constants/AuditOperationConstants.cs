namespace MMS.BLL.Constants
{
    /// <summary>
    /// Audit operation constants for activity logging.
    /// DCC Compliance (NCA DCC-1:2022 Section 2-4): Defines operation types for audit trail.
    /// These IDs must match the Operations table in MMS_Audit_Trail database.
    /// </summary>
    public static class AuditOperationConstants
    {
        /// <summary>User login operation</summary>
        public const int Login = 1;

        /// <summary>User logout operation</summary>
        public const int Logout = 2;

        /// <summary>Create/Add new record</summary>
        public const int Create = 3;

        /// <summary>Update existing record</summary>
        public const int Update = 4;

        /// <summary>Delete record</summary>
        public const int Delete = 5;

        /// <summary>View/Read record</summary>
        public const int View = 6;

        /// <summary>Download file</summary>
        public const int Download = 7;

        /// <summary>Upload file</summary>
        public const int Upload = 8;

        /// <summary>Approve action</summary>
        public const int Approve = 9;

        /// <summary>Reject action</summary>
        public const int Reject = 10;

        /// <summary>Submit action</summary>
        public const int Submit = 11;

        /// <summary>Search operation</summary>
        public const int Search = 12;

        /// <summary>Export data</summary>
        public const int Export = 13;

        /// <summary>Print document</summary>
        public const int Print = 14;

        /// <summary>Password change</summary>
        public const int PasswordChange = 15;

        /// <summary>Permission change</summary>
        public const int PermissionChange = 16;

        /// <summary>Role assignment</summary>
        public const int RoleAssignment = 17;

        /// <summary>Settings change</summary>
        public const int SettingsChange = 18;

        /// <summary>Meeting action</summary>
        public const int MeetingAction = 19;

        /// <summary>Task action</summary>
        public const int TaskAction = 20;

        /// <summary>Signature action</summary>
        public const int SignatureAction = 21;

        /// <summary>Two-factor authentication</summary>
        public const int TwoFactorAuth = 22;
    }
}
