using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Common.Log.TraceListenerLog
{
    
    public class TraceListenerLog : TraceListener
    {
        public delegate void TraceEventHandler(TraceListenerLog sender, TraceEventType eventType, string message);
        public event TraceEventHandler OnTrace;
        protected virtual void OnTraceTrigger(TraceEventType eventType, string message) => OnTrace?.Invoke(this, eventType, message);

        public TraceListenerLog(string name)
            : base(name)
        {
        }

        protected TraceListenerLog()
        {

        }

        #region override
        public override void Write(string message)
        {
            Trace(TraceEventType.Verbose, message);
        }

        public override void WriteLine(string message)
        {
            Trace(TraceEventType.Verbose, message);
        }

        public override void Fail(string message)
        {
            Trace(TraceEventType.Critical, message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceEvent(eventCache, eventType, args != null ? string.Format(format, args) : format);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventCache, eventType, message);
        }
        #endregion

        protected virtual void Trace(TraceEventType eventType, string message)
        {
            OnTraceTrigger(eventType, message);
        }

        void TraceEvent(TraceEventCache eventCache, TraceEventType eventType, string message)
        {
            Trace(eventType, message);
        }

        //====================================================================================
        protected TraceLevel TraceEventTypeToTraceLevel(TraceEventType eventType)
        {
            switch(eventType)
            {
                case TraceEventType.Critical:
                    return TraceLevel.Error;
                case TraceEventType.Error:
                    return TraceLevel.Error;
                case TraceEventType.Warning:
                    return TraceLevel.Warning;
                case TraceEventType.Information:
                    return TraceLevel.Info;
                case TraceEventType.Verbose:
                    return TraceLevel.Verbose;
                case TraceEventType.Start:
                    return TraceLevel.Verbose;
                case TraceEventType.Stop:
                    return TraceLevel.Verbose;
                case TraceEventType.Suspend:
                    return TraceLevel.Verbose;
                case TraceEventType.Resume:
                    return TraceLevel.Verbose;
                case TraceEventType.Transfer:
                    return TraceLevel.Verbose;
                default: throw new Exception("Unknown TraceEventType.");
            }
        }

        protected string GetStackStr(int skipStackFrames, bool reverseStack)
        {
            var stackFrames = new StackTrace(skipStackFrames).GetFrames();
            var stackMethods = stackFrames.Select(f => { var m = f.GetMethod(); return $"{m?.DeclaringType?.Name}.{m?.Name}"; });
            if(reverseStack)
                stackMethods = stackMethods.Reverse();
            var stackStr = string.Join(" => ", stackMethods);

            return stackStr;
        }
    }
}


