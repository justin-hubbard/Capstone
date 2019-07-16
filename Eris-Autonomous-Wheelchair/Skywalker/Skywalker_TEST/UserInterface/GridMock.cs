using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Skywalker.Driver;

namespace Skywalker_TEST.UserInterface
{
    class GridMock : IGrid
    {
        public bool[,] GetGrid()
        {
            bool[,] gridData = new bool[512, 512];

            for (int i = 0; i < 512; i++)
            {
                for (int j = 0; j < 512; j++)
                {
                    if (j % 2 == 0)
                    {
                        gridData[i, j] = true;
                    }
                }
            }

            return gridData;
        }

        public WriteableBitmap RenderBitmap()
        {
            throw new NotImplementedException();
        }
    }
}
