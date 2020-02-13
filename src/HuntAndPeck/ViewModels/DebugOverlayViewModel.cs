﻿using System.Collections.Generic;
using System.Windows;
using HuntAndPeck.Models;
using System.Linq;

namespace HuntAndPeck.ViewModels
{
    public class DebugOverlayViewModel : NotifyPropertyChanged
    {
        private Rect _bounds;

        public DebugOverlayViewModel(IEnumerable<Hint> hints, Rect bounds)
        {
            Bounds = bounds;
            Hints = hints.OfType<DebugHint>().Select(x => new DebugHintViewModel(x)).ToList();
        }

        public List<DebugHintViewModel> Hints { get; set; }

        /// <summary>
        /// Bounds in logical screen coordiantes
        /// </summary>
        public Rect Bounds
        {
            get
            {
                return _bounds;
            }
            set
            {
                _bounds = value;

                NotifyOfPropertyChange();
            }
        }
    }
}
