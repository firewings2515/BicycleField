using System;
using Unity.Mathematics;

namespace QuadTerrain
{
    [Serializable]
    public struct NodeBounds
    {
        public int2 min;
        public int2 max;

        public float2 Center => min + new float2(Size.x / 2.0f, Size.y / 2.0f);
        public int2 Extents => Size / 2;
        public int2 Size => (max - min);

        public bool Contains(float2 point)
        {
            return min.x <= point.x && max.x >= point.x && min.y <= point.y && max.y >= point.y;
        }

        public bool Intersects(NodeBounds other)
        {
            if (min.x > other.max.x || other.min.x > max.x)
            {
                return false;
            }

            if (min.y > other.max.y || other.min.y > max.y)
            {
                return false;
            }

            return true;
        }

        public void Expand(int amount)
        {
            min = new int2(min.x - amount, min.y - amount);
            max = new int2(max.x + amount, max.y + amount);
        }

        public FourNodeBounds Subdivide()
        {
            FourNodeBounds bounds = new FourNodeBounds();

            var extents = Extents;
            bounds.Bl = new NodeBounds
            {
                min = min,
                max = min + extents
            };

            bounds.Tl = new NodeBounds();
            bounds.Tl.min = new int2(min.x, min.y + extents.y);
            bounds.Tl.max = bounds.Tl.min + extents;


            bounds.Br = new NodeBounds();
            bounds.Br.min = new int2(min.x + extents.x, min.y);
            bounds.Br.max = bounds.Br.min + extents;

            bounds.Tr = new NodeBounds();
            bounds.Tr.min = new int2(min.x + extents.x, min.y + extents.y);
            bounds.Tr.max = max;

            return bounds;
        }

        public override string ToString()
        {
            return string.Format("min:{0} max:{1}", min.ToString(), max.ToString());
        }

    }
}
