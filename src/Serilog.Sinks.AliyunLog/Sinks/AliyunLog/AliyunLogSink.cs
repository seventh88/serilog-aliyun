using Aliyun.Api.LogService;
using Aliyun.Api.LogService.Domain.Log;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog
{
    class AliyunLogSink : PeriodicBatchingSink
    {

        /// <summary>
        /// A reasonable default for the number of events posted in
        /// each batch.
        /// </summary>
        public const int DefaultBatchPostingLimit = 100;

        /// <summary>
        /// A reasonable default time to wait between checking for event batches.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(30);

        private readonly ILogServiceClient logServiceClient;
        private readonly ITextFormatter formatter;
        private readonly string logstoreName;
        private readonly string project;
        private readonly string source;
        private readonly string topic;
        private readonly IDictionary<string, string> logTags;

        public AliyunLogSink(
            ILogServiceClient logServiceClient,
            ITextFormatter formatter,
            string logstoreName = null,
            string project = null,
            string source = null,
            string topic = null,
            IDictionary<string, string> logTags = null,
            int batchSizeLimit = DefaultBatchPostingLimit,
            TimeSpan period = default) : base(batchSizeLimit, period == default ? DefaultPeriod : period)
        {
            this.logServiceClient = logServiceClient ?? throw new ArgumentNullException(nameof(logServiceClient));
            this.formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            this.logstoreName = logstoreName ?? throw new ArgumentNullException(nameof(logstoreName));
            this.project = project;
            this.source = source ?? "127.0.0.1";
            this.topic = topic ?? nameof(AliyunLogSink);
            this.logTags = logTags;
        }

        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }
            var logGroupInfo = new LogGroupInfo
            {
                Source = source,
                Topic = topic,
            };
            if (logTags != null)
            {
                logGroupInfo.LogTags = logTags;
            }
            foreach (var logEvent in events)
            {
                var contents = new Dictionary<string, string>(logEvent.Properties.ToDictionary(k => k.Key, v => v.Value.ToString()))
                {
                    { "Level", logEvent.Level.ToString() }
                };

                if (logEvent.Exception != null)
                {
                    contents.Add("Exception", logEvent.Exception.ToString());
                }

                var stringBuilder = new StringBuilder();
                using (var stringWriter = new StringWriter(stringBuilder))
                {
                    this.formatter.Format(logEvent, stringWriter);
                }
                contents.Add("Message", stringBuilder.ToString());

                logGroupInfo.Logs.Add(new LogInfo
                {
                    Time = logEvent.Timestamp,
                    Contents = contents
                });
            }

            var response = await logServiceClient.PostLogStoreLogsAsync(this.logstoreName, logGroupInfo, hashKey: Guid.NewGuid().ToString("N"), project: project);
            if (!response.IsSuccess)
            {
                Console.WriteLine("阿里云日志发送失败");
                //REDO
            }
        }
    }
}
