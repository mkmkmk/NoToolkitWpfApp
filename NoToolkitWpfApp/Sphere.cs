using System;
using System.Linq;
using PureSharpDx;
using SharpDX;

namespace NoToolkitWpfApp
{

    public class Sphere
    {
        public VertexPositionNormalTexture[] Vertices { get; private set; }
        public ushort[] Indices { get; private set; }


        public Sphere(float diameter = 1.0f, int tessellation = 16, bool toLeftHanded = false)
        {
            if (tessellation < 3) throw new ArgumentOutOfRangeException("tessellation", "Must be >= 3");

            int verticalSegments = tessellation;
            int horizontalSegments = tessellation * 2;

            var vertices = new VertexPositionNormalTexture[(verticalSegments + 1) * (horizontalSegments + 1)];
            var indices = new int[(verticalSegments) * (horizontalSegments + 1) * 6];

            float radius = diameter / 2;

            int vertexCount = 0;


            for (int i = 0; i <= verticalSegments; i++)
            {
                float v = 1.0f - (float)i / verticalSegments;

                var latitude = (float)((i * Math.PI / verticalSegments) - Math.PI / 2.0);
                var dy = (float)Math.Sin(latitude);
                var dxz = (float)Math.Cos(latitude);

                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float u = (float)j / horizontalSegments;

                    var longitude = (float)(j * 2.0 * Math.PI / horizontalSegments);
                    var dx = (float)Math.Sin(longitude);
                    var dz = (float)Math.Cos(longitude);

                    dx *= dxz;
                    dz *= dxz;

                    var normal = new Vector3(dx, dy, dz);
                    var textureCoordinate = new Vector2(u, v);

                    vertices[vertexCount++] = new VertexPositionNormalTexture(normal * radius, normal, textureCoordinate);
                }
            }

            int stride = horizontalSegments + 1;

            int indexCount = 0;
            for (int i = 0; i < verticalSegments; i++)
            {
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % stride;

                    indices[indexCount++] = (i * stride + j);
                    indices[indexCount++] = (nextI * stride + j);
                    indices[indexCount++] = (i * stride + nextJ);

                    indices[indexCount++] = (i * stride + nextJ);
                    indices[indexCount++] = (nextI * stride + j);
                    indices[indexCount++] = (nextI * stride + nextJ);
                }
            }

            Vertices = vertices;
            Indices = indices.Select(el => (ushort)el).ToArray();

            if (!toLeftHanded)
            {
                for (int i1 = 0; i1 < Indices.Length; i1 += 3)
                {
                    Utilities.Swap(ref Indices[i1], ref Indices[i1 + 2]);
                }
            }
        }
    }
}