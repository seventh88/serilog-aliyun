namespace Serilog.Sinks.AliyunLog.Model
{
    public class AliLogConfig
    {
        public string AccessKey { get; set; }

        public string AccessKeySecret { get; set; }

        public string Endpoint { get; set; }

        public string Project { get; set; }

        public string LogStoreName { get; set; }

        public bool IsValid()
        {
            return AccessKey != null && AccessKeySecret != null && Endpoint != null && Project != null &&
                   LogStoreName != null;
        }
    }
}
