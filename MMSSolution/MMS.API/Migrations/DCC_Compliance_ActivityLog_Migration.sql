-- =============================================
-- DCC Compliance Migration Script
-- NCA Data Cybersecurity Controls (DCC-1:2022) Section 2-4
--
-- This script adds security audit fields to the ActivityLog table
-- for enhanced audit trail and compliance requirements.
-- =============================================

-- Check if columns already exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLog]') AND name = 'IpAddress')
BEGIN
    ALTER TABLE [dbo].[ActivityLog]
    ADD [IpAddress] NVARCHAR(45) NULL;

    PRINT 'Added IpAddress column to ActivityLog table';
END
ELSE
BEGIN
    PRINT 'IpAddress column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLog]') AND name = 'UserAgent')
BEGIN
    ALTER TABLE [dbo].[ActivityLog]
    ADD [UserAgent] NVARCHAR(500) NULL;

    PRINT 'Added UserAgent column to ActivityLog table';
END
ELSE
BEGIN
    PRINT 'UserAgent column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLog]') AND name = 'SessionId')
BEGIN
    ALTER TABLE [dbo].[ActivityLog]
    ADD [SessionId] NVARCHAR(100) NULL;

    PRINT 'Added SessionId column to ActivityLog table';
END
ELSE
BEGIN
    PRINT 'SessionId column already exists';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLog]') AND name = 'DeviceInfo')
BEGIN
    ALTER TABLE [dbo].[ActivityLog]
    ADD [DeviceInfo] NVARCHAR(500) NULL;

    PRINT 'Added DeviceInfo column to ActivityLog table';
END
ELSE
BEGIN
    PRINT 'DeviceInfo column already exists';
END
GO

-- Add index on IpAddress for security analysis queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLog_IpAddress' AND object_id = OBJECT_ID('dbo.ActivityLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLog_IpAddress]
    ON [dbo].[ActivityLog] ([IpAddress])
    WHERE [IpAddress] IS NOT NULL;

    PRINT 'Created index IX_ActivityLog_IpAddress';
END
GO

-- Add index on SessionId for session tracking queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLog_SessionId' AND object_id = OBJECT_ID('dbo.ActivityLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLog_SessionId]
    ON [dbo].[ActivityLog] ([SessionId])
    WHERE [SessionId] IS NOT NULL;

    PRINT 'Created index IX_ActivityLog_SessionId';
END
GO

-- Add composite index for security audit queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLog_Security_Audit' AND object_id = OBJECT_ID('dbo.ActivityLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLog_Security_Audit]
    ON [dbo].[ActivityLog] ([UserId], [IpAddress], [CreatedDate])
    INCLUDE ([SessionId], [DeviceInfo], [ActionName], [ControllerName]);

    PRINT 'Created index IX_ActivityLog_Security_Audit';
END
GO

PRINT 'DCC Compliance Migration completed successfully';
