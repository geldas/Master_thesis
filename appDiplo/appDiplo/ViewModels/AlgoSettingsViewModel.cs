using System;
using System.Collections.Generic;
using System.Text;
using appDiplo.Models;
using MvvmHelpers;
using Xamarin.Forms;
using System.ComponentModel;

namespace appDiplo.ViewModels 
{ 
    public class AlgoSettingsViewModel : ObservableObject
    {
        private int antsNumber { get; set; }
        private int antsTime { get; set; }
        private double antsRho { get; set; }
        private double antsPsi { get; set; }
        private double antsQ0 { get; set; }
        private double antsPheromone { get; set; }

        private int ilsTime { get; set; }
        private int ilsThreshold1 { get; set; }
        private int ilsThreshold2 { get; set; }
        private int ilsThreshold3 { get; set; }
        private int ilsF { get; set; }

        private int sailsTime { get; set; }
        private int sailsLimit { get; set; }
        private int sailsThreshold2 { get; set; }
        private int sailsThreshold3 { get; set; }
        private double sailsMaxInnerLoop { get; set; }
        private int sailsF { get; set; }
        private double sailsT0 { get; set; }
        private double sailsAlpha { get; set; }


        public AlgoSettingsViewModel()
        {
            antsNumber = 5;
            antsTime = 30;
            antsRho = 0.05;
            antsPsi = 0.05;
            antsQ0 = 0.9;
            antsPheromone = 5;
            ilsThreshold1 = 10; // 10
            ilsThreshold2 = 20; // ils 15
            ilsThreshold3 = 3; // 3
            ilsF = 2; // 5
            ilsTime = 30;
            sailsThreshold2 = 20; // ils 15
            sailsThreshold3 = 3; // 3
            sailsF = 2; // 5
            sailsTime = 30;
            sailsLimit = 5;
            sailsT0 = 1000;
            sailsAlpha = 0.75; // 0.75
            sailsMaxInnerLoop = 200; // 50

        }

        public int AntsNumber
        {
            get => antsNumber;
            set
            {
                if (antsNumber == value)
                    return;
                antsNumber = value;
                OnPropertyChanged();
            }
        }

        public int AntsTime
        {
            get => antsTime;
            set
            {
                if (antsTime == value)
                    return;
                antsTime = value;
                OnPropertyChanged();
            }
        }

        public double AntsRho
        {
            get => antsRho;
            set
            {
                if (antsRho == value)
                    return;
                antsRho = value;
                OnPropertyChanged();
            }
        }

        public double AntsPsi
        {
            get => antsPsi;
            set
            {
                if (antsPsi == value)
                    return;
                antsPsi = value;
                OnPropertyChanged();
            }
        }

        public double AntsPheromone
        {
            get => antsPheromone;
            set
            {
                if (antsPheromone == value)
                    return;
                antsPheromone = value;
                OnPropertyChanged();
            }
        }

        public double AntsQ0
        {
            get => antsQ0;
            set
            {
                if (antsQ0 == value)
                    return;
                antsQ0 = value;
                OnPropertyChanged();
            }
        }
        public int IlsTime
        {
            get => ilsTime;
            set
            {
                if (ilsTime == value)
                    return;
                ilsTime = value;
                OnPropertyChanged();
            }
        }

        public int IlsThreshold1
        {
            get => ilsThreshold1;
            set
            {
                if (ilsThreshold1 == value)
                    return;
                ilsThreshold1 = value;
                OnPropertyChanged();
            }
        }

        public int IlsThreshold2
        {
            get => ilsThreshold2;
            set
            {
                if (ilsThreshold2 == value)
                    return;
                ilsThreshold2 = value;
                OnPropertyChanged();
            }
        }

        public int IlsThreshold3
        {
            get => ilsThreshold3;
            set
            {
                if (ilsThreshold3 == value)
                    return;
                ilsThreshold3 = value;
                OnPropertyChanged();
            }
        }

        public int IlsF
        {
            get => ilsF;
            set
            {
                if (ilsF == value)
                    return;
                ilsF = value;
                OnPropertyChanged();
            }
        }

        public int SailsTime
        {
            get => sailsTime;
            set
            {
                if (sailsTime == value)
                    return;
                sailsTime = value;
                OnPropertyChanged();
            }
        }

        public int SailsLimit
        {
            get => sailsLimit;
            set
            {
                if (sailsLimit == value)
                    return;
                sailsLimit = value;
                OnPropertyChanged();
            }
        }

        public int SailsThreshold2
        {
            get => sailsThreshold2;
            set
            {
                if (sailsThreshold2 == value)
                    return;
                sailsThreshold2 = value;
                OnPropertyChanged();
            }
        }

        public int SailsThreshold3
        {
            get => sailsThreshold3;
            set
            {
                if (sailsThreshold3 == value)
                    return;
                sailsThreshold3 = value;
                OnPropertyChanged();
            }
        }

        public double SailsMaxInnerLoop
        {
            get => sailsMaxInnerLoop;
            set
            {
                if (sailsMaxInnerLoop == value)
                    return;
                sailsMaxInnerLoop = value;
                OnPropertyChanged();
            }
        }
        public int SailsF
        {
            get => sailsF;
            set
            {
                if (sailsF == value)
                    return;
                sailsF = value;
                OnPropertyChanged();
            }
        }
        public double SailsT0
        {
            get => sailsT0;
            set
            {
                if (sailsT0 == value)
                    return;
                sailsT0 = value;
                OnPropertyChanged();
            }
        }

        public double SailsAlpha
        {
            get => sailsAlpha;
            set
            {
                if (sailsAlpha == value)
                    return;
                sailsAlpha = value;
                OnPropertyChanged();
            }
        }

    }
}
