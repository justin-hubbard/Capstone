using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Skywalker.Driver;

namespace Skywalker.UserInterface
{
    /// <summary>
    /// Presenter for the ObstacleGridView
    /// </summary>
    public class ObstacleGridPresenter
    {
        /// <summary>
        /// The grid being displayed.
        /// </summary>
        private IGrid _grid;

        /// <summary>
        /// The view being presented.
        /// </summary>
        private ObstacleGridView _view;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="obstacleGrid">The grid to be displayed.</param>
        /// <param name="view">The view to be presented.</param>
        public ObstacleGridPresenter(IGrid obstacleGrid, ObstacleGridView view)
        {
            _grid = obstacleGrid;
            _view = view;
            //_view.BindGrid(GetBindable2DArray(_grid.GetGrid(), 246, 266, 246, 266));
        }

        /// <summary>
        /// Creates a bindable array from a 2D array of primitives.
        /// Taken from this StackOverflow answer: http://stackoverflow.com/a/4002409
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="array">Array to make bindable.</param>
        /// <returns></returns>
        public static DataView GetBindable2DArray<T>(T[,] array, int startX, int endX, int startY, int endY)
        {
            DataTable dataTable = new DataTable();
            for (int i = startX; i < endX; i++)
            {
                dataTable.Columns.Add(i.ToString(), typeof(Ref<T>));
            }
            for (int i = startY; i < endY; i++)
            {
                DataRow dataRow = dataTable.NewRow();
                dataTable.Rows.Add(dataRow);
            }
            DataView dataView = new DataView(dataTable);
            for (int i = startX; i < endX; i++)
            {
                for (int j = startY; j < endY; j++)
                {
                    int a = i;
                    int b = j;
                    Ref<T> refT = new Ref<T>(() => array[a, b], z => { array[a, b] = z; });
                    dataView[i - startX][j - startY] = refT;
                }
            }
            return dataView;
        }
    }

    /// <summary>
    /// Helper class to create a referencable objects from primitives.
    /// </summary>
    /// <typeparam name="T">A primitive type.</typeparam>
    internal class Ref<T>
    {
        private readonly Func<T> getter;
        private readonly Action<T> setter;
        public Ref(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }
        public T Value { get { return getter(); } set { setter(value); } }
    }
}
