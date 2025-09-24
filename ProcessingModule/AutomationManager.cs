using Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for automated work.
    /// </summary>
    public class AutomationManager : IAutomationManager, IDisposable
	{
		private Thread automationWorker;
        private AutoResetEvent automationTrigger;
        private IStorage storage;
		private IProcessingManager processingManager;
		private int delayBetweenCommands;
        private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationManager"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="processingManager">The processing manager.</param>
        /// <param name="automationTrigger">The automation trigger.</param>
        /// <param name="configuration">The configuration.</param>
        public AutomationManager(IStorage storage, IProcessingManager processingManager, AutoResetEvent automationTrigger, IConfiguration configuration)
		{
			this.storage = storage;
			this.processingManager = processingManager;
            this.configuration = configuration;
            this.automationTrigger = automationTrigger;
        }

        /// <summary>
        /// Initializes and starts the threads.
        /// </summary>
		private void InitializeAndStartThreads()
		{
			InitializeAutomationWorkerThread();
			StartAutomationWorkerThread();
		}

        /// <summary>
        /// Initializes the automation worker thread.
        /// </summary>
		private void InitializeAutomationWorkerThread()
		{
			automationWorker = new Thread(AutomationWorker_DoWork);
			automationWorker.Name = "Aumation Thread";
		}

        /// <summary>
        /// Starts the automation worker thread.
        /// </summary>
		private void StartAutomationWorkerThread()
		{
			automationWorker.Start();
		}


		private void AutomationWorker_DoWork()
		{
			EGUConverter eguConverter = new EGUConverter();
			PointIdentifier analogOut = new PointIdentifier(PointType.ANALOG_OUTPUT, 1000);
			PointIdentifier digitalOn = new PointIdentifier(PointType.DIGITAL_OUTPUT, 3000);
			PointIdentifier digitalOff = new PointIdentifier(PointType.DIGITAL_OUTPUT, 3001);
			PointIdentifier digitalObstacle = new PointIdentifier(PointType.DIGITAL_INPUT, 2000);
            List<PointIdentifier> pointList = new List<PointIdentifier> { analogOut, digitalOn, digitalOff,digitalObstacle };
			while (!disposedValue)
			{
				List<IPoint> points = storage.GetPoints(pointList);
				int initValue = (int)eguConverter.ConvertToRaw(points[0].ConfigItem.ScaleFactor, points[0].ConfigItem.Deviation, points[0].RawValue);
				int value = initValue;

				int OnMoveInit = (int)eguConverter.ConvertToRaw(points[1].ConfigItem.ScaleFactor, points[1].ConfigItem.Deviation, points[1].ConfigItem.DefaultValue);
				int OnCloseInit = (int)eguConverter.ConvertToRaw(points[2].ConfigItem.ScaleFactor, points[2].ConfigItem.Deviation, points[2].ConfigItem.DefaultValue);

				int OnMove = OnMoveInit;
				int OnClose = OnCloseInit;

				if (points[1].RawValue == 1)
				{
					value += 10;
				}
				if (points[2].RawValue == 1)
				{
					value -= 10;
				}
				if (points[3].RawValue == 1)
				{
					OnClose = 1;
					OnMove = 0;
				}
				if (value != initValue)
				{
					value = (int)eguConverter.ConvertToRaw(points[0].ConfigItem.ScaleFactor, points[0].ConfigItem.Deviation, eguConverter.ConvertToEGU(points[0].ConfigItem.ScaleFactor, points[0].ConfigItem.Deviation, (ushort)value));
					processingManager.ExecuteWriteCommand(points[0].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 1000, value);
				}
				if (OnMove != OnMoveInit)
				{
                    processingManager.ExecuteWriteCommand(points[1].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 3000, OnClose);
                }
				if (OnClose != OnCloseInit)
				{
					processingManager.ExecuteWriteCommand(points[2].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 3001, OnClose);
				}

				if (value > points[0].ConfigItem.HighLimit) {
					if (points[0].RawValue != 600 && points[0].RawValue < 600)
                    processingManager.ExecuteWriteCommand(points[1].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 3000, 0);
                }
				if (value < points[0].ConfigItem.LowLimit)
				{
					if (points[0].RawValue != 20 && points[0].RawValue >20)
                    processingManager.ExecuteWriteCommand(points[2].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 3001, 0);
                }

				automationTrigger.WaitOne(delayBetweenCommands);
            }
        }

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls


        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">Indication if managed objects should be disposed.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}
				disposedValue = true;
			}
		}


		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// GC.SuppressFinalize(this);
		}

        /// <inheritdoc />
        public void Start(int delayBetweenCommands)
		{
			this.delayBetweenCommands = delayBetweenCommands*1000;
            InitializeAndStartThreads();
		}

        /// <inheritdoc />
        public void Stop()
		{
			Dispose();
		}
		#endregion
	}
}
