using System.Runtime.InteropServices;
using SharpDX;

namespace PureSharpDx
{
  
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTexture
    {
      
        public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
        }
      
        public Vector3 Position;

      
        public Vector3 Normal;

      
        public Vector2 TextureCoordinate;
        
        
        public static readonly int Size = 32;

     
    }
}
