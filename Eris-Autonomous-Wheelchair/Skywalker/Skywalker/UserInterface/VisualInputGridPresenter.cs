using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Input;
using System.Windows;

namespace Skywalker.UserInterface
{
    /// <summary>
    /// Presenter for the Visual Input Grid control
    /// </summary>
    public class VisualInputGridPresenter : IInputDevice
    {
        /// <summary>
        /// Visual Input Grid view
        /// </summary>
        private VisualInputGrid _view;

        /// <summary>
        /// Instance of presenter
        /// </summary>
        private static VisualInputGridPresenter _instance;

        public static VisualInputGridPresenter Instance
        {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Extends the IInputDevice interface as a UI control.
        /// </summary>
        public Vector InputDirection
        {
            get
            {
                return _view.InputDirection;
            }
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="view">The view to present.</param>
        public VisualInputGridPresenter(VisualInputGrid view)
        {
            _view = view;
            _instance = this;
        }
    }
}
