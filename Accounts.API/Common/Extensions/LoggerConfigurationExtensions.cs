using Serilog.Configuration;
using Serilog.Sinks.Oracle.Batch;
using Serilog.Sinks.Oracle.Columns;
using Serilog;
using System.Collections.Generic;
using System.Data;

namespace Accounts.API.Common.Extensions
{
    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration Oracle(this LoggerSinkConfiguration loggerConfiguration, string connectionString, string tableSpaceAndTableName, string tableSpaceAndFunctionName)
        {
            var columnOptions = new ColumnOptions
            {
                AdditionalDataColumns = new List<DataColumn>
                          {
                              new DataColumn("SERVICE" , typeof(string))
                          }
            };
            var sink = new BatchLoggerConfiguration()
                .WithSettings(connectionString, tableSpaceAndTableName, tableSpaceAndFunctionName, columnOptions: columnOptions)
                .UseBurstBatch()
                .CreateSink();
            return loggerConfiguration.Sink(sink);
        }
    }
}
