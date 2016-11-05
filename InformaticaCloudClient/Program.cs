using System;
using System.Linq;

namespace InformaticaCloudClient
{

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //FileHelper.Test(); return;
                CommandOptions opts = CommandOptions.ParseCommandLine(args);
                if (!String.IsNullOrEmpty(opts.ParserMessage))
                {
                    Console.WriteLine(opts.ParserMessage);
                    return;
                }
                if (opts.UserName != null && opts.Password != null)
                {
                    Credentials.UserName = opts.UserName;
                    Credentials.Password = opts.Password;
                }
                else
                {
                    opts.UserName = Credentials.UserName;
                    opts.Password = Credentials.Password;
                }
                if (opts.UserName == null || opts.Password == null)
                {
                    throw new ApplicationException("You must provide the username and password on the command line because these are not currently saved");
                }

                var session = IodClientMethods.DoLogin(opts.UserName, opts.Password).Result;
                var isLoggedIn = session.IsConnected;
                if (isLoggedIn)
                {
                    string taskId = IodClientMethods.DoGetTaskId(session, opts.TaskName, opts.TaskType).Result;

                    if (opts.DoRun)
                    {
                        var resp = IodClientMethods.DoStartTask(session, taskId, opts.TaskType).Result;
                    }

                    if (opts.DoWait)
                    {
                        var resp = IodClientMethods.DoWaitTask(session, taskId, opts.TaskType).Result;
                    }

                    if (!String.IsNullOrWhiteSpace(opts.OutputTo))
                    {

                        var logEntries = IodClientMethods.DoGetActivityLog(session, taskId).Result;
                        var cnt = logEntries.Count();
                        var monitorEntries = IodClientMethods.DoGetActivityMonitor(session).Result;
                        var report = LogReport.MakeLogReport(monitorEntries, logEntries);
                        FileHelper.MakeCsv(report, opts.OutputTo);
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("**************************************");
                Console.WriteLine(ex);
                Console.WriteLine("**************************************");
            }
            finally
            {
                #if DEBUG
                System.Diagnostics.Debugger.Break();
                #endif
            }
        }
    }
    
}

