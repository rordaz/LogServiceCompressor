using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading.Tasks;

using Quartz;
using Quartz.Impl;

namespace LogFilesServiceCompressor
{
    public class ScheduleService
    {
        private readonly IScheduler scheduler;
        private DateTime _scheduleStartTime;


        public ScheduleService()
        {
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" },
                { "quartz.scheduler.instanceName", "MyScheduler" },
                { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                { "quartz.threadPool.threadCount", "3" }
            };

            // Read the parameters
            if (ConfigurationManager.AppSettings.Get("ScheduleStartTime") != null && !ConfigurationManager.AppSettings.Get("ScheduleStartTime").ToString().Equals(""))
                DateTime.TryParse(ConfigurationManager.AppSettings.Get("ScheduleStartTime"), out _scheduleStartTime);
            else
                return;

            // Grab the Scheduler instance from the Factory
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            scheduler = factory.GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public DateTime ScheduleStartTime { get => _scheduleStartTime; set => _scheduleStartTime = value; }

        public void Start()
        {
            scheduler.Start().ConfigureAwait(false).GetAwaiter().GetResult();
            ScheduleJobs();
        }

        public void Stop()
        {
            scheduler.Shutdown().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void ScheduleJobs()
        {
            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<ZipJob>()
                .WithIdentity("job1", "group1")
                .Build();

            #if DEBUG
                ScheduleStartTime = DateTime.Now.AddMinutes(1);
                //System.IO.File.Delete(@"C:\apps\sample1\zips\LogZipFile_2020_10.zip");
           #endif

            Console.Out.WriteLine("Next Run at " + ScheduleStartTime.ToShortTimeString());
            LogHelper.Info("Next Run at " + ScheduleStartTime.ToShortTimeString());

            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(ScheduleStartTime.Hour, ScheduleStartTime.Minute))
                .Build();

            // Tell quartz to schedule the job using our trigger
            // await scheduler.ScheduleJob(job, trigger);
            scheduler.ScheduleJob(job, trigger).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

    public class ZipJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            //await Console.Out.WriteLineAsync("Greetings from my First Quartz Application!");
            LogHelper.Info("ZipJob Task Starting");
            await Console.Out.WriteLineAsync("[" + DateTime.Now.ToLongTimeString() + "] Running ZipJob");
            // CALL THIS TO INITIATE A ZIP
            LogArchiver logArchiver = new LogArchiver();
            logArchiver.ZipLogs();
            
            Console.Out.WriteLine("Zip Compression completed");
            LogHelper.Info("Zip Compression completed");
            
            if (logArchiver.RemoveOriginalFiles)
            {
                LogHelper.Info("Delete Original Files Initiated");
                Console.Out.WriteLine("Delete Original Files Initiated");
                ZipUtil.DeleteOriginalFiles(logArchiver.SourceLogFiles, logArchiver);
            }
           
            LogHelper.Info("ZipJob Task Completed");
            Console.Out.WriteLine("ZipJob Task Completed");

        }
    }
}
