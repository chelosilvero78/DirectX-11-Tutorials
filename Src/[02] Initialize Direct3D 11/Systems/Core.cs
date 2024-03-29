﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphics;
using Desktop;

namespace Systems
{
    public class Core : IDisposable
    {

        public Windows Windows { get; set; }

        public PresentationParameters parameters { get; set; }

        public RenderSystem RenderSystem { get; set; }



        public Core() { }

        public void Initialize()
        {

            if (Windows == null)
                // Create the Windows object.
                Windows = new Windows("Initialize Direct3D 11", 800, 600);
            




            if (parameters == null)
            {
                // Create the PresentationParameters object.
                parameters = new PresentationParameters();

                parameters.Height = Windows.ClientSize.Height;
                parameters.Width = Windows.ClientSize.Width;
                parameters.Handle = Windows.Handle;
            }



            if (RenderSystem == null)
            {
                // Create the RenderSystem object.
                RenderSystem = new RenderSystem(parameters);
            }


            Console.WriteLine(RenderSystem.Adapters[0].Description);
            Console.WriteLine(RenderSystem.Adapters[1].Description);
        }


        public void Run()
        {
            RenderLoop.Run(Windows,Render);
        }


        public void Render()
        {
            Update();


            // Clear the buffer to begin the scene.
            RenderSystem.Draw();

            // Present the rendered scene to the screen.
            RenderSystem.End();
        }


        public void Update()
        {

        }


        public void Dispose()
        {

            if (Windows != null)
            {
                Windows.Dispose();
                Windows = null;
            }

        }
    }
}
