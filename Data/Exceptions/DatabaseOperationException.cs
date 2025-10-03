using System;
using System.Data.SqlClient;

namespace SusEquip.Data.Exceptions
{
    /// <summary>
    /// Exception thrown when database operations fail.
    /// Provides detailed information about SQL errors and operation context.
    /// </summary>
    public class DatabaseOperationException : SusEquipException
    {
        public string Operation { get; }
        public string TableName { get; }
        public SqlException? SqlException { get; }

        public DatabaseOperationException(string message, string operation, string tableName) 
            : base("DB_OPERATION_FAILED", message, "A database error occurred. Please try again.", ErrorSeverity.Error)
        {
            Operation = operation ?? string.Empty;
            TableName = tableName ?? string.Empty;
            
            AddContext("Operation", Operation);
            AddContext("TableName", TableName);
        }

        public DatabaseOperationException(string message, string operation, string tableName, SqlException sqlException) 
            : base("DB_OPERATION_FAILED", message, "A database error occurred. Please try again.", ErrorSeverity.Error, sqlException)
        {
            Operation = operation ?? string.Empty;
            TableName = tableName ?? string.Empty;
            SqlException = sqlException;
            
            AddContext("Operation", Operation);
            AddContext("TableName", TableName);
            AddContext("SqlErrorNumber", sqlException?.Number.ToString() ?? "Unknown");
            AddContext("SqlState", sqlException?.State.ToString() ?? "Unknown");
        }

        public DatabaseOperationException(string message, string operation, string tableName, Exception innerException) 
            : base("DB_OPERATION_FAILED", message, "A database error occurred. Please try again.", ErrorSeverity.Error, innerException)
        {
            Operation = operation ?? string.Empty;
            TableName = tableName ?? string.Empty;
            
            AddContext("Operation", Operation);
            AddContext("TableName", TableName);
        }
    }
}