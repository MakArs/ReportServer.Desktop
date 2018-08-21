using System.Threading.Tasks;

namespace ReportServer.Desktop.Interfaces
{
    public interface ISaveableViewModel
    {
        Task Save();
    }
}