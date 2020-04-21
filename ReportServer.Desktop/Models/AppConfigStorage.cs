using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nito.AsyncEx;
using ReportServer.Desktop.Interfaces;

namespace ReportServer.Desktop.Models
{
    public class AppConfigStorage : IAppConfigStorage
    {
        private readonly FileInfo _fileInfo;
        private readonly AsyncReaderWriterLock _lock;

        public AppConfigStorage()
        {
            _fileInfo = new FileInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"ReportServer.Desktop\app.config"));
            _fileInfo.Directory?.Create();
            _lock = new AsyncReaderWriterLock();
        }

        public async Task Save(AppConfig value)
        {
            var data = JsonConvert.SerializeObject(value);
            using (await _lock.WriterLockAsync())
            {
                await File.WriteAllTextAsync(_fileInfo.ToString(), data, Encoding.UTF8);
            }
        }

        public async Task<AppConfig> Load()
        {
            if (!_fileInfo.Exists)
            {
                return new AppConfig();
            }

            string data;
            using (await _lock.ReaderLockAsync())
            {
                data = await File.ReadAllTextAsync(_fileInfo.ToString());
            }
            return JsonConvert.DeserializeObject<AppConfig>(data);
        }
    }
}
