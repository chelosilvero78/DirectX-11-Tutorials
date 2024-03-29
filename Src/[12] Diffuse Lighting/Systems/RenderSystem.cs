﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphics;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = Graphics.Buffer;
using CommandList = Graphics.CommandList;
using SamplerState = Graphics.SamplerState;

namespace Systems
{
    public class RenderSystem
    {
        public List<GraphicsAdapter> Adapters { get; set; }

        public GraphicsDevice Device { get; set; }

        public GraphicsSwapChain SwapChain { get; set; }

        public CommandList CommandList { get; set; }

        public Texture Texture { get; set; }

        public Buffer[] VertexBuffer { get; set; }

        public Buffer[] IndexBuffer { get; set; }

        public Mesh  Mesh { get; set; }

        public Shaders  Shaders { get; set; }

        public PipelineState StateSolid { get; set; }

        public SamplerState  SamplerState { get; set; }

        public Camera Camera { get; set; }

        public Buffer[] ConstantBuffer { get; set; }

        public Matrix Rotation { get; set; }

        public Matrix Translation { get; set; }

        public Matrix[] World { get; set; }

        public float R { get; set; } = 3.14f;

        public ShaderResourceView[] Textures;



        public RenderSystem(PresentationParameters parameters)
        {
            Adapters = GraphicsAdapter.EnumerateGraphicsAdapter();

            Device = new GraphicsDevice(Adapters[0]);

            SwapChain = new GraphicsSwapChain(parameters, Device);

            CommandList = new CommandList(Device);

            Texture = new Texture(Device, SwapChain);

            Shaders = new Shaders(Device, "Shaders/VertexShader.hlsl", "Shaders/PixelShader.hlsl");


            StateSolid = new PipelineState(Device, FillMode.Solid, CullMode.None);


            Mesh = new Mesh("Models/mitsuba-sphere.obj");

            VertexBuffer = new Buffer[1];
            IndexBuffer = new Buffer[1];
            ConstantBuffer = new Buffer[2];



            VertexBuffer[0] = new Buffer(Mesh.SizeInBytes, Mesh.Size, Device, ResourceInfo.VertexBuffer);
            IndexBuffer[0] = new Buffer(Mesh.IndexSizeInBytes, Mesh.IndexSize, Device, ResourceInfo.IndexBuffer);


            ConstantBuffer[0] = new Buffer(Utilities.SizeOf<Transform>(), Utilities.SizeOf<Transform>(), Device, ResourceInfo.ConstantBuffer);
            ConstantBuffer[1] = new Buffer(Utilities.SizeOf<LightBuffer>(), Utilities.SizeOf<LightBuffer>(), Device, ResourceInfo.ConstantBuffer);


            TextureAddressMode Wrap = TextureAddressMode.Wrap;

            SamplerState = new SamplerState(Device, Wrap, Wrap, Wrap, Filter.MinMagMipLinear);


            Camera = new Camera(CameraType.Static);

            Camera.Position = new Vector3(0.0f, 1.2f, -8.0f);

            Camera.SetLens((float)Math.PI / 4, 1.2f, 1.0f, 1000.0f);


            World = new Matrix[3];

            Textures = new ShaderResourceView[3];
            Textures[0] = Texture.LoadFromFile(Device, "Text/cracked_c.png");
            Textures[1] = Texture.LoadFromFile(Device, "Text/mtl01_c.png");
            Textures[2] = Texture.LoadFromFile(Device, "Text/wall_stone04_c2.png");
        }


        public void Update()
        {
            Camera.Update();

            R += .012f;


            // Reset World[0]
            World[0] = Matrix.Identity;
            // Define world space matrix
            Rotation = Matrix.RotationYawPitchRoll(R, 0.0f, 0.0f);
            Translation = Matrix.Translation(-2.5f, -1.5f, 0.0f);
            // Set world space using the transformations
            World[0] = Rotation * Translation;




            // Reset World[1]
            World[1] = Matrix.Identity;
            // Define world space matrix
            Rotation = Matrix.RotationYawPitchRoll(3.14f, 0.0f, 0.0f);
            Translation = Matrix.Translation(0.0f, -1.5f, 0.0f);
            // Set world space using the transformations
            World[1] = Rotation * Translation;





            // Reset World[2]
            World[2] = Matrix.Identity;
            // Define world space matrix
            Rotation = Matrix.RotationYawPitchRoll(-R, 0.0f, 0.0f);
            Translation = Matrix.Translation(2.5f, -1.5f, 0.0f);
            // Set world space using the transformations
            World[2] = Rotation * Translation;
        }




