﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphics;
using SharpDX;
using Buffer = Graphics.Buffer;
using CommandList = Graphics.CommandList;

namespace Systems
{
    public class RenderSystem
    {
        public List<GraphicsAdapter> Adapters { get; set; }

        public GraphicsDevice Device { get; set; }

        public GraphicsSwapChain SwapChain { get; set; }

        public CommandList CommandList { get; set; }

        public Texture Texture { get; set; }

        public Buffer VertexBuffer { get; set; }

        public Buffer IndexBuffer { get; set; }

        public Mesh Mesh { get; set; }

        public Shaders  Shaders { get; set; }




        public RenderSystem(PresentationParameters parameters)
        {
            Adapters = GraphicsAdapter.EnumerateGraphicsAdapter();

            Device = new GraphicsDevice(Adapters[0]);

            SwapChain = new GraphicsSwapChain(parameters, Device);

            CommandList = new CommandList(Device);

            Texture = new Texture(Device, SwapChain);

            Shaders = new Shaders(Device, "Shaders/VertexShader.hlsl", "Shaders/PixelShader.hlsl");

            Mesh = new Mesh();

            VertexBuffer = new Buffer(Mesh.SizeInBytes, Mesh.Size, Device, ResourceInfo.VertexBuffer);

            IndexBuffer = new Buffer(Mesh.IndexSizeInBytes, Mesh.IndexSize, Device, ResourceInfo.IndexBuffer);
        }




        public void Draw()
        {
            Device.Reset();

            VertexBuffer.Update<Vertex>(Mesh.Vertices);

            IndexBuffer.Update<int>(Mesh.Indices);

            CommandList.SetRenderTarget(Texture);

            //Clear our backbuffer to the updated color
            CommandList.Clear(Texture, new Color4(0.0f, 0.2f, 0.4f, 1));

            CommandList.SetInputLayout(Shaders.Layout);

            CommandList.SetViewPort(SwapChain.PresentParameters.Width, SwapChain.PresentParameters.Height, 0, 0);

            CommandList.SetVertexBuffer(VertexBuffer);

            CommandList.SetIndexBuffer(IndexBuffer);

            CommandList.SetVertexShader(Shaders.VertexShader);

            CommandList.SetPixelShader(Shaders.PixelShader);



            //---Draw Mesh
            CommandList.SetPrimitiveType(SharpDX.Direct3D.PrimitiveTopology.TriangleList);
            CommandList.DrawIndexed(Mesh.IndexCount);
        }



        public void End()
        {
            //Present the backbuffer to the screen
            SwapChain.Present(true);
        }
    }
}
