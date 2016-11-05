using System;
using System.Collections.Generic;
using System.Linq;

namespace InformaticaCloudClient
{
    public class ActivityBase
    {
        public string id { get; set; }
        public string type { get; set; }
        public string objectName { get; set; }
        public string runId { get; set; }
        public string startedBy;
        public string scheduleName;
        public string starter  { get {return String.IsNullOrEmpty(startedBy) ? scheduleName : startedBy; } }
        public DateTime startTimeUtc;
        public DateTime endTimeUtc;

        public DateTime startTime { get {return startTimeUtc.ToLocalTime(); }  }
        public DateTime endTime { get { return endTimeUtc.ToLocalTime(); }  }
        public int failedSourceRows { get; set; }
        public int successSourceRows { get; set; }
        public int failedTargetRows { get; set; }
        public int successTargetRows { get; set; }
        public string errorMsg { get; set; }
        public string runtimeEnvironmentId;


        public ActivityBase(ActivityBase t)
        {
            if (t == null) return;
            id = t.id;
            type = t.type;
            objectName = t.objectName;
            runId = t.runId;
            startedBy = t.startedBy;
            scheduleName = t.scheduleName;
            startTimeUtc = t.startTimeUtc;
            endTimeUtc = t.endTimeUtc;
            failedSourceRows = t.failedSourceRows;
            successSourceRows = t.successSourceRows;
            failedTargetRows = t.failedTargetRows;
            successTargetRows = t.successTargetRows;
            errorMsg = t.errorMsg;
            runtimeEnvironmentId = t.runtimeEnvironmentId;
    }
    }

    /// <summary>
    /// DTOs and stubs for task data
    /// </summary>
    public class ActivityLogEntry : ActivityBase
    {
        public int state; 
        /* 
         *  1. The task completed successfully
         *  2. The task completed with errors
         *  3. The task failed to complete
        */
        public List<ActivityLogEntry> entries;

        public ActivityLogEntry(ActivityBase t) : base(t)
        {
        }
    }

    public class ActivityMonitorEntry : ActivityBase
    {
        public string taskId;
        public string taskName;
        public string executionState;
        /*
         * INITIALIZED
         * RUNNING
         * STOPPING
         * COMPLETED
         * FAILED
         */
        public string agentId;
        public string runContextType;
        public List<ActivityMonitorEntry> entries;

        public ActivityMonitorEntry(ActivityBase t) : base(t)
        {
        }
    }

    public class ActivityReportRecord : ActivityBase
    {
        public int level { get; set; }
        public string parentId { get; set; }
        public string executionState { get; set; }

        public ActivityReportRecord(ActivityBase t) : base(t)
        {
        }

        public ActivityReportRecord(ActivityLogEntry t, string _parentId, int _level) : base(t)
        {
            this.executionState =
                t.state == 1 ? "COMPLETED" :
                t.state == 2 ? "COMPLETED" :
                "FAILED";
            parentId = _parentId;
            level = _level;
        }

        public ActivityReportRecord(ActivityMonitorEntry t, string _parentId, int _level) : base(t)
        {
            this.executionState = t.executionState;
            parentId = _parentId;
            level = _level;
            objectName = String.IsNullOrEmpty(objectName) ? t.taskName : objectName;
        }

    }

    public class Task
    {
        public string id; // TaskID
        public string orgId; // Organization ID.
        public string name; // Task name.
        public string description; //Description.
        public DateTime updateTime; //Last time the task was updated.
        public string createdBy; //User who created the task.
        public string updatedBy; //User who last updated the task.
    }

    public static class LogReport
    {

        public static List<ActivityReportRecord> MakeLogReport(IEnumerable<ActivityMonitorEntry> monitorRecords, IEnumerable<ActivityLogEntry> logRecords)
        {
            return new List<ActivityReportRecord>()
                .Append(monitorRecords, 0, null)
                .Append(logRecords, 0, null);
        }

        /// <summary>
        /// Convert the ActivityMonitorEntry to ActivityReportRecords
        /// Flatten the list by recursively doing the same with each ActivityMonitorEntry in entries
        /// Return the result appended to _in 
        /// </summary>
        /// <param name="_in"></param>
        /// <param name="monitorRecords"></param>
        /// <param name="level"></param>
        /// <param name="parentId"></param>
        /// <returns>monitorRecords flattened and appended to _in </returns>
        public static List<ActivityReportRecord> Append(this List<ActivityReportRecord> _in, IEnumerable<ActivityMonitorEntry> monitorRecords, int level, string parentId)
        {
            return monitorRecords.Aggregate(_in, (recs, rec) => { recs.Add(new ActivityReportRecord(rec, parentId, level)); return Append(recs, rec.entries, level+1, rec.id); });
        }
        /// <summary>
        /// Convert the ActivityLogEntry records to ActivityReportRecords
        /// Flatten the list by recursively doing the same with each ActivityLogEntry in entries
        /// Return the result appended to _in
        /// </summary>
        /// <param name="_in"></param>
        /// <param name="logRecords"></param>
        /// <param name="level"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static List<ActivityReportRecord> Append(this List<ActivityReportRecord> _in, IEnumerable<ActivityLogEntry> logRecords, int level, string parentId)
        {
            return logRecords.Aggregate(_in, (recs, rec) => { recs.Add(new ActivityReportRecord(rec, parentId, level)); return Append(recs, rec.entries, level + 1, rec.id); });
        }

    }
}