        public void Draw()
        {
            Device.Reset();

            VertexBuffer[0].Update<Vertex>(Mesh.Vertices.ToArray());
            IndexBuffer[0].Update<int>(Mesh.Indices.ToArray());


            CommandList.ClearDepthStencilView(Texture, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil);

            CommandList.SetRenderTargets(Texture);

            //Clear our backbuffer to the updated color
            CommandList.Clear(Texture, new Color4(0.0f, 0.2f, 0.4f, 1));

            CommandList.SetInputLayout(Shaders.Layout);

            CommandList.SetViewPort(SwapChain.PresentParameters.Width, SwapChain.PresentParameters.Height, 0, 0);

            CommandList.SetVertexShader(Shaders.VertexShader);

            CommandList.SetPixelShader(Shaders.PixelShader);




            //---Draw Mesh #2
            CommandList.SetVertexBuffer(VertexBuffer[0]);
            CommandList.SetIndexBuffer(IndexBuffer[0]);
            ConstantBuffer[0].UpdateConstant<Transform>(ShaderType.VertexShader, 0, new Transform(World[0], Camera.View, Camera.Projection)); // cbuffer MatrixBuffer (W V P)   
            ConstantBuffer[1].UpdateConstant<LightBuffer>(ShaderType.PixelShader, 0, new LightBuffer(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), new Vector3(0, 0, 1.05f))); 
            CommandList.SetPrimitiveType(SharpDX.Direct3D.PrimitiveTopology.TriangleList);
            CommandList.SetRasterizerState(StateSolid);
            CommandList.SetSampler(ShaderType.PixelShader, SamplerState, 0);
            CommandList.SetShaderResource(ShaderType.PixelShader, Textures[0], 0);
            CommandList.DrawIndexed(Mesh.IndexCount);




            //---Draw Mesh #2
            CommandList.SetVertexBuffer(VertexBuffer[0]);
            CommandList.SetIndexBuffer(IndexBuffer[0]);
            ConstantBuffer[0].UpdateConstant<Transform>(ShaderType.VertexShader, 0, new Transform(World[1], Camera.View, Camera.Projection)); // cbuffer MatrixBuffer (W V P) : register(b0)
            ConstantBuffer[1].UpdateConstant<LightBuffer>(ShaderType.PixelShader, 0, new LightBuffer(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), new Vector3(0, 0, 1.05f)));
            CommandList.SetPrimitiveType(SharpDX.Direct3D.PrimitiveTopology.TriangleList);
            CommandList.SetRasterizerState(StateSolid);
            CommandList.SetSampler(ShaderType.PixelShader, SamplerState, 0);
            CommandList.SetShaderResource(ShaderType.PixelShader, Textures[1], 0);
            CommandList.DrawIndexed(Mesh.IndexCount);





            //---Draw Mesh #3
            CommandList.SetVertexBuffer(VertexBuffer[0]);
            CommandList.SetIndexBuffer(IndexBuffer[0]);
            ConstantBuffer[0].UpdateConstant<Transform>(ShaderType.VertexShader, 0, new Transform(World[2], Camera.View, Camera.Projection)); // cbuffer MatrixBuffer (W V P) : register(b0)
            ConstantBuffer[1].UpdateConstant<LightBuffer>(ShaderType.PixelShader, 0, new LightBuffer(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), new Vector3(0, 0, 1.05f)));
            CommandList.SetPrimitiveType(SharpDX.Direct3D.PrimitiveTopology.TriangleList);
            CommandList.SetRasterizerState(StateSolid);
            CommandList.SetSampler(ShaderType.PixelShader, SamplerState, 0);
            CommandList.SetShaderResource(ShaderType.PixelShader, Textures[2], 0);
            CommandList.DrawIndexed(Mesh.IndexCount);

        }



        public void End()
        {
            //Present the backbuffer to the screen
            SwapChain.Present(true);
        }
    }
}
