using System.Threading.Tasks;

namespace ReportServer.Desktop.ViewModel
{
    public interface ISaveableViewModel
    {
        Task Save();
    }
}