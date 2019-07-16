using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Skywalker.UserInterface {
	/// <summary>
	/// Interaction logic for Calibration.xaml
	/// </summary>
	public partial class Calibration : UserControl {

		private int[] calibrationResults;
		public Calibration() {
			InitializeComponent();
		}
		
		private void Run_Test_Click(object sender, RoutedEventArgs e) {
			BackgroundWorker worker = new BackgroundWorker(); //does work in background (duh)
			worker.DoWork += runTest;
			worker.RunWorkerCompleted += calibrationDone;
			worker.RunWorkerAsync();
		}

		private void runTest(object sender, DoWorkEventArgs e) {
			calibrationResults = Data.Navigator.MotorCalibrationRun();
		}
		private void calibrationDone(object sender, RunWorkerCompletedEventArgs e) {
			BeforeTest.Text = calibrationResults[0].ToString();
			AfterTest.Text = calibrationResults[1].ToString();
			DegreeChange.Text = calibrationResults[2].ToString();
			OffetValue.Text = Properties.Settings.Default.CalibrationOffset.ToString();
		}

		
		/*		private void Button_Click(object sender, RoutedEventArgs e) {
			double originalDegrees = CurrentPose.CurrentDirection;
			U.Text = originalDegrees.ToString();
			
			//double endDegrees = Data.Navigator.Wat();
			//Thread.Sleep(5000);
//			double endDegrees = CurrentPose.CurrentDirection;
		//	V.Text = endDegrees.ToString();
//			Data.Navigator.setCurrentDriveType(1);
//			Thread t = new Thread(lolWork);
//			t.Start();
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += lolWork;
			worker.RunWorkerCompleted += workDone;
			worker.RunWorkerAsync();
		}

		private double WAT;
		private void lolWork(object sender, DoWorkEventArgs e) {
			WAT = Data.Navigator.MotorCalibrationRun();
		}
		private void workDone(object sender, RunWorkerCompletedEventArgs e) {
			V.Text = WAT.ToString();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e) {
			WatBox.Text = Data.Navigator.GetCalibrationResult().ToString();

		}
*/
	}
}
