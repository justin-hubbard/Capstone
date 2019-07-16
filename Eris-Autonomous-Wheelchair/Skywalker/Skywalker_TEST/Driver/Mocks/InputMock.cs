using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Input;
using Skywalker.Resources;

namespace Skywalker_TEST.Driver.Mocks
{
    /// <summary>
    /// Mock of the Input Device interface.
    /// </summary>
    public class InputMock : IInputDevice
    {
        public Vector InputToReturn = new Vector(0,0);

        public Vector InputDirection
        {
            get
            {
                return InputToReturn;
            }
        }
    }
}
