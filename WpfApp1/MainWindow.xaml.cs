using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Timers;
using TimerTimer = System.Timers.Timer;
using Timer = System.Threading.Timer;
using System.Threading.Tasks;

namespace WpfApp1 {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow:Window {
		Timer tick;
		TimerTimer tack;
		const int interval = 10;
		delegate void ThreadSafe1(object arg);
		CancellationToken cancel = CancellationToken.None;
		public MainWindow() {
			InitializeComponent();
			//CreateTimerOne(this.face);
			CreateTimerTwoAsync(this.face);
		}
		private async void CreateTimerTwoAsync(object arg) {
			if(tack!=null) {
				TextBlock tb = arg as TextBlock;
				if(!tb.Dispatcher.CheckAccess()) {
					try {
						Dispatcher.Invoke(() => CreateTimerTwoAsync(arg));
					} catch { }
				} else {
					tb.Text=DateTime.Now.ToString("HH:mm:ss.fff");
				}
				DoEvents();
			} else {
#if false
				tack=new TimerTimer();
				tack.Interval=interval;
				tack.Elapsed+=Tack_Elapsed;
				tack.Start();
#else
				await DoUIThreadWorkAsync(cancel);
#endif
			}
		}
		private void Tack_Elapsed(object sender,ElapsedEventArgs e) {
			TextBlock tb = this.face;
			if(!tb.Dispatcher.CheckAccess()) {
				try {
					Dispatcher.Invoke(() => CreateTimerTwoAsync(tb));
				} catch { }
			} else {
				tb.Text=DateTime.Now.ToString("HH:mm:ss.fff");
			}
		}
		private void CreateTimerOne(object arg) {
			if(tick!=null) {
				TextBlock tb = arg as TextBlock;
				if(!tb.Dispatcher.CheckAccess()) {
					try {
						Dispatcher.Invoke(() => CreateTimerOne(arg));
					} catch { }
				} else {
					tb.Text=DateTime.Now.ToString("HH:mm:ss.fff");
				}
				DoEvents();
			} else {
				tick=new Timer((o) => {
					TextBlock tb = o as TextBlock;
					if(!tb.Dispatcher.CheckAccess()) {
						try {
							Dispatcher.Invoke(() => CreateTimerOne(arg));
						} catch { }
					} else {
						tb.Text=DateTime.Now.ToString("HH:mm:ss.fff");
					}
				},arg,1000,interval);
			}
		}
		public void DoEvents() {
			Thread.Sleep(0);
			DispatcherFrame frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
				new DispatcherOperationCallback((o) => {
					((DispatcherFrame)o).Continue=false;
					return null;
				}),frame);
			Dispatcher.PushFrame(frame);
		}
		async Task DoUIThreadWorkAsync(CancellationToken token) {
			Func<Task> idleYield = async () => await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

			var cancellationTcs = new TaskCompletionSource<bool>();
			using(token.Register(() => cancellationTcs.SetCanceled(),useSynchronizationContext: true)) {
				while(true) {
					await Task.Delay(100,token);
					await Task.WhenAny(idleYield(),cancellationTcs.Task);
					token.ThrowIfCancellationRequested();

					// do the UI-related work
					face.Text=DateTime.Now.ToString("HH:mm:ss.fff");
				}

			}
		}
		private void Button_Click(object sender,RoutedEventArgs e) {
			if(MessageBoxResult.OK==MessageBox.Show("Are you sure to terminate this program?",this.Title,MessageBoxButton.OKCancel,MessageBoxImage.Question)) {
				Close();
			}
		}
	}
}
