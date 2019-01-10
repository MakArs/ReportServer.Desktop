using System;
using System.Collections.Generic;
using System.Linq;
using ReportService;

namespace ReportServer.Desktop.Entities
{
    public class ProtoPackageBuilder
    {
        private object GetFromVariantValue(ColumnInfo info, VariantValue value)
        {
            if (info.Nullable && value.IsNull)
                return null;

            switch (info.Type)
            {
                case ScalarType.Int32:
                    return value.Int32Value;

                case ScalarType.Int16:
                    return value.Int16Value;

                case ScalarType.Int8:
                    return value.Int8Value;

                case ScalarType.Double:
                    return value.DoubleValue;

                case ScalarType.Decimal:
                    return value.DecimalValue;

                case ScalarType.Int64:
                    return value.Int64Value;

                case ScalarType.Bool:
                    return value.BoolValue;

                case ScalarType.String:
                    return value.StringValue;

                case ScalarType.Bytes:
                    return value.BytesValue.ToByteArray();

                case ScalarType.DateTime:
                    return DateTimeOffset
                        .FromUnixTimeSeconds(value.DateTime);

                case ScalarType.DateTimeOffset:
                    return DateTimeOffset
                        .FromUnixTimeMilliseconds(value.DateTimeOffsetValue).UtcDateTime;

                case ScalarType.TimeSpan:
                    return new TimeSpan(value.TimeSpanValue);
            }

            return null;
        }

        public List<DataSetContent> GetPackageValues(OperationPackage package)
        {
            var allContent = new List<DataSetContent>();

                foreach (var set in package.DataSets)
                {
                    var setHeaders = set.Columns.Select(col => col.Name).ToList();
                    var setRows = new List<List<object>>();

                    foreach (var row in set.Rows)
                    {
                        var rowValues = new List<object>();

                        for (int i = 0; i < set.Columns.Count; i++)
                        {
                            var colInfo = set.Columns[i];
                            var varValue = row.Values[i];

                            rowValues.Add(GetFromVariantValue(colInfo, varValue));
                        }

                        setRows.Add(rowValues);
                    }

                    allContent.Add(new DataSetContent
                    {
                        Headers = setHeaders,
                        Rows = setRows,
                        Name = set.Name
                    });
                }

            return allContent;
        }
    }

    public class DataSetContent
    {
        public string Name;
        public List<string> Headers;
        public List<List<object>> Rows;
    }
}