﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace template
{
    class RayTracer
    {
        public static float Lambda = 0.002f;

        public Camera camera;
        public Scene scene;


        Surface screen;

        // initialize
        public void Init(Surface screen)
        {
            this.screen = screen;
            // camera setup
            Vector3 camPos = new Vector3(0, 0, 1);
            Vector3 camDir = new Vector3(1, 0, 0);
            float view = 1;
            camera = new Camera(camPos, camDir, view);


            // initialize the scene with a few object (only one plane atm)
            scene = new Scene();
            scene.AddLight(new Vector3(10, 6, 0), 10, new Vector3(1, 1, 1));

            Plane p = new Plane(new Vector3(-1, -1, -1), new Vector3(0, 0, 1), new Vector3(1, 1, 0));
            scene.AddObject(p);

            Sphere s = new Sphere(new Vector3(10, 0, 0), 2, new Vector3(16, 8, 24));
            scene.AddObject(s);
        }

        public void Render()
        {
            int width = screen.width / 2;

            float xSteps = 1f / width;
            float ySteps = 1f / screen.height;
            
            for (int x = 0; x < width; x++)
                for(int y = 0; y < screen.height; y++)
                {
                    // for each pixel, cast a ray
                    // for now, use static camera, so the 2nd vector is simply calculated with x and y
                    Vector3 pixelDirection = camera.direction + new Vector3(0, (x * xSteps) - 0.5f, -(y * ySteps) + 0.5f);
                    Ray ray = new Ray(camera.position, pixelDirection.Normalized());

                    Vector3 color = TraceRay(ray);

                    screen.pixels[x + y * screen.width] = CreateColor(color);


                    // keep for Debug (for now)
                    Intersection intersect = scene.intersectScene(ray);
                    if (x % 8 == 0 && y == 256)
                    {
                        DebugRay(ray, intersect);
                        //Console.WriteLine(x + " Intersection: " + intersect.Color);
                    }
                }

            DebugView();
        }

        public Vector3 TraceRay(Ray ray)
        {
            Intersection intersect = scene.intersectScene(ray);

            // did not hit anything, return black
            if (intersect.collider == null)
                return Vector3.Zero;
            // TODO: check for materials
            else
            {
                return intersect.Color;//DirectIllumination(intersect) * intersect.Color;
            }
        }

        public Vector3 DirectIllumination(Intersection intersect)
        {
            Vector3 totalIllumination = Vector3.Zero;

            foreach(Light lightSource in scene.lightSources)
            {
                Vector3 lightD = (lightSource.position - intersect.intersectionPoint).Normalized();
                float angle = Vector3.Dot(lightD, intersect.normal);

                float cos = (float) MathHelper.Clamp( Math.Cos(angle), 0, 1);
                
                if(cos > 0)
                {
                    Ray shadowRay = new Ray(intersect.intersectionPoint, lightD);
                    // if we hit nothing
                    if ( !scene.intersectSceneShadow(shadowRay) )
                    {
                        totalIllumination += (lightSource.color * lightSource.intensity * cos);
                    }
                }
                
            }
            return totalIllumination;
        }
        

        #region Debug
        // screen scale relative to world scale
        float scale = -32;
        int xOffset = 512 + 256;
        int yOffset = 480;

        public void DebugView()
        {
            screen.Print("Camera", (int)(-camera.position.Y * scale + xOffset), (int) (camera.position.X * scale + yOffset), 0xFF0000);

            screen.Line(767, 0, 767, 512, 0xffffff);
            screen.Print("+x", 760, 10, 0xffffff);
            screen.Print("-x", 760, 490, 0xffffff);
            screen.Line(512, 256, 1023, 256, 0xffffff);
            screen.Print("+y", 1000, 260, 0xffffff);
            screen.Print("-y", 516, 260, 0xffffff);

            foreach (Primitive p in scene.sceneObjects)
            {
                if (p is Sphere)
                    screen.Print("SPHERE", (int)(-p.position.Y * scale + xOffset), (int) (p.position.X * scale + yOffset) - 10, 0x00ff00);
            }
            foreach (Light l in scene.lightSources)
            {
                screen.Print("LIGHT", (int)(-l.position.Y * scale + xOffset), (int)(l.position.X * scale + yOffset) - 10, 0x00ffff);
            }
        }

        // draw a debug line for the specified ray.
        public void DebugRay(Ray ray, Intersection i)
        {
            Vector3 startPoint = ray.origin;
            Vector3 endPoint;
            
            if (i.collider != null)
                endPoint = i.intersectionPoint;
            else
                endPoint = ray.origin + -scale * ray.direction;

            int x1 = (int)(-startPoint.Y * scale) + xOffset;
            int y1 = (int)(startPoint.X * scale) + yOffset;

            int x2 = (int)(-endPoint.Y * scale) + xOffset;
            int y2 = (int)(endPoint.X * scale) + yOffset;
            /*
            x1 = MathHelper.Clamp(x1, 512, 1023);
            y1 = MathHelper.Clamp(y1, 0, 511);
            x2 = MathHelper.Clamp(x2, 512, 1023);
            y2 = MathHelper.Clamp(y2, 0, 511);
            */
            screen.Line(x1, y1, x2, y2, 0xffff00);
        }
        #endregion

        int CreateColor(int red, int green, int blue)
        {
            return (red << 16) + (green << 8) + blue;
        }

        int CreateColor(Vector3 color)
        {
            int red = (int) (color.X * 255);
            int green = (int)(color.Y * 255);
            int blue = (int)(color.Z * 255);

            red = Math.Min(255, red);
            green = Math.Min(255, green);
            blue = Math.Min(255, blue);

            return CreateColor(red, green, blue);
        }
    }
}