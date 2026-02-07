-- Reset DATA only (keeps schema/migrations) for SQL Server.
-- Database: hospital_appointment (adjust if needed)
-- IMPORTANT: This deletes rows from almost all tables.
-- It keeps __EFMigrationsHistory by default.

-- Some tables/indexes (e.g., filtered indexes, computed columns) require specific SET options.
-- Ensure consistent session settings to avoid errors like:
--   Msg 1934 ... SET options have incorrect settings: 'QUOTED_IDENTIFIER'
SET ANSI_NULLS ON;
SET ANSI_WARNINGS ON;
SET ANSI_PADDING ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;

SET NOCOUNT ON;

DECLARE @keep TABLE (FullName nvarchar(256) NOT NULL);
INSERT INTO @keep (FullName) VALUES
(N'[dbo].[__EFMigrationsHistory]');

-- Disable constraints
DECLARE @sql nvarchar(max) = N'';
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N' NOCHECK CONSTRAINT ALL;' + CHAR(13)
FROM sys.tables t;
EXEC sp_executesql @sql;

-- Delete rows (excluding kept tables)
SET @sql = N'';
SELECT @sql += N'DELETE FROM ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N';' + CHAR(13)
FROM sys.tables t
WHERE (N'[' + SCHEMA_NAME(t.schema_id) + N'].[' + t.name + N']') NOT IN (SELECT FullName FROM @keep)
  AND t.is_ms_shipped = 0;
EXEC sp_executesql @sql;

-- Re-enable constraints
SET @sql = N'';
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(13)
FROM sys.tables t;
EXEC sp_executesql @sql;

PRINT 'Done: data cleared (schema kept).';
