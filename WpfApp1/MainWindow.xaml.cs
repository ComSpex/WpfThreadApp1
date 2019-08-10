using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfApp1 {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow:Window {
		Timer tick;
		delegate void ThreadSafe1(object arg);
		public MainWindow() {
			InitializeComponent();
			CreateOneThread(this.face);
		}
		private void CreateOneThread(object arg) {
			if(tick!=null) {
				TextBlock tb = arg as TextBlock;
				if(!tb.Dispatcher.CheckAccess()) {
					try {
						Dispatcher.Invoke(() => CreateOneThread(arg));
					} catch { }
				} else {
					tb.Text=DateTime.Now.ToString("HH:mm:ss.fff");
				}
				//Thread.Sleep(0);
				DoEvents();
			}
			tick=new Timer((o) => {
				TextBlock tb = o as TextBlock;
				if(!tb.Dispatcher.CheckAccess()) {
					try {
						Dispatcher.Invoke(() => CreateOneThread(arg));
					} catch { }
				} else {
					tb.Text=DateTime.Now.ToString("HH:mm:ss.fff");
				}
			},arg,1000,10);
		}
		public void DoEvents() {
			DispatcherFrame frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
				new DispatcherOperationCallback((o)=> {
					((DispatcherFrame)o).Continue=false;
					return null;
				}),frame);
			Dispatcher.PushFrame(frame);
		}
	}
}
