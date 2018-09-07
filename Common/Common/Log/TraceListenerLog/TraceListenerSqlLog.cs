using System;
using System.Linq;

namespace Common.Log.TraceListenerLog
{
    using System;
    using System.Diagnostics;
    using System.Linq;


    namespace OMLib.Log
    {
        public class TraceListenerSqlLog : TraceListenerLog
        {
            string _ProgVersion;
            bool _ReverceStack;
            public static string UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            public string ConnectionString { get; set; }
            public string TableName { get; set; }
            public int SkipStackFrames { get; set; }
            TraceSwitch _TraceSwitch;
            public string Command { get; set; } =
                $@"INSERT INTO #tableName (DT,Message,UserName,EventType,Stack,ProgVersion)
values(getdate(),@Message,@UserName,@EventType,@Stack,@ProgVersion)";

            /*    public string CreateTableCommand { get; set; } = @"
            CREATE TABLE dbo.[LogTrace]
            (Oid int NOT NULL IDENTITY (1, 1) PRIMARY KEY,
            DT datetime2(2) NULL,
            Message nvarchar(MAX) NULL,
            UserName nvarchar(100) NULL,
            Host nvarchar(150) NULL DEFAULT host_name(),
            SqlUserName nvarchar(150) NULL DEFAULT user_name(),
            EventType nvarchar(20) NULL,
            Stack nvarchar(MAX) NULL,
            ProgVersion nvarchar(20) NULL)
        ";*/

            /// <summary>
            /// 
            /// </summary>
            /// <param name="connectionString">Строка подключения для SqlConnection</param>
            /// <param name="tableName">Имя талицы SQL для хранения логов.</param>
            /// <param name="progVersion">Версия программы (выводится в лог)</param>
            /// <param name="skipStackFrames">Пропуск n фрэймов</param>
            /// <param name="reverseStack">Меняет направление стека</param>
            /// <param name="traceSwitch">Если не указано берет из конфига c именем "TraceListenerSqlLogSwitch".</param>
            public TraceListenerSqlLog(string connectionString, string tableName, string progVersion = "1.0", int skipStackFrames = 3, bool reverseStack = true, TraceSwitch traceSwitch = null) : base("SqlLog")
            {
                _TraceSwitch = traceSwitch ?? new TraceSwitch("TraceListenerSqlLogSwitch", "TraceSwitch for TraceListenerSqlLog");
                _ReverceStack = reverseStack;
                _ProgVersion = progVersion;
                SkipStackFrames = skipStackFrames;
                TableName = tableName;
                ConnectionString = connectionString;
                Command = Command.Replace("#tableName", TableName);
            }

            protected override void Trace(TraceEventType eventType, string message)
            {
                try
                {
                    base.Trace(eventType, message);

                    if(_TraceSwitch.Level < TraceEventTypeToTraceLevel(eventType))
                        return;

                    using(System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(ConnectionString))
                    {
                        var sqlCommand = connection.CreateCommand();
                        sqlCommand.CommandText = Command;
                        sqlCommand.Parameters.Add("@Message", System.Data.SqlDbType.NVarChar).Value = message;
                        sqlCommand.Parameters.Add("@UserName", System.Data.SqlDbType.NVarChar).Value = UserName;
                        sqlCommand.Parameters.Add("@EventType", System.Data.SqlDbType.NVarChar).Value = eventType.ToString();
                        sqlCommand.Parameters.Add("@Stack", System.Data.SqlDbType.NVarChar).Value = GetStackStr(SkipStackFrames,_ReverceStack);
                        sqlCommand.Parameters.Add("@ProgVersion", System.Data.SqlDbType.NVarChar).Value = _ProgVersion;

                        BeforeExecute?.Invoke(this, new EventArgsTrace(sqlCommand));

                        connection.Open();
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch(Exception ex)
                {
#if DEBUG
                    throw ex;
#endif
                }
            }


            public EventHandler<EventArgsTrace> BeforeExecute;
            public class EventArgsTrace : EventArgs
            {
                public EventArgsTrace(System.Data.SqlClient.SqlCommand command) : base() => Command = command;
                System.Data.SqlClient.SqlCommand Command { get; }
            }
        }
    }

}
