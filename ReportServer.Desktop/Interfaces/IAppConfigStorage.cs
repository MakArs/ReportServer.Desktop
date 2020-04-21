using System.Threading.Tasks;
using ReportServer.Desktop.Models;

namespace ReportServer.Desktop.Interfaces
{
    public interface IAppConfigStorage
    {
        Task Save(AppConfig value);
        Task<AppConfig> Load();
    }
}
