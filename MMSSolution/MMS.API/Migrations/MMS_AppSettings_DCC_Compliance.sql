-- =============================================
-- MMS AppSettings - DCC Compliance Configuration
-- Run this script on your MMS database
-- =============================================

USE [MMS];
GO

-- =============================================
-- Add AuditTrail Connection String
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[AppSettings] WHERE [Name] = 'ConnectionStrings:AuditTrail')
BEGIN
    INSERT INTO [dbo].[AppSettings] ([Name], [Value])
    VALUES ('ConnectionStrings:AuditTrail', 'database=MMS_Audit_Trail;server=INTALIO-PKA1\SQL2022;Integrated Security=false;User Id=ES;Password=ES;encrypt=false;TrustServerCertificate=True;');
    PRINT 'AuditTrail connection string added';
END
ELSE
BEGIN
    UPDATE [dbo].[AppSettings]
    SET [Value] = 'database=MMS_Audit_Trail;server=INTALIO-PKA1\SQL2022;Integrated Security=false;User Id=ES;Password=ES;encrypt=false;TrustServerCertificate=True;'
    WHERE [Name] = 'ConnectionStrings:AuditTrail';
    PRINT 'AuditTrail connection string updated';
END
GO

-- =============================================
-- DCC Compliance: Rate Limiting Settings
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[AppSettings] WHERE [Name] = 'RateLimiting:Enabled')
    INSERT INTO [dbo].[AppSettings] ([Name], [Value]) VALUES ('RateLimiting:Enabled', 'true');

IF NOT EXISTS (SELECT * FROM [dbo].[AppSettings] WHERE [Name] = 'RateLimiting:MaxRequestsPerWindow')
    INSERT INTO [dbo].[AppSettings] ([Name], [Value]) VALUES ('RateLimiting:MaxRequestsPerWindow', '100');

IF NOT EXISTS (SELECT * FROM [dbo].[AppSettings] WHERE [Name] = 'RateLimiting:WindowSeconds')
    INSERT INTO [dbo].[AppSettings] ([Name], [Value]) VALUES ('RateLimiting:WindowSeconds', '60');

IF NOT EXISTS (SELECT * FROM [dbo].[AppSettings] WHERE [Name] = 'RateLimiting:AuthEndpointMaxRequests')
    INSERT INTO [dbo].[AppSettings] ([Name], [Value]) VALUES ('RateLimiting:AuthEndpointMaxRequests', '5');

IF NOT EXISTS (SELECT * FROM [dbo].[AppSettings] WHERE [Name] = 'RateLimiting:AuthEndpointWindowSeconds')
    INSERT INTO [dbo].[AppSettings] ([Name], [Value]) VALUES ('RateLimiting:AuthEndpointWindowSeconds', '60');

PRINT 'Rate Limiting settings added';
GO

-- =============================================
-- DCC Compliance: Session Limit Settings
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[AppSettings] WHERE [Name] = 'SessionLimit:EnableConcurrentSessionLimit')
    INSERT INTO [dbo].[AppSettings] ([Name], [Value]) VALUES ('SessionLimit:EnableConcurrentSessionLimit', 'true');

IF NOT EXISTS (SELECT * FROM [dbo].[AppSettings] WHERE [Name] = 'SessionLimit:MaxConcurrentSessions')
    INSERT INTO [dbo].[AppSettings] ([Name], [Value]) VALUES ('SessionLimit:MaxConcurrentSessions', '3');

IF NOT EXISTS (SELECT * FROM [dbo].[AppSettings] WHERE [Name] = 'SessionLimit:SessionTimeoutMinutes')
    INSERT INTO [dbo].[AppSettings] ([Name], [Value]) VALUES ('SessionLimit:SessionTimeoutMinutes', '30');

PRINT 'Session Limit settings added';
GO

PRINT '=========================================';
PRINT 'DCC Compliance AppSettings completed';
PRINT '=========================================';
GO
