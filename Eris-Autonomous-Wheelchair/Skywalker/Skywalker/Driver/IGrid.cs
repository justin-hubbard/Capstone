using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Skywalker.Driver
{
    /// <summary>
    /// Interface for Model components exposing two-dimensional
    /// grid data to the UI.
    /// </summary>
    public interface IGrid
    {
        /// <summary>
        /// Retrieves the grid data.
        /// </summary>
        /// <returns>Two-dimensional grid.</returns>
        bool[,] GetGrid();

        WriteableBitmap RenderBitmap();
    }
}
