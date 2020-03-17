using Aliyun.Api.LogService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Aliyun.Api.LogService.Infrastructure.Protocol.Http;
using Serilog.Sinks.AliyunLog.Model;
using Xunit;

namespace Serilog.Sinks.AliyunLog.Tests
{

    public class AliyunLogSinkTests
    {
        [Fact]
        public void WrittenToTheAliyunLog()
        {
            var logger= BuildLoggerConfiguration().CreateLogger();
            logger.Information("Test");
        }


        public static LoggerConfiguration BuildLoggerConfiguration()
        {
            string templateWithSource = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {SourceContext} {CorrelationId} {Message}{NewLine}{Exception}";

            var loggerConfiguration = new LoggerConfiguration();
            var aliLogConfig = new AliLogConfig()
            {
                AccessKey = "accessKeyId",
                AccessKeySecret = "accessKey",
                Endpoint = "https://cn-qingdao.log.aliyuncs.com",
                Project = "serilog-aliyunlog-test",
                LogStoreName = "LogStoreName",
            };
            if (aliLogConfig.IsValid())
            {
                var aliLogClient = GetAliLogServiceClient(aliLogConfig);
                loggerConfiguration.WriteTo.AliyunLog(aliLogClient, aliLogConfig.LogStoreName, aliLogConfig.Project, outputTemplate: templateWithSource);
                Console.WriteLine("加载阿里云日志配置成功");
            }
            else
            {
                Console.WriteLine("无效的阿里云日志配置文件");
            }

            return loggerConfiguration;
        }


        public static HttpLogServiceClient GetAliLogServiceClient(AliLogConfig config)
        {
            var aliLogClient = LogServiceClientBuilders
                .HttpBuilder
                .Endpoint(config.Endpoint, config.Project)
                .Credential(config.AccessKey, config.AccessKeySecret)
                .Build();
            return aliLogClient;
        }

    }
}
