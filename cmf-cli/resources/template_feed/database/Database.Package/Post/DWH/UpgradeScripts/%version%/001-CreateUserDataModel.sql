IF(NOT EXISTS(SELECT 1 FROM [sys].[schemas] WHERE [name] = N'UserDataModel'))
    EXEC sp_executeSQL N'CREATE SCHEMA [UserDataModel]'