using GpuService;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GpuServiceInterface
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            // Безопасное уведомление UI-потока WPF из фонового цикла
            Dispatcher.BeginInvoke(new Action(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name))));
        }

        // Создаем экземпляр службы для сбора данных
        private Service1 _winService = new Service1();

        // Это свойство-мост. Оно возвращает текущее окно 
        public MainWindow GpuService => this;

        private uint _gpuClock;
        private uint _memClock;
        private string _gpuName = "Служба запускается...";
        private string _archinfo;

        public uint GpuClock
        {
            get => _gpuClock;
            set { if (_gpuClock != value) { _gpuClock = value; OnPropertyChanged(); } }
        }

        public uint MemClock
        {
            get => _memClock;
            set { if (_memClock != value) { _memClock = value; OnPropertyChanged(); } }
        }

        public string GpuName
        {
            get => _gpuName;
            set { if (_gpuName != value) { _gpuName = value; OnPropertyChanged(); } }
        }

        public string ArchInfo
        {
            get => _archinfo;
            set { if (_archinfo != value) { _archinfo = value; OnPropertyChanged(); } }
        }


        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();

            // Привязываем контекст данных к этому окну
            this.DataContext = this;

            _cts = new CancellationTokenSource();

            // Запускаем фоновый мониторинг внутри класса службы
            _winService.StartMonitoring();

            // Запускаем локальный цикл опроса, который переносит данные в UI
            _ = StartLocalPolling(_cts.Token);
        }

        private async Task StartLocalPolling(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // Копируем актуальные данные из полей Windows-службы в свойства UI
                    GpuName = _winService.GpuName;
                    GpuClock = _winService.GpuClock;
                    MemClock = _winService.MemClock;
                    ArchInfo = _winService.ArchInfo;
                }
                catch { }

                // Опрашиваем объект службы ровно раз в секунду
                await Task.Delay(1000, ct);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Гарантированно глушим потоки, чтобы приложение не зависло в процессах
            _cts?.Cancel();
            _winService?.Dispose();
            base.OnClosed(e);
        }
    }
}
