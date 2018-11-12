using System;

namespace ReportServer.Desktop.Entities
{
    public class ApiOperTemplate
    {
        public int Id { get; set; }
        public string ImplementationType { get; set; }
        public string Name { get; set; }
        public string ConfigTemplate { get; set; }
    }

    public class ApiRecepientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }
    }

    public class ApiTelegramChannel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long ChatId { get; set; }
        public int Type { get; set; }
    }

    public class ApiSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }

    public class ApiTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ScheduleId { get; set; }
        public ApiOperation[] BindedOpers { get; set; }
    }

    public class ApiOperation
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string ImplementationType { get; set; }
        public bool IsDefault { get; set; }
        public string Config { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ApiTaskInstance
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
    }

    public class ApiOperInstance
    {
        public int Id { get; set; }
        public int TaskInstanceId { get; set; }
        public int OperationId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public byte[] DataSet { get; set; }
        public string ErrorMessage { get; set; }
        public string OperName { get; set; }
    }
}
