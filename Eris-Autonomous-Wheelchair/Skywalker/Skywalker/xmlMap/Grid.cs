using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.xmlMap
{
	public class Grid
	{
		int _column_count;
		int _row_count;
		int _this_column;
		GridPoint[,] _grid;

		public Grid(int column_count, int row_count)
		{
			_column_count = column_count;
			_row_count = row_count;
			_grid = new GridPoint[_column_count, _row_count];
		}

		public void Add_GridPoint(GridPoint grid_point, int row, int column)
		{
			_grid[column, row] = grid_point;
		}

		public GridPoint Point(int column, int row)
		{
			return _grid[column, row];
		}
	}
}
