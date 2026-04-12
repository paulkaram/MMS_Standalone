-- =============================================
-- Create MMS_Audit_Trail Database
-- DCC Compliance (NCA DCC-1:2022)
--
-- Run this script in SSMS with admin privileges
-- =============================================

-- Create the database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'MMS_Audit_Trail')
BEGIN
    CREATE DATABASE [MMS_Audit_Trail];
    PRINT 'Database MMS_Audit_Trail created successfully';
END
ELSE
BEGIN
    PRINT 'Database MMS_Audit_Trail already exists';
END
GO

USE [MMS_Audit_Trail];
GO

-- =============================================
-- Create Operations table (lookup table)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Operations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Operations] (
        [Id] INT NOT NULL,
        [OperationCodeName] NVARCHAR(256) NOT NULL,
        [OperationNameEn] NVARCHAR(256) NOT NULL,
        [OperationNameAr] NVARCHAR(256) NOT NULL,
        CONSTRAINT [PK_Operations] PRIMARY KEY CLUSTERED ([Id])
    );
    PRINT 'Table Operations created successfully';
END
GO

-- =============================================
-- Create DatabaseNames table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DatabaseNames]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DatabaseNames] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [DBName] NVARCHAR(256) NOT NULL,
        CONSTRAINT [PK_DatabaseNames] PRIMARY KEY CLUSTERED ([Id])
    );
    PRINT 'Table DatabaseNames created successfully';
END
GO

-- =============================================
-- Create ActivityLog table with DCC compliance fields
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActivityLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ActivityLog] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Username] NVARCHAR(50) NOT NULL,
        [UserId] INT NOT NULL,
        [OperationId] INT NOT NULL,
        [ProcessInstanceId] INT NULL,
        [CommentId] INT NULL,
        [LetterId] INT NULL,
        [RecordId] INT NULL,
        [ActionName] NVARCHAR(100) NOT NULL,
        [ControllerName] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [AdditionalInfo] NVARCHAR(1000) NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        -- DCC Compliance fields (NCA DCC-1:2022 Section 2-4)
        [IpAddress] NVARCHAR(45) NULL,
        [UserAgent] NVARCHAR(500) NULL,
        [SessionId] NVARCHAR(100) NULL,
        [DeviceInfo] NVARCHAR(500) NULL,
        CONSTRAINT [PK_lActivityLog] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_ActivityLog_Operations] FOREIGN KEY ([OperationId])
            REFERENCES [dbo].[Operations] ([Id])
    );
    PRINT 'Table ActivityLog created successfully';
END
GO

-- =============================================
-- Create indexes for performance
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLog_UserId' AND object_id = OBJECT_ID('dbo.ActivityLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLog_UserId]
    ON [dbo].[ActivityLog] ([UserId]);
    PRINT 'Index IX_ActivityLog_UserId created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLog_CreatedDate' AND object_id = OBJECT_ID('dbo.ActivityLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLog_CreatedDate]
    ON [dbo].[ActivityLog] ([CreatedDate]);
    PRINT 'Index IX_ActivityLog_CreatedDate created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLog_IpAddress' AND object_id = OBJECT_ID('dbo.ActivityLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLog_IpAddress]
    ON [dbo].[ActivityLog] ([IpAddress])
    WHERE [IpAddress] IS NOT NULL;
    PRINT 'Index IX_ActivityLog_IpAddress created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLog_SessionId' AND object_id = OBJECT_ID('dbo.ActivityLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLog_SessionId]
    ON [dbo].[ActivityLog] ([SessionId])
    WHERE [SessionId] IS NOT NULL;
    PRINT 'Index IX_ActivityLog_SessionId created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLog_Security_Audit' AND object_id = OBJECT_ID('dbo.ActivityLog'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ActivityLog_Security_Audit]
    ON [dbo].[ActivityLog] ([UserId], [IpAddress], [CreatedDate])
    INCLUDE ([SessionId], [DeviceInfo], [ActionName], [ControllerName]);
    PRINT 'Index IX_ActivityLog_Security_Audit created';
END
GO

-- =============================================
-- Insert default Operations
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[Operations] WHERE [Id] = 1)
BEGIN
    INSERT INTO [dbo].[Operations] ([Id], [OperationCodeName], [OperationNameEn], [OperationNameAr]) VALUES
    (1, 'LOGIN', 'Login', N'تسجيل الدخول'),
    (2, 'LOGOUT', 'Logout', N'تسجيل الخروج'),
    (3, 'CREATE', 'Create', N'إنشاء'),
    (4, 'UPDATE', 'Update', N'تحديث'),
    (5, 'DELETE', 'Delete', N'حذف'),
    (6, 'VIEW', 'View', N'عرض'),
    (7, 'DOWNLOAD', 'Download', N'تحميل'),
    (8, 'UPLOAD', 'Upload', N'رفع'),
    (9, 'APPROVE', 'Approve', N'موافقة'),
    (10, 'REJECT', 'Reject', N'رفض'),
    (11, 'SUBMIT', 'Submit', N'إرسال'),
    (12, 'SEARCH', 'Search', N'بحث'),
    (13, 'EXPORT', 'Export', N'تصدير'),
    (14, 'PRINT', 'Print', N'طباعة'),
    (15, 'PASSWORD_CHANGE', 'Password Change', N'تغيير كلمة المرور'),
    (16, 'PERMISSION_CHANGE', 'Permission Change', N'تغيير الصلاحيات'),
    (17, 'ROLE_ASSIGNMENT', 'Role Assignment', N'تعيين الدور'),
    (18, 'SETTINGS_CHANGE', 'Settings Change', N'تغيير الإعدادات'),
    (19, 'MEETING_ACTION', 'Meeting Action', N'إجراء الاجتماع'),
    (20, 'TASK_ACTION', 'Task Action', N'إجراء المهمة'),
    (21, 'SIGNATURE_ACTION', 'Signature Action', N'إجراء التوقيع'),
    (22, 'TWO_FACTOR_AUTH', 'Two-Factor Authentication', N'المصادقة الثنائية');
    PRINT 'Default Operations inserted';
END
GO

-- Insert database name
IF NOT EXISTS (SELECT * FROM [dbo].[DatabaseNames] WHERE [DBName] = 'MMS_Audit_Trail')
BEGIN
    INSERT INTO [dbo].[DatabaseNames] ([DBName]) VALUES ('MMS_Audit_Trail');
    PRINT 'DatabaseName entry inserted';
END
GO

PRINT '=========================================';
PRINT 'MMS_Audit_Trail database setup completed';
PRINT 'DCC Compliance (NCA DCC-1:2022) ready';
PRINT '=========================================';
GO
