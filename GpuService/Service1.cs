using NvAPIWrapper;
using NvAPIWrapper.GPU;
using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace GpuService
{
    // partial, чтобы связать с Service1.designer.cs
    public partial class Service1 : ServiceBase, IDisposable
    {
        // Публичные свойства для чтения из WPF-интерфейса
        public string GpuName { get; private set; } = "Инициализация...";
        public uint GpuClock { get; private set; }
        public string ArchInfo{ get; private set; }
        public uint MemClock { get; private set; }

        private CancellationTokenSource _cts;

        string[] ArcinfoBlocks = new string[6];

        public Service1()
        {
          
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            StartMonitoring();
        }

        protected override void OnStop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            try { NVIDIA.Unload(); } catch { }
        }

        public void StartMonitoring()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            Task.Run(() => GetData(_cts.Token));
        }

        private async Task GetData(CancellationToken ct)
        {
            PhysicalGPU mainGpu = null;



            while (!ct.IsCancellationRequested && mainGpu == null)
            {
                try
                {
                    NVIDIA.Initialize();
                    var gpus = PhysicalGPU.GetPhysicalGPUs();
                    if (gpus != null && gpus.Length > 0)
                    {
                        mainGpu = gpus[0];
                        GpuName = mainGpu.FullName;
                        ArchInfoToArray(mainGpu);
                        
                        break;
                    }
                }
                catch
                {
                    // В службах ошибки пишутся в системный EventLog
                }
                await Task.Delay(3000, ct);
            }

            while (!ct.IsCancellationRequested)
            {

                
                try
                {
                    var frequencies = mainGpu.CurrentClockFrequencies;
                    GpuClock = frequencies.GraphicsClock.Frequency / 1000;
                    MemClock = frequencies.MemoryClock.Frequency / 1000;
                   

                    await Task.Delay(1000, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    await Task.Delay(3000, ct);
                }
            }
        }

        private void ArchInfoToArray(PhysicalGPU gpu)
        {
            ArcinfoBlocks[0] = gpu.ArchitectInformation.NumberOfROPs.ToString();
            ArcinfoBlocks[1] = gpu.ArchitectInformation.TotalNumberOfSMs.ToString();
            ArcinfoBlocks[2] = gpu.ArchitectInformation.NumberOfCores.ToString();
            ArcinfoBlocks[3] = gpu.ArchitectInformation.NumberOfGPC.ToString();
            ArcinfoBlocks[4] = gpu.ArchitectInformation.NumberOfShaderPipelines.ToString();
            ArcinfoBlocks[5] = gpu.ArchitectInformation.Revision.ToString();
        }
    }
}
