using System;

namespace Enkel.Core.Common
{
    public struct Unit : IEquatable<Unit>, IComparable<Unit>
    {
        public static readonly Unit Value = new Unit();

        public bool Equals(Unit other)
        {
            return true;
        }

        public int CompareTo(Unit other)
        {
            return 0;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            return obj is Unit;
        }

        public static bool operator ==(Unit first, Unit second)
        {
            return true;
        }

        public static bool operator !=(Unit first, Unit second)
        {
            return false;
        }

        public override string ToString()
        {
            return "()";
        }
    }
}