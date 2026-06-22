using NvAPIWrapper;
using NvAPIWrapper.GPU;
using System;
using System.ServiceProcess; // Обязательно для служб
using System.Threading;
using System.Threading.Tasks;

namespace GpuService
{
    partial class Service1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.CanShutdown = true;
            this.ServiceName = "Service1";
        }
    }
}
