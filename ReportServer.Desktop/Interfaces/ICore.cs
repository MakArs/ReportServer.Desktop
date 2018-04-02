using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportServer.Desktop.Interfaces
{


    public interface ICore
    {
        void LoadTaskCompacts();
        void LoadInstanceCompacts();
        void LoadSelectedTaskById(int id);
        void LoadSelectedInstanceById(int id);
        void LoadInstanceCompactsByTaskId(int taskId);
    }
}
