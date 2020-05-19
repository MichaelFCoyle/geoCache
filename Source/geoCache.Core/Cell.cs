//
// File: Cell.cs
//
// Author:
//   Steinar Herland <steinar.herland@gecko.no>
//
// Copyright (C) 2008 Steinar Herland
// Copyright (C) 2008 Gecko Informasjonssystmer AS (http://www.gecko.no)
// Copyright (C) 2015 Blue Toque Software (http://www.BlueToque.ca)
//
// Licensed under the terms of the GNU Lesser General Public License
// (http://www.opensource.org/licenses/lgpl-license.php)

using System;

namespace GeoCache.Core
{
    public struct Cell : IEquatable<Cell>
    {
        public Cell(double x, double y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        double _x, _y;
        int _z;

        public double X { get => _x; set => _x = value; }

        public double Y { get => _y; set => _y = value; }

        public int Z { get => _z; set => _z = value; }

        public override string ToString() => string.Format("Cell X={0} Y={1} Z={2}", X, Y, X);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z;

        public override bool Equals(object obj) => obj is Cell ? Equals((Cell)obj) : false;

        public bool Equals(Cell other) => other.X == X && other.Y == Y && other.Z == Z;

        public static bool operator ==(Cell cell1, Cell cell2) => cell1.Equals(cell2);

        public static bool operator !=(Cell cell1, Cell cell2) => !cell1.Equals(cell2);
    }
}
