using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Skywalker.Driver;
using Skywalker.Input;
using System.Threading;
using Skywalker_Vision.Kinect;
using TETCSharpClient;

namespace Skywalker.UserInterface {
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Settings : UserControl {
        List<string> stringNames = new List<string>();

        public Settings() {
            //initialize default values
            Data._GazeDelay = 2000;

            //initialize IImageStreams
            //initialize streams
            Data._streams = new List<IImageStream>(5);
            Data._streams.Add(VideoStream.Instance);
            Data._streams.Add(DepthStream.Instance);
            Data._streams.Add(InfraredStream.Instance);
            Data._streams.Add(CustomStream.GetEdgeViewInstance());
            Data._streams.Add(CustomStream.GetHoughLineInstance());

            //set initial background stream
            Data._BackgroundStream = Data.streams[0];

            InitializeComponent();

            GazeDelaySlider.Value = Data.GazeDelay/1000;
            GazeDelayValue.Content = Data.GazeDelay/1000;

            if (!Data.XboxConnected)
            {
                controller.IsEnabled = false;
            }

            if (!Data.EyeTribeConnected)
            {
                eyenav.IsEnabled = false;
            }

            for (int i = Data.streams.Count -1; i >= 0; i--)
            {
                stringNames.Insert(0, Data.streams[i].GetName());
            }

            //configure delay slider
            GazeDelaySlider.Minimum = 0.1;
            GazeDelaySlider.Maximum = 5;

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (radioButton == null) {
                //do nothing?
            } else if (radioButton.Name == "keyboard") {
                Input.Input.RequestChange(this, "Keyboard");
            } else if (radioButton.Name == "controller") {
                Input.Input.RequestChange(this, "Controller");
            } else if (radioButton.Name == "eyenav") {
                // How do I do this?
                //MainWindow.EnableEyeTribe();
                //Fire an event to the MainWindow class to enable the gaze listener
                Input.Input.RequestChange(this, "EyeTribe");
                Data._SmoothMouse = new SmoothMouse(10);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var combo = sender as ComboBox;
            string selected = combo.SelectedItem as string;

            IImageStream selectedStream = Data.streams.Find(x => x.GetName() == selected);
            //set stream
            Data._BackgroundStream = selectedStream;
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            combo.ItemsSource = stringNames;

            //check which is the current stream in the VisualInputGrid(?)
            //change to that
            int streamIndex = Data.streams.IndexOf(Data.BackgroundStream);

            //for now
            combo.SelectedIndex = streamIndex;

        }

        private void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            Data._GazeDelay = (int)slider.Value * 1000;
            GazeDelayValue.Content = Math.Round(slider.Value, 1) + " seconds";
        }
    }
}
