using System;
using System.Linq;
using System.Text;

namespace HarnessControl
{
    public class PointValue
    {
        public Type ValueType { get; set; }
        public object Value { get; set; }

        public static implicit operator PointValue(string _Value)
        {
            return new PointValue()
            {
                ValueType = _Value.GetType(),
                Value = _Value
            };
        }

        public static implicit operator PointValue(double _value)
        {
            return new PointValue()
            {
                ValueType = _value.GetType(),
                Value = _value
            };
        }

        public static bool operator ==(PointValue a, PointValue b)
        {
            if (a.ValueType != b.ValueType)
                return false;
            if (a.ValueType == typeof(string))
            {
                return ((string)a.Value).Equals((string)b.Value);
            }
            else
            {
                return ((double)a.Value).Equals((double)b.Value);
            }

        }
        public static bool operator !=(PointValue a, PointValue b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }
            
            // If parameter cannot be cast to Point return false.
            PointValue p = obj as PointValue;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return this == p;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

}
