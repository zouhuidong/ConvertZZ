using System.ComponentModel;
using System.Windows;
namespace ConvertZZ
{
	/// <summary>
	/// Window_OpenCCSetting.xaml 的互動邏輯
	/// </summary>
	public partial class Window_OpenCCSetting : Window
	{
		public Window_OpenCCSetting()
		{
			InitializeComponent();
			base.DataContext = App.Settings.OpenCC_Setting;
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			App.Save();
		}
	}
}
