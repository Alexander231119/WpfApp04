using System;
using System.IO;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using DirectShowLib;
using System.Runtime.ConstrainedExecution;

namespace VideoLib;

public class CarDrawer : IDisposable
{
    private readonly RenderToSurface _rts;
    private readonly Texture _renderTexture;
    private readonly Surface _renderSurface;
    private readonly UserMesh _carMesh;
    private readonly Texture _carTexture;
    private readonly Sprite _sprite;
    private readonly Material _material;
    public double _angle = 73.4;

    public CarDrawer(Device device)
    {
        var folderPath = "..\\..\\Resources\\";
        _carMesh = new($"{folderPath}Vaz21099.mesh", device);
        _carTexture = LoadTexture($"{folderPath}Vaz21099Blue.jpg", device);

        var width = device.Viewport.Width;
        var height = device.Viewport.Height;
        _rts = new RenderToSurface(device, width, height, Format.A8R8G8B8, true, DepthFormat.D24S8);
        _renderTexture = new Texture(device, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
        _renderSurface = _renderTexture.GetSurfaceLevel(0);
        _sprite = new(device);

        var light = device.Lights[0];
        light.Type = LightType.Directional;
        light.Direction = new(0f, -1f, 0f);
        light.Ambient = Color.White;
        light.Enabled = true;
        light.Range = 15000f;
        light.Position = new(0, 0, 0);
        light.Update();

        _material = new()
        {
            Diffuse = Color.White
        };
    }
    //используется камера Sony AX 700
    public float CameraFieldOfView = (float)(73.4 * Math.PI / 180d);
    public static float CameraAspect = 16f / 9f;

    public  Matrix ComputeProjectionMatrix() => Matrix.PerspectiveFovRH((float)(_angle * Math.PI / 180d), CameraAspect, 1f, 15000f);
    //CameraFieldOfView
    private static Texture LoadTexture(string fileName, Device device) =>
        TextureLoader.FromFile(device, fileName,
            -1, -1, -1,
            Usage.Dynamic, Format.Unknown, Pool.Default, Filter.Linear, Filter.Linear, 0);

    //Call Before device.RenderScene
    public void RenderInSurface(CarInFrameInfo car, Device device)
    {
        device.SetTexture(0, _carTexture);
        _rts.BeginScene(_renderSurface);

        device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Transparent, 1f, 0);
        device.VertexFormat = (VertexFormats)258;// CustomVertex.PositionTextured.Format;
        device.SetStreamSource(0, _carMesh.vertexBuffer, 0);
        device.Indices = _carMesh.indexBuffer;

        device.VertexFormat = (VertexFormats)274;// CustomVertex.PositionNormalTextured.Format;
        device.RenderState.CullMode = Cull.Clockwise;
        device.RenderState.ZBufferWriteEnable = true;
        device.RenderState.ZBufferEnable = true;
        device.RenderState.ZBufferFunction = Compare.LessEqual;

        device.Transform.World = Matrix.RotationYawPitchRoll(car.rotationYaw, car.rotationPitch,car.rotationRoll) *
                                 Matrix.Translation(car.positionX, car.positionY, -car.positionZ);
        //var projection = device.Transform.Projection;
        device.Transform.Projection = ComputeProjectionMatrix();

        device.Material = _material;
        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _carMesh.vertexCount, 0, _carMesh.triangleCount);
        _rts.EndScene(Filter.Linear);
    }
    public void Draw(Device device, CarInFrameInfo car)
    {
        device.Transform.World = Matrix.RotationYawPitchRoll(car.rotationYaw, car.rotationPitch, car.rotationRoll) *
        Matrix.Translation(car.positionX, car.positionY, -car.positionZ);
        //var projection = device.Transform.Projection;
        device.Transform.Projection = ComputeProjectionMatrix();

        device.Material = _material;
        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _carMesh.vertexCount, 0, _carMesh.triangleCount);

        return;

        _sprite.Begin(SpriteFlags.AlphaBlend);

        _sprite.Draw2D(_renderTexture, Rectangle.Empty, new Rectangle(0,0,1920, 1080), new Point(0, 0), Color.White);
        _sprite.Flush();
        _sprite.End();
    }

    public void Dispose()
    {
        _renderTexture.Dispose();
        _renderSurface.Dispose();
        _carMesh.Dispose();
        _carTexture.Dispose();
        _sprite.Dispose();
    }
}

public class UserMesh : IDisposable
{
    public readonly VertexBuffer vertexBuffer;
    public readonly IndexBuffer indexBuffer;
    public readonly int vertexCount;
    public readonly int triangleCount;
    public UserMesh(string destination, Device device)
    {
        using var br = new BinaryReader(File.OpenRead(destination));
        var vertices = new CustomVertex.PositionNormalTextured[br.ReadInt32()];
        for (var i = 0; i < vertices.Length; i++)
        {
            //uv.y = 1 - sourceUv.y
            vertices[i] = new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                br.ReadSingle(), br.ReadSingle());
        }
        var indices = new short[br.ReadInt32()];
        for (var i = 0; i < indices.Length; i++)
        {
            indices[i] = br.ReadInt16();
        }

        var format = (VertexFormats)274;//PositionNormalTextured
        vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), vertices.Length, device, Usage.Dynamic | Usage.WriteOnly,
            format, Pool.Default);
        vertexBuffer.SetData(vertices, 0, LockFlags.None);

        indexBuffer = new IndexBuffer(typeof(ushort), indices.Length, device, Usage.WriteOnly, Pool.Default);
        indexBuffer.SetData(indices, 0, LockFlags.None);

        vertexCount = vertices.Length;
        triangleCount = indices.Length / 3;
    }

    public void Dispose()
    {
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }
}
