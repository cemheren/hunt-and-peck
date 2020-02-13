using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using HuntAndPeck.Models;
using HuntAndPeck.Services.Interfaces;
using System.Collections.Generic;

namespace HuntAndPeck.ViewModels
{
    internal class OverlayViewModel : NotifyPropertyChanged
    {
        private Rect _bounds;
        private readonly IntPtr owningWindow;
        private readonly IHintProviderService hintProviderService;
        private ObservableCollection<HintViewModel> _hints = new ObservableCollection<HintViewModel>();

        public OverlayViewModel(
            IEnumerable<Hint> hints,
            Rect owningWindowBounds,
            IntPtr owningWindow,
            IHintLabelService hintLabelService,
            IHintProviderService hintProviderService)
        {
            _bounds = owningWindowBounds;
            this.owningWindow = owningWindow;
            this.hintProviderService = hintProviderService;
            var labels = hintLabelService.GetHintStrings(hints.Count());

            var i = 0;
            foreach (var hint in hints)
            {
                _hints.Add(new HintViewModel(hint)
                {
                    Label = labels[i],
                    Active = false
                });

                i++;
            }
        }

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

        public ObservableCollection<HintViewModel> Hints
        {
            get
            {
                return _hints;
            }
            set
            {
                _hints = value;
                NotifyOfPropertyChange();
            }
        }

        public Action CloseOverlay { get; set; }

        public string MatchString
        {
            set
            {
                foreach (var x in Hints)
                {
                    x.Active = false;
                }

                var matching = Hints.Where(x => x.Label.StartsWith(value)).ToArray();
                foreach (var x in matching)
                {
                    x.Active = true;
                }

                if (matching.Count() == 1)
                {
                    matching.First().Hint.Invoke();
                    CloseOverlay?.Invoke();
                    this.hintProviderService.Invalidate(this.owningWindow);
                }
            }
        }
    }
}
