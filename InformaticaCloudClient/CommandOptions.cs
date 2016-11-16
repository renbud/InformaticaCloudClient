using System;
using System.Linq;
using System.Text;
using CommandLine;
using System.IO;

namespace InformaticaCloudClient
{
    class CommandOptions
    {
        [Option('u', "user", Required = false, HelpText = "User login name username@organisation.com")]
        public string UserName { get; set; }
        [Option('p', "password", Required = false, HelpText = "Password for user login")]
        public string Password { get; set; }

        [Option('t', "task", Required = true, HelpText = "Task name")]
        public string TaskName { get; set; }

        [Option('e', "type", Required = true, HelpText = @"Task type: Task type. Use one of the following codes:
AVS = Contact Validation task.
DMASK = Data Masking task.
DNB_TASK = D&B360 task.
DNB_WORKFLOW = D&B360 workflow.
DQA = Data Assessment task.
DRS = Data Replication task.
DSS = Data Synchronization task.
MTT = Mapping Configuration task.
PCS = PowerCenter task.")]
        public string TaskType { get; set; }

        [Option('r', "run", Required = false, Default = false, HelpText = "Run the specified task.")]
        public bool DoRun { get; set; }

        [Option('w', "wait", Required = false, Default = false, HelpText = "Wait for the specified task to finish before returning.")]
        public bool DoWait { get; set; }

        [Option('s', "stop", Required = false, Default = false, HelpText = "Stop the specified task.")]
        public bool DoStop { get; set; }

        [Option('o', "output", Required = false, Default = "x", HelpText = "File name to send log to")]
        public string OutputTo { get; set; }

        public string ParserMessage { get; set; }

        public static CommandOptions ParseCommandLine(string[] args)
        {
            try {
                CommandOptions thisOptions = new CommandOptions();
                var writer = new StringWriter();
                var parser = new Parser(with => with.HelpWriter = writer);
                var options = parser.ParseArguments<CommandOptions>(args);
                ParserResultType tag = options.Tag;

                if (tag == ParserResultType.NotParsed)
                {
                    thisOptions.ParserMessage = writer.ToString();
                    return thisOptions;
                }


                options.WithParsed(opt => thisOptions = opt);  // An arcane way to set thisOptions because the CommandOptions library if going for functional style
                if (!IodClientMethods.TaskTypes.Keys.Contains(thisOptions.TaskType))
                {
                    var help = CommandLine.Text.HelpText.AutoBuild(parser.ParseArguments<CommandOptions>(new[] { "--help" }));
                    thisOptions.ParserMessage = $"Invalid task type -e {thisOptions.TaskType}\n{help}";
                    return thisOptions;
                }
                if ((thisOptions.UserName==null || thisOptions.Password== null) && (thisOptions.UserName != null || thisOptions.Password != null))
                {
                    var help = CommandLine.Text.HelpText.AutoBuild(parser.ParseArguments<CommandOptions>(new[] { "--help" }));
                    thisOptions.ParserMessage = $"UserName and Password must both be specified or neither specified\n{help}";
                    return thisOptions;
                }
                return thisOptions;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error parsing command line arguments: ", ex);
            }
        }

    }
}
