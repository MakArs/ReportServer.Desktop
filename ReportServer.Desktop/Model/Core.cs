using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReportServer.Desktop.Interfaces;

namespace ReportServer.Desktop.Model
{
    public class Core : ICore
    {
        private readonly ObservableCollection<ApiTaskCompact> _taskCompacts;
        private readonly ObservableCollection<ApiInstanceCompact> _instanceCompacts;
        private readonly IReportService _reportService;

        public ApiInstance SelectedInstance { get; set; }
        public ApiTask SelectedTask { get; set; }


        public Core(IReportService reportService)
        {
            _reportService = reportService;
            _taskCompacts=new ObservableCollection<ApiTaskCompact>();
            _instanceCompacts=new ObservableCollection<ApiInstanceCompact>();
            LoadTaskCompacts();
            LoadInstanceCompacts();
        }

        public void LoadTaskCompacts()
        {
            var taskList = _reportService.LoadAllTaskCompacts();
            lock (this)
            {
                _taskCompacts.Clear();
                foreach (var task in taskList)
                    _taskCompacts.Add(task);
            }
        }

        public void LoadInstanceCompacts()
        {
            var instanceList = _reportService.LoadInstanceCompacts();
            lock (this)
            {
                _instanceCompacts.Clear();
                foreach (var instance in instanceList)
                    _instanceCompacts.Add(instance);
            }
        }

        public void LoadSelectedTaskById(int id)
        {
            SelectedTask = _reportService.LoadTaskById(id);
        }

        public void LoadSelectedInstanceById(int id)
        {
            throw new NotImplementedException();
        }

        public void LoadInstanceCompactsByTaskId(int taskId)
        {
            throw new NotImplementedException();
        }
    }
}
