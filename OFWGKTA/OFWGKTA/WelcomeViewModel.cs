﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using NAudio.Wave;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using Microsoft.Win32;
using System.IO;
using Microsoft.Speech.Recognition;

namespace OFWGKTA 
{
    class WelcomeViewModel : ViewModelBase, IView
    {
        private AppState appState;
        private AppState AppState
        {
            get
            {
                // appState is set lazily
                if (this.appState != null)
                    return this.appState;

                // try to get Kinect reference and instantiate its model
                KinectModel kinectModel = null;
                SpeechRecognizer speechRecognizer = null;
                try
                {
                    List<string> list = new List<string> {"record", "play", "stop"};
                    speechRecognizer = new SpeechRecognizer(list, null);
                    kinectModel = new FreePlayKinectModel(null);
                }
                catch 
                {
                    // TODO: alert the user that no Kinect was detected
                }

                // mic interface is hardcoded to 0 here:
                this.appState = new AppState(kinectModel, speechRecognizer, 1);
                return this.appState;
            }
        }

        public const string ViewName = "WelcomeView";

        // Instance variables
        private ObservableCollection<string> applicationModes;
        private int selectedApplicationModeIndex;

        // Commands
        private ICommand continueCommand;

        public WelcomeViewModel()
        {
            this.applicationModes = new ObservableCollection<string>();
            this.continueCommand = new RelayCommand(() => MoveToHomeScreen());
        }

        public ICommand ContinueCommand { get { return continueCommand; } }

        #region de/activated
        public void Activated(object state)
        {
            this.applicationModes.Clear();
            this.applicationModes.Add("Start Application");
        }

        public void Deactivated() { }
        #endregion

        public ObservableCollection<string> ApplicationModes{ get { return applicationModes; } }

        private void MoveToHomeScreen()
        {
            // Selected index must be valid
            // TODO: either default the selected index when the user returns to the home screen, or prompt them to select an option if SelectedIndex is invalid
            if (SelectedIndex < 0 || SelectedIndex > this.applicationModes.Count)
                return;

            switch (this.applicationModes[SelectedIndex])
            {
                case ("Settings"):
                    {
                        Stream fileStream;
                        OpenFileDialog openFileDialog = new OpenFileDialog { };
                        openFileDialog.ShowDialog();
                        try
                        {
                            fileStream = File.OpenRead(openFileDialog.FileName);
                            var curState = new AppState(new ReplayKinectModel(fileStream), null, 0);
                            Messenger.Default.Send(new NavigateMessage(SettingsViewModel.ViewName, curState));
                        }
                     catch { }
                        break;
                    }
                case ("Replay"):
                    {
                        Stream fileStream;
                        OpenFileDialog openFileDialog = new OpenFileDialog { };
                        openFileDialog.ShowDialog();
                        try
                        {
                            fileStream = File.OpenRead(openFileDialog.FileName);
                            var curState = new DemoAppState(this.applicationModes[SelectedIndex], new ReplayKinectModel(fileStream));
                            Messenger.Default.Send(new NavigateMessage(DemoViewModel.ViewName, curState));
                        }
                     catch { }
                        break;
                    }
                case ("Record"):
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog { };
                        saveFileDialog.ShowDialog();
                        Stream fileStream;
                        try
                        {
                            fileStream = File.OpenWrite(saveFileDialog.FileName);
                            var curState = new DemoAppState(this.applicationModes[SelectedIndex], new FreePlayKinectModel(fileStream));
                            Messenger.Default.Send(new NavigateMessage(DemoViewModel.ViewName, curState));
                        }
                        catch { }
                        break;
                    }
                case ("Free Use"):
                    {
                        var curState = new DemoAppState(this.applicationModes[SelectedIndex], new FreePlayKinectModel(null));
                        Messenger.Default.Send(new NavigateMessage(DemoViewModel.ViewName, curState));
                        break;
                    }
                case ("Audio App"):
                    {
                        Messenger.Default.Send(new NavigateMessage(HomeViewModel.ViewName, this.AppState));
                        break;
                    }
                case ("Start Application"):
                    {
                        Messenger.Default.Send(new NavigateMessage(MicRecordViewModel.ViewName, this.AppState));
                        break;
                    }
                case ("Fancy Graph"):
                    {
                        Messenger.Default.Send(new NavigateMessage(FancyGraphViewModel.ViewName, 0));
                        break;
                    }
            }
        }
        
        public int SelectedIndex
        {
            get
            {
                return selectedApplicationModeIndex;
            }
            set
            {
                if (selectedApplicationModeIndex != value)
                {
                    selectedApplicationModeIndex = value;
                    RaisePropertyChanged("SelectedIndex");
                }
            }
        }
    }
}
