using SFML.Graphics;
using SFML.Graphics.Glsl;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Runtime.Versioning;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

class Program
{
    private static int WINDOW_HEIGHT = 1080;
    private static int WINDOW_WIDTH = 1920;

    static vec3 camera_loc = new vec3((float)9.065402, -6, (float)5.9969325);
    ///static vec3 camera_loc = new vec3(10, 100, 10);
    //static vec3 camera_loc = new vec3(0, 0, -5);
    private static Mat4x4 proj_mat = Mat4x4.GetProjMat(WINDOW_WIDTH, WINDOW_HEIGHT, 0.01f, 1.0f, 30);

    static List<Mesh> ListTestMesh = new List<Mesh> { new Mesh(), new Mesh(), new Mesh(), new Mesh(), new Mesh() };
    static List<List<float>> DrawZline = new List<List<float>>();
    static List<List<SFML.Graphics.Color>> ColorZline = new List<List<SFML.Graphics.Color>>();



    static bool outline_only = true;
    static bool allow_mouse_movement = false;
    static bool allow_rotation = false;
    static bool allow_overlay = false;

    static bool[] keys = new bool[6];
    static bool[] RotateGeometric = new bool[6];
    static float[] Floff = {1, 1, 1};
    static float RotKoff = 0.1f;
    static float theta = 0;

    static vec3 look_dir = new vec3((float)-0.062466577, (float)0.99632233, (float)-0.0013507119); // -0,062466845 0,99632233 -0,0013507119
    static float z_dir = look_dir.z;
    //static vec3 look_dir = new vec3((float)-0.033997405, (float)-0.99975, (float) 0.02525); 
    //static vec3 look_dir = new vec3(0, 0, 1);
    static vec3 up_dir = new vec3(0, 1, 0);

    static vec3 light_dir = new vec3(-1, -1, -1);

    static Clock clock_for_movement = new Clock();
    static Clock clock_for_FPS = new Clock();
    static Vector2i mouse_offset;

    static int step = 1;

    static void Start3D()    
    {
        light_dir.norm();

        if (!ListTestMesh[0].load_from_file("C:/Users/461/Documents/5geom/1.obj")) return;
        //if (!ListTestMesh[1].load_from_file("C:/Users/461/Documents/5geom/n/2.obj")) return;
        //if (!ListTestMesh[2].load_from_file("C:/Users/461/Documents/5geom/n/3.obj")) return;
        //if (!ListTestMesh[3].load_from_file("C:/Users/461/Documents/5geom/n/4.obj")) return;
        //if (!ListTestMesh[4].load_from_file("C:/Users/461/Documents/5geom/n/5.obj")) return;

        RenderWindow window = new RenderWindow(new VideoMode((uint)WINDOW_WIDTH, (uint)WINDOW_HEIGHT), "lab 123");
        window.SetVerticalSyncEnabled(true);
        Mouse.SetPosition(new Vector2i(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), window);
        window.SetMouseCursorVisible(false);
        allow_mouse_movement = true;

        window.Closed += (sender, e) => window.Close();
        window.KeyPressed += OnKeyPressed;
        window.KeyReleased += OnKeyReleased;
        window.MouseMoved += OnMouseMoved;

        while (window.IsOpen)
        {
            window.DispatchEvents();
            UpdateMovement();
            Render(window);
        }
    }
    static void Main()
    {
        Start3D();
        //lab4();
    }

    static void OnKeyPressed(object sender, KeyEventArgs e)
    {
        switch (e.Code)
        {
            case Keyboard.Key.Escape: ((RenderWindow)sender).Close(); break;
            case Keyboard.Key.W: keys[0] = true; break;
            case Keyboard.Key.A: keys[1] = true; break;
            case Keyboard.Key.S: keys[2] = true; break;
            case Keyboard.Key.D: keys[3] = true; break;
            case Keyboard.Key.Space: keys[4] = true; break;
            case Keyboard.Key.LShift: keys[5] = true; break;
            case Keyboard.Key.I: RotateGeometric[0] = true; break;
            case Keyboard.Key.J: RotateGeometric[1] = true; break;
            case Keyboard.Key.K: RotateGeometric[2] = true; break;
            case Keyboard.Key.L: RotateGeometric[3] = true; break;
            case Keyboard.Key.U: RotateGeometric[4] = true; break;
            case Keyboard.Key.M: RotateGeometric[5] = true; break;
            case Keyboard.Key.Tab: outline_only = !outline_only; break;
            case Keyboard.Key.Q: allow_overlay = !allow_overlay; break;
            case Keyboard.Key.LControl: allow_mouse_movement = !allow_mouse_movement; break;
            case Keyboard.Key.R: allow_rotation = !allow_rotation; break;
        }
    }

    static void OnKeyReleased(object sender, KeyEventArgs e)
    {
        switch (e.Code)
        {
            case Keyboard.Key.W: keys[0] = false; break;
            case Keyboard.Key.A: keys[1] = false; break;
            case Keyboard.Key.S: keys[2] = false; break;
            case Keyboard.Key.D: keys[3] = false; break;
            case Keyboard.Key.Space: keys[4] = false; break;
            case Keyboard.Key.LShift: keys[5] = false; break;
            case Keyboard.Key.I: RotateGeometric[0] = false; break;
            case Keyboard.Key.J: RotateGeometric[1] = false; break;
            case Keyboard.Key.K: RotateGeometric[2] = false; break;
            case Keyboard.Key.L: RotateGeometric[3] = false; break;
            case Keyboard.Key.U: RotateGeometric[4] = false; break;
            case Keyboard.Key.M: RotateGeometric[5] = false; break;
        }
    }

    static void OnMouseMoved(object sender, MouseMoveEventArgs e)
    {
        if (allow_mouse_movement)
        {
            mouse_offset.X = e.X - WINDOW_WIDTH / 2;
            mouse_offset.Y = e.Y - WINDOW_HEIGHT / 2;
            Mouse.SetPosition(new Vector2i(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), (RenderWindow)sender);
        }
    }

    static void UpdateMovement()
    {
        if (clock_for_movement.ElapsedTime.AsMilliseconds() >= 10)
        {
            vec3 vel = new vec3(0, 0, 0);
            float mapkol = 1.0f;
            if (keys[0]) vel += (new vec3(0, 0, 0.1f) * mapkol);
            if (keys[1]) vel += (new vec3(-0.1f, 0, 0) * mapkol);
            if (keys[2]) vel += (new vec3(0, 0, -0.1f) * mapkol);
            if (keys[3]) vel += (new vec3(0.1f, 0, 0) * mapkol);
            if (keys[4]) vel += (new vec3(0, -0.1f, 0) * mapkol);
            if (keys[5]) vel += (new vec3(0, 0.1f, 0) * mapkol);

            if (RotateGeometric[0]) Floff[0] -= RotKoff;
            if (RotateGeometric[1]) Floff[1] -= RotKoff;
            if (RotateGeometric[2]) Floff[0] -= RotKoff;
            if (RotateGeometric[3]) Floff[1] -= RotKoff;
            if (RotateGeometric[4]) Floff[2] += RotKoff;
            if (RotateGeometric[5]) Floff[2] -= RotKoff;

            if (RotateGeometric[1]) z_dir = (float)Math.Min(0.99, z_dir + 0.01f);
            if (RotateGeometric[3]) z_dir = (float)Math.Max(-0.99, z_dir - 0.01f);


            vec3 temp_dir = new vec3(look_dir.x, 0, look_dir.z);
            temp_dir.norm();
            float phi = (float)Math.Acos(vec3.dot_prod(temp_dir, new vec3(0, 0, 1)));
            phi = (temp_dir.x < 0) ? phi : -phi;
            vec3 rot_vel = vel * Mat4x4.GetRotY(phi);
            camera_loc += rot_vel;
            //Console.WriteLine($"{camera_loc.x} {camera_loc.y} {camera_loc.z}");
            //Console.WriteLine($"{look_dir.x} {look_dir.y} {look_dir.z}");
            clock_for_movement.Restart();
        }
    }
    static void Render(RenderWindow window)
    {
        window.Clear(new SFML.Graphics.Color(255, 255, 220));

        look_dir = look_dir * Mat4x4.GetRotY((float)(-mouse_offset.X * 0.005));
        vec3 hor_dir = new vec3(look_dir.x, 0, look_dir.z);
        hor_dir.norm();
        float phi = (float)Math.Acos(vec3.dot_prod(hor_dir, new vec3(0, 0, 1)));
        phi = (hor_dir.x < 0) ? -phi : phi;
        vec3 temp_dir = look_dir * Mat4x4.GetRotY(phi);
        temp_dir = temp_dir * Mat4x4.GetRotX((float)(-mouse_offset.Y * 0.005));
        look_dir = temp_dir * Mat4x4.GetRotY(-phi);
        look_dir.norm();
        vec3 target = camera_loc + look_dir;
        mouse_offset = new Vector2i(0, 0);
        Mat4x4 view_mat = Mat4x4.GetPointAtMat(camera_loc, target, up_dir);
        view_mat.Invert();

        List<Triangle> toDraw = new List<Triangle>();

        toDraw.Clear();
        foreach (var testMesh in ListTestMesh)
        {
            foreach (var t in testMesh.tris)
            {
                Triangle newtri = new Triangle();
                for (int k = 0; k < 3; k++)
                {
                    newtri.p[k] = t.p[k] * Mat4x4.GetRotY(theta * 1.5f);

                    newtri.p[k] = t.p[k] * Mat4x4.GetScaleMat(Floff[2]);
                    //newtri.p[k] = t.p[k] * Mat4x4.GetRotX(Floff[0]);
                    //newtri.p[k] = t.p[k] * Mat4x4.GetRotY(Floff[1]);
                    //newtri.p[k] = t.p[k] * Mat4x4.GetRotZ(Floff[2]);

                    newtri.p[k] = newtri.p[k] * Mat4x4.GetRotZ(3.1415f);
                }

                newtri.normal = vec3.cross_prod(newtri.p[2] - newtri.p[0], newtri.p[1] - newtri.p[0]);
                newtri.normal.norm();

                vec3 camDir = (newtri.p[0] + newtri.p[1] + newtri.p[2]) / 3 - camera_loc;
                //if (vec3.dot_prod(newtri.normal, camDir) < 0) continue;

                for (int k = 0; k < 3; k++)
                {
                    newtri.p[k] = newtri.p[k] * view_mat;
                }

                List<Triangle> clipped = newtri.clip_fun(new vec3(0, 0, 0.2f), new vec3(0, 0, 1.0f));
                for (int n = 0; n < clipped.Count(); ++n)
                {
                    for (int m = 0; m < 3; m++)
                    {
                        clipped[n].p[m] = clipped[n].p[m] * proj_mat;
                    }
                }

                List<Triangle> Q = new List<Triangle>(clipped);
                for (int x = 0; x < 4; x++)
                {
                    List<Triangle> temp = new List<Triangle>();
                    foreach (var tClip in Q)
                    {
                        List<Triangle> newT = new List<Triangle>();
                        switch (x)
                        {
                            case 0: newT = tClip.clip_fun(new vec3(0, -1, 0), new vec3(0, 1, 0)); break;
                            case 1: newT = tClip.clip_fun(new vec3(0, 1, 0), new vec3(0, -1, 0)); break;
                            case 2: newT = tClip.clip_fun(new vec3(-1, 0, 0), new vec3(1, 0, 0)); break;
                            case 3: newT = tClip.clip_fun(new vec3(1, 0, 0), new vec3(-1, 0, 0)); break;
                        }
                        for(int z = 0; z < newT.Count(); ++z) temp.Add(newT[z]);
                    }
                    Q = temp;
                }

                foreach(Triangle ttt in Q) toDraw.Add(ttt);
            }
        }

        toDraw.Sort((t1, t2) =>
        {
            float z1 = t1.p[0].z + t1.p[1].z + t1.p[2].z;
            float z2 = t2.p[0].z + t2.p[1].z + t2.p[2].z;
            if (z1 <= z2) return 1;
            else return -1;
        });

        foreach (var T in toDraw)
        {
            if (outline_only)
            {
                var outline = new VertexArray(PrimitiveType.LineStrip, 4);

                for (int j = 0; j < 4; j++)
                {
                    float x = (T.p[j % 3].x + 1) * window.Size.X / 2;
                    float y = (T.p[j % 3].y + 1) * window.Size.Y / 2;
                    outline[(uint)j] = new Vertex(new Vector2f(x, y), SFML.Graphics.Color.Black);
                }

                var tri = new VertexArray(PrimitiveType.Triangles, 3);

                for (int j = 0; j < 3; j++)
                {
                    float x = (T.p[j].x + 1) * window.Size.X / 2;
                    float y = (T.p[j].y + 1) * window.Size.Y / 2;
                    tri[(uint)j] = new Vertex(new Vector2f(x, y), new SFML.Graphics.Color(255, 255, 220));
                }

                window.Draw(outline);
            }
            else
            {
                var tri = new SFML.Graphics.VertexArray(PrimitiveType.Triangles, 3);

                for (int j = 0; j < 3; j++)
                {
                    float x = (T.p[j].x + 1) * window.Size.X / 2;
                    float y = (T.p[j].y + 1) * window.Size.Y / 2;

                    float lightFactor = Math.Max(0.0f, vec3.dot_prod(-T.normal, light_dir));
                    int R = (int)(256 * (0.2f + 0.7f * lightFactor));
                    int G = (int)(256 * (0.2f + 0.7f * lightFactor));
                    int B = (int)(256 * (0.2f + 0.7f * lightFactor));

                    var color = new SFML.Graphics.Color((byte)R, (byte)G, (byte)B);
                    tri[(uint)j] = new Vertex(new Vector2f(x, y), color);
                }

                window.Draw(tri);
            }
        }

        if(outline_only)
        {       
        int width = (int)window.Size.X;
        int height = (int)window.Size.Y;

        float[] zBuffer = new float[width * height];
        for (int i = 0; i < zBuffer.Length; i++)
        {
            zBuffer[i] = float.MaxValue;
        }

        SFML.Graphics.Color[] frameBuffer = new SFML.Graphics.Color[width * height];
        for (int i = 0; i < frameBuffer.Length; i++)
        {
            frameBuffer[i] = new SFML.Graphics.Color(255, 255, 220);
        }


        foreach (Triangle tri in toDraw) 
        {
            vec3[] screenVerts = new vec3[3];
            for (int i = 0; i < 3; i++)
            {
                float x = (tri.p[i].x + 1) * width / 2;
                float y = (tri.p[i].y + 1) * height / 2;
                float z = tri.p[i].z;
                screenVerts[i] = new vec3(x, y, z);
            }

            int minX = (int)Math.Max(0, Math.Min(screenVerts[0].x, Math.Min(screenVerts[1].x, screenVerts[2].x)));
            int maxX = (int)Math.Min(width - 1, Math.Max(screenVerts[0].x, Math.Max(screenVerts[1].x, screenVerts[2].x)));
            int minY = (int)Math.Max(0, Math.Min(screenVerts[0].y, Math.Min(screenVerts[1].y, screenVerts[2].y)));
            int maxY = (int)Math.Min(height - 1, Math.Max(screenVerts[0].y, Math.Max(screenVerts[1].y, screenVerts[2].y)));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float w0, w1, w2;
                    if (IsInsideTriangle(x, y, screenVerts[0], screenVerts[1], screenVerts[2], out w0, out w1, out w2))
                    {
                        float pixelZ = w0 * screenVerts[0].z + w1 * screenVerts[1].z + w2 * screenVerts[2].z;
                        int idx = x + y * width;
                        if (pixelZ < zBuffer[idx])
                        {
                            zBuffer[idx] = pixelZ;
                            float lightFactor = Math.Max(0.0f, vec3.dot_prod(-tri.normal, light_dir));
                            int R = (int)(256 * (0.2f + 0.7f * lightFactor));
                            int G = (int)(256 * (0.2f + 0.7f * lightFactor));
                            int B = (int)(256 * (0.2f + 0.7f * lightFactor));
                            frameBuffer[idx] = new SFML.Graphics.Color((byte)R, (byte)G, (byte)B);
                        }
                    }
                }
            }
        }
        SFML.Graphics.Image img = new SFML.Graphics.Image((uint)width, (uint)height);
        for (uint y = 0; y < height; y++)
        {
            for (uint x = 0; x < width; x++)
            {
                int index = (int)(x + y * width);
                img.SetPixel(x, y, frameBuffer[index]);
            }
        }
        SFML.Graphics.Texture tex = new SFML.Graphics.Texture(img);
        SFML.Graphics.Sprite sprite = new SFML.Graphics.Sprite(tex);
            window.Draw(sprite);
        }
        window.Display();
    }

    static bool IsInsideTriangle(int px, int py, vec3 v0, vec3 v1, vec3 v2, out float w0, out float w1, out float w2)
    {
        Vector2f p = new Vector2f((float)px, (float)py);
        Vector2f a = new Vector2f(v0.x, v0.y);
        Vector2f b = new Vector2f(v1.x, v1.y);
        Vector2f c = new Vector2f(v2.x, v2.y);

        float area = EdgeFunction(a, b, c);
        w0 = EdgeFunction(b, c, p) / area;
        w1 = EdgeFunction(c, a, p) / area;
        w2 = EdgeFunction(a, b, p) / area;

        return (w0 >= 0 && w1 >= 0 && w2 >= 0);
    }

    static float EdgeFunction(Vector2f a, Vector2f b, Vector2f c)
    {
        return (c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X);
    }

    static public float GetHight(SixLabors.ImageSharp.PixelFormats.Rgba64 pixelColor)
    {
        //float Xcolor = (765 - (pixelColor.R + pixelColor.B + pixelColor.G)) / 765;
        //float res = ((float)Math.Sin((Xcolor * Math.PI * Math.PI) / 4 - Math.PI / 2) + 1) * 20.5f;
        float c = (pixelColor.B & 0xff + 256 * pixelColor.G & 0xff + 65536 * pixelColor.R & 0xff) * 0.1f;

        return c;
    }
    static public bool Loadfile(string file_name)
    {
        try
        {
            if (!File.Exists(file_name))
                return false;

            SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba64> image =
                SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba64>(file_name);

            int width = image.Width;
            int height = image.Height;


            int frame = width / 100;
            float RadKouf = (float)Math.PI / 180;
            float grad = z_dir*500.0f;
            float tay = grad * RadKouf;
            float tay90 = (90 - grad) * RadKouf;
            float h = width * (float)Math.Abs(Math.Cos(tay)) + height * (float)Math.Abs(Math.Sin(tay));
            float w = height * (float)Math.Abs(Math.Cos(tay)) + width * (float)Math.Abs(Math.Sin(tay));
            //Console.WriteLine($"{h} {w} {Math.Abs(Math.Cos(tay))} {Math.Abs(Math.Sin(tay))}");
            int SizeDataWight = frame * 2 + (int)w / step;
            int SizeDataHeight = frame * 2 + (int)h / step;
            

            int Rwidth = width / step;
            int Rheight = height / step;

            DrawZline = new List<List<float>>(SizeDataWight);
            ColorZline = new List<List<SFML.Graphics.Color>>(SizeDataWight);

            for (int x = 0; x < SizeDataWight; ++x)
            {
                DrawZline.Add(new List<float>(new float[SizeDataHeight]));
                ColorZline.Add(new List<SFML.Graphics.Color>(new SFML.Graphics.Color[SizeDataHeight]));
            }

            //int Xmx = 0, Xmn = 10000000;
            //int Ymx = 0, Ymn = 10000000;
            for (int y = 0; y < Rheight; y++)
            {
                for (int x = 0; x < Rwidth; x++)
                {
                    int indX = (int)Math.Abs((float)((x - Rwidth / 2) * (float)Math.Cos(tay)) - (float)((y - Rheight / 2) * (float)Math.Sin(tay)) + SizeDataWight / 2);
                    //Xmx = Math.Max(Xmx, indX);
                    //Xmn = Math.Min(Xmn, indX);

                    int indY = (int)Math.Abs((float)((x - Rwidth / 2) * (float)Math.Sin(tay)) + (float)((y - Rheight / 2) * (float)Math.Cos(tay)) + SizeDataHeight / 2);
                    //Ymx = Math.Max(Ymx, indY);
                    //Ymn = Math.Min(Ymn, indY);
                    SixLabors.ImageSharp.PixelFormats.Rgba64 pixelColor = image[x * step, y * step];
                    DrawZline[indX][indY] = (GetHight(pixelColor));
                    ColorZline[indX][indY] = new SFML.Graphics.Color((byte)pixelColor.R, (byte)pixelColor.G, (byte)pixelColor.B);
                    //Console.WriteLine($"Pixel H = {GetHight(pixelColor)}");
                }
            }

            for(int i = frame; i < SizeDataWight - frame; ++i)
            {
                float NotNull = 0.0f;
                SFML.Graphics.Color NullColor = new SFML.Graphics.Color((byte) 0, (byte)0, (byte)0);
                int NullCount = 0;
                for(int j = frame; j < SizeDataWight - frame; ++j)
                {
                    if (DrawZline[i][j] == 0 && NullCount < 1)
                    {
                        DrawZline[i][j] = NotNull;
                        ColorZline[i][j] = NullColor;
                        NullCount++;
                    }
                    else
                    {
                        NotNull = DrawZline[i][j];
                        NullColor = ColorZline[i][j];
                        NullCount = 0;
                    }
                }
            }

            //Console.WriteLine($"{Xmn} {Xmx} {Ymn} {Ymx}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File read error: {ex.Message}");
            return false;
        }
    }
    static void WriteLines(RenderWindow window)
    {
        window.Clear(new SFML.Graphics.Color(0, 0, 0));
        look_dir = look_dir * Mat4x4.GetRotY((float)(-mouse_offset.X * 0.005));
        vec3 hor_dir = new vec3(look_dir.x, 0, look_dir.z);
        hor_dir.norm();
        float phi = (float)Math.Acos(vec3.dot_prod(hor_dir, new vec3(0, 0, 1)));
        phi = (hor_dir.x < 0) ? -phi : phi;
        vec3 temp_dir = look_dir * Mat4x4.GetRotY(phi);
        temp_dir = temp_dir * Mat4x4.GetRotX((float)(-mouse_offset.Y * 0.005));
        look_dir = temp_dir * Mat4x4.GetRotY(-phi);
        look_dir.norm();
        vec3 target = camera_loc + look_dir;
        mouse_offset = new Vector2i(0, 0);
        Mat4x4 view_mat = Mat4x4.GetPointAtMat(camera_loc, target, up_dir);
        view_mat.Invert();



        float sizeGrid = 0.03f * (step);
        List<List<vec3>> totoDraw = new List<List<vec3>>();
        totoDraw.Clear();

        for (int i = 0; i < DrawZline.Count; ++i)
        {
            List<vec3> row = new List<vec3>();
            for (int j = 0; j < DrawZline[i].Count; ++j)
            {
                float gx = sizeGrid * i;
                float gz = sizeGrid * j;
                float gy = DrawZline[i][j] / (sizeGrid * 10);
                vec3 preGor = new vec3(gx, gy, gz) * view_mat * Mat4x4.GetRotZ(3.1415f);
                //preGor.y = gz * 0.01f;
                //preGor.y = gy;
                //preGor = preGor * view_mat;

                row.Add(preGor); //  * Mat4x4.GetRotZ(3.1415f)
                //Console.WriteLine($"{sizeGrid * i}, {sizeGrid * j}, {DrawZline[i][j]}");
            }
            totoDraw.Add(row);
        }

        //camera_loc = new vec3((float)sizeGrid * DrawZline.Count , (float)0, (float)sizeGrid * DrawZline.Count / 2);
        int tempme = 0;
        foreach (var T in totoDraw)
        {
            var outline = new VertexArray(PrimitiveType.LineStrip, (uint)totoDraw.Count());

            for (int j = 0; j < (uint)totoDraw.Count(); j++)
            {
                float x = (T[j].x + 1) * window.Size.X / 2;
                float y = (T[j].y + 1) * window.Size.Y / 2;
                //Console.WriteLine($"{x}  {y}");
                outline[(uint)j] = new Vertex(new Vector2f(x, y), ColorZline[tempme][j]);
                //outline[(uint)j] = new Vertex(new Vector2f(x, y), SFML.Graphics.Color.Yellow);
            }
            tempme++;

            window.Draw(outline);
        }
        window.Display();
    }
    static void lab4()
    {
        light_dir.norm();

        RenderWindow window = new RenderWindow(new VideoMode((uint)WINDOW_WIDTH, (uint)WINDOW_HEIGHT), "lab 4");
        window.SetVerticalSyncEnabled(true);
        Mouse.SetPosition(new Vector2i(WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2), window);
        window.SetMouseCursorVisible(false);
        allow_mouse_movement = false;

        window.Closed += (sender, e) => window.Close();
        window.KeyPressed += OnKeyPressed;
        window.KeyReleased += OnKeyReleased;
        window.MouseMoved += OnMouseMoved;


        while (window.IsOpen)
        {
            if (!Program.Loadfile("C:/Users/461/Downloads/a1m.bmp")) return;
            //if (!Program.Loadfile("C:/Users/461/Downloads/a1b.bmp")) return;
            //if (!Program.Loadfile("C:/Users/461/Downloads/gerb.bmp")) return;
            //if (!Program.Loadfile("C:/Users/461/Downloads/womans.bmp")) return;
            //if (!Program.Loadfile("C:/Users/461/Downloads/mona.bmp")) return;
            //if (!Program.Loadfile("C:/map.bmp")) return;

            //if (!Program.Loadfile("C:/Users/461/Downloads/hen.bmp")) return;
            //if (!Program.Loadfile("C:/Users/461/Downloads/blackhole.bmp")) return;
            //if (!Program.Loadfile("C:/Users/461/Downloads/BLACKHOPE.bmp")) return;
            //if (!Program.Loadfile("C:/Users/461/Downloads/nicelab.bmp")) return;
            //if (!Program.Loadfile("C:/Users/461/Downloads/tyan.bmp")) return;

            window.DispatchEvents();
            UpdateMovement();
            WriteLines(window);
            //Console.WriteLine($"{look_dir.x} {look_dir.y} {look_dir.z}");
        }
    }

}

public class vec3
{
    public float x, y, z, w;
    public vec3() { x = 0; y = 0; z = 0; w = 1; }
    public vec3(float xx, float yy, float zz) { x = xx; y = yy; z = zz; w = 1; }
    public static vec3 operator +(vec3 v1, vec3 v2) => new vec3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    public static vec3 operator -(vec3 v1, vec3 v2) => new vec3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    public static vec3 operator *(vec3 v, float f) => new vec3(v.x * f, v.y * f, v.z * f);
    public static vec3 operator /(vec3 v, float f) => new vec3(v.x / f, v.y / f, v.z / f) { w = v.w };
    public static vec3 operator -(vec3 v) => v * -1;

    //public void Add(vec3 v) { x += v.x; y += v.y; z += v.z; }
    public void Sub(vec3 v) { x -= v.x; y -= v.y; z -= v.z; }
    public void Mul(float f) { x *= f; y *= f; z *= f; }
    public void Div(float f) { x /= f; y /= f; z /= f; }

    public static float dot_prod(vec3 v1, vec3 v2) => v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
    public static vec3 cross_prod(vec3 a, vec3 b) => new vec3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);

    public float len() => (float)Math.Sqrt(x * x + y * y + z * z);
    public void norm() => len();


    public static vec3 IntersectPlane(ref vec3 plane_p,ref vec3 plane_n,ref vec3 lineStart, ref vec3 lineEnd, out float t)
    {
        plane_n.norm();
        float plane_d = -dot_prod(plane_n, plane_p);    
        float ad = dot_prod(lineStart, plane_n);
        float bd = dot_prod(lineEnd, plane_n);
        t = (-plane_d - ad) / (bd - ad);
        vec3 lineStartToEnd = lineEnd - lineStart;
        vec3 lineToIntersect = lineStartToEnd * t;
        return lineStart + lineToIntersect;
    }
}
public class Mat4x4
{
    public float[,] M { get; set; } = new float[4, 4];

    public Mat4x4() { }

    public Mat4x4(float[][] values)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                M[i, j] = values[i][j];
            }
        }
    }

    public static Mat4x4 GetProjMat(int width = 1920, int height = 1080, float fNear = 0.1f, float fFar = 100f, float FOV = 90f)
    {
        const float PI = 3.14159f;
        float aspectRatio = (float)height / (float) width;
        float fovRad = FOV / 2 / 180 * PI;

        return new Mat4x4(new float[][]
        {
            new float[] { aspectRatio / (float)Math.Tan(fovRad), 0, 0, 0 },
            new float[] { 0, 1 / (float)Math.Tan(fovRad),           0, 0 },
            new float[] { 0, 0, fFar / (fFar - fNear),                 1 },
            new float[] { 0, 0, -fFar * fNear / (fFar - fNear),        0 }
        });
    }

    public static Mat4x4 GetRotX(float angle)
    {
        return new Mat4x4(new float[][]
        {
            new float[] { 1, 0, 0, 0 },
            new float[] { 0, (float)Math.Cos(angle), (float)Math.Sin(angle), 0 },
            new float[] { 0, -(float)Math.Sin(angle), (float)Math.Cos(angle), 0 },
            new float[] { 0, 0, 0, 1 }
        });
    }

    public static Mat4x4 GetRotY(float angle)
    {
        return new Mat4x4(new float[][]
        {
            new float[] { (float)Math.Cos(angle), 0, (float)Math.Sin(angle), 0 },
            new float[] { 0, 1, 0, 0 },
            new float[] { -(float)Math.Sin(angle), 0, (float)Math.Cos(angle), 0 },
            new float[] { 0, 0, 0, 1 }
        });
    }

    public static Mat4x4 GetRotZ(float angle)
    {
        return new Mat4x4(new float[][]
        {
            new float[] { (float)Math.Cos(angle), (float)Math.Sin(angle), 0, 0 },
            new float[] { -(float)Math.Sin(angle), (float)Math.Cos(angle), 0, 0 },
            new float[] { 0, 0, 1, 0 },
            new float[] { 0, 0, 0, 1 }
        });
    }

    public static Mat4x4 GetPointAtMat(vec3 pos, vec3 target, vec3 up)
    {
        vec3 forward = target - pos;
        forward.norm();

        vec3 a = forward * vec3.dot_prod(up, forward);
        vec3 newUp = up - a;
        newUp.norm();

        vec3 right = vec3.cross_prod(newUp, forward);

        return new Mat4x4(new float[][]
        {
            new float[] { right.x, right.y, right.z, 0 },
            new float[] { newUp.x, newUp.y, newUp.z, 0 },
            new float[] { forward.x, forward.y, forward.z, 0 },
            new float[] { pos.x, pos.y, pos.z, 1 }
        });
    }

    public static Mat4x4 GetScaleMat(float scale)
    {
        return new Mat4x4(new float[][]
        {
        new float[] { scale, 0,      0,      0 },
        new float[] { 0,      scale, 0,      0 },
        new float[] { 0,      0,      scale, 0 },
        new float[] { 0,      0,      0,      1 }
        });
    }
    public static Mat4x4 GetTranslationMat(float dx, float dy, float dz)
    {
        return new Mat4x4(new float[][]
        {
        new float[] { 1, 0, 0, 0 },
        new float[] { 0, 1, 0, 0 },
        new float[] { 0, 0, 1, 0 },
        new float[] { dx, dy, dz, 1 }
        });
    }


    public void Invert()
    {
        Mat4x4 matrix = new Mat4x4();
        matrix.M[0, 0] = M[0, 0]; matrix.M[0, 1] = M[1, 0]; matrix.M[0, 2] = M[2, 0]; matrix.M[0, 3] = 0.0f;
        matrix.M[1, 0] = M[0, 1]; matrix.M[1, 1] = M[1, 1]; matrix.M[1, 2] = M[2, 1]; matrix.M[1, 3] = 0.0f;
        matrix.M[2, 0] = M[0, 2]; matrix.M[2, 1] = M[1, 2]; matrix.M[2, 2] = M[2, 2]; matrix.M[2, 3] = 0.0f;
        matrix.M[3, 0] = -(M[3, 0] * matrix.M[0, 0] + M[3, 1] * matrix.M[1, 0] + M[3, 2] * matrix.M[2, 0]);
        matrix.M[3, 1] = -(M[3, 0] * matrix.M[0, 1] + M[3, 1] * matrix.M[1, 1] + M[3, 2] * matrix.M[2, 1]);
        matrix.M[3, 2] = -(M[3, 0] * matrix.M[0, 2] + M[3, 1] * matrix.M[1, 2] + M[3, 2] * matrix.M[2, 2]);
        matrix.M[3, 3] = 1.0f;
        M = matrix.M;
    }

    public void Print()
    {
        Console.WriteLine("=======================");
        for(int i = 0; i < 4; ++i)
        {
            for(int j = 0; j < 4; ++j)
            {
                Console.Write(M[i, j] + " ");
            }
            Console.Write("\n");
        }
    }

    public static vec3 operator *(vec3 v, Mat4x4 m)
    {
        vec3 res = new vec3();
        res.x = v.x * m.M[0, 0] + v.y * m.M[1, 0] + v.z * m.M[2, 0] + v.w * m.M[3, 0];
        res.y = v.x * m.M[0, 1] + v.y * m.M[1, 1] + v.z * m.M[2, 1] + v.w * m.M[3, 1];
        res.z = v.x * m.M[0, 2] + v.y * m.M[1, 2] + v.z * m.M[2, 2] + v.w * m.M[3, 2];
        res.w = v.x * m.M[0, 3] + v.y * m.M[1, 3] + v.z * m.M[2, 3] + v.w * m.M[3, 3];
        if(res.w > 0) {res.x = res.x / res.w; res.y = res.y / res.w; res.z = res.z / res.w; }
        return res;
    }
}
struct Triangle
{
    public List<vec3> p;
    public vec3 normal;
    public SFML.Graphics.Color color;

    public Triangle() { p = new List<vec3> { new vec3(), new vec3(), new vec3() }; }
    public Triangle(List<vec3> P) { p = P; }
    public Triangle(vec3 p1, vec3 p2, vec3 p3) { p = new List<vec3> { p1, p2, p3 }; }

    public List<Triangle> clip_fun(vec3 plane_p, vec3 plane_n)
    {
        plane_n.norm();

        Func<vec3, float> dist = (vec3 point) =>
        {
            vec3 n = point;
            n.norm();
            return (plane_n.x * point.x + plane_n.y * point.y + plane_n.z * point.z - vec3.dot_prod(plane_n, plane_p));
        };

        vec3[] inside = new vec3[3]; int inside_count = 0;
        vec3[] outside = new vec3[3]; int outside_count = 0;

        for (int i = 0; i < 3; i++)
        {
            if (dist(this.p[i]) >= 0)
            {
                inside[inside_count++] = this.p[i];
            }
            else
            {
                outside[outside_count++] = this.p[i];
            }
        }

        if (inside_count == 0) return new List<Triangle>(0);
        if (inside_count == 3) return new List<Triangle>{this};
        if (inside_count == 1 && outside_count == 2)
        {
            float t;
            Triangle outTri = new Triangle();
            outTri.p[0] = inside[0];
            outTri.p[1] = vec3.IntersectPlane(ref plane_p,ref plane_n,ref inside[0],ref outside[0], out t);
            outTri.p[2] = vec3.IntersectPlane(ref plane_p, ref plane_n, ref inside[0], ref outside[1], out t);
            outTri.normal = normal;
            return new List<Triangle> { outTri };
        }
        if (inside_count == 2 && outside_count == 1)
        {
            float t;
            Triangle out1 = new Triangle(), out2 = new Triangle();

            out1.p[0] = inside[0];
            out1.p[1] = inside[1];
            out1.p[2] = vec3.IntersectPlane(ref plane_p, ref plane_n, ref inside[0], ref outside[0], out t);
            out1.normal = normal;

            out2.p[0] = inside[1];
            out2.p[1] = out1.p[2];
            out2.p[2] = vec3.IntersectPlane(ref plane_p, ref plane_n, ref inside[1], ref outside[0], out t);
            out2.normal = normal;

            return new List<Triangle> { out1, out2 };
        }
        return new List<Triangle>();
    }
}
class Mesh
{
    public List<Triangle> tris;

    public Mesh()
    {
        tris = new List<Triangle>();
    }

    public Mesh(List<Triangle> TRIS)
    {
        tris = TRIS;
    }

    public bool load_from_file(string file_name)
    {
        this.tris = new List<Triangle>();
        if (!File.Exists(file_name)) return false;

        List<vec3> verts = new List<vec3>();
        string[] lines = File.ReadAllLines(file_name);

        foreach (string line in lines)
        {
            string[] parts = line.Split(' ');
            if (parts.Length == 0) continue;

            if (parts[0] == "v")
            {
                float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                verts.Add(new vec3(x, y, z));
            }
            else if (parts[0] == "f")
            {
                List<int> f = new List<int> { 0, 0, 0 };
                bool flag = line.Contains("/");
                if (flag)
                {
                    f[0] = int.Parse(parts[1].Split('/')[0], CultureInfo.InvariantCulture);
                    f[1] = int.Parse(parts[2].Split('/')[0], CultureInfo.InvariantCulture);
                    f[2] = int.Parse(parts[3].Split('/')[0], CultureInfo.InvariantCulture);
                }
                else
                {
                    f[0] = int.Parse(parts[1], CultureInfo.InvariantCulture);
                    f[1] = int.Parse(parts[2], CultureInfo.InvariantCulture);
                    f[2] = int.Parse(parts[3], CultureInfo.InvariantCulture);
                }
                Triangle t = new Triangle(verts[f[0] - 1], verts[f[1] - 1], verts[f[2] - 1]);
                this.tris.Add(t);
            }
        }
        return true;
    }

    public void define_as_cube()
    {
        this.tris = new List<Triangle>
        {
            new Triangle(new vec3(0.0f, 0.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f), new vec3(1.0f, 1.0f, 0.0f)),
            new Triangle(new vec3(0.0f, 0.0f, 0.0f), new vec3(1.0f, 1.0f, 0.0f), new vec3(1.0f, 0.0f, 0.0f)),
            new Triangle(new vec3(1.0f, 0.0f, 0.0f), new vec3(1.0f, 1.0f, 0.0f), new vec3(1.0f, 1.0f, 1.0f)),
            new Triangle(new vec3(1.0f, 0.0f, 0.0f), new vec3(1.0f, 1.0f, 1.0f), new vec3(1.0f, 0.0f, 1.0f)),
            new Triangle(new vec3(1.0f, 0.0f, 1.0f), new vec3(1.0f, 1.0f, 1.0f), new vec3(0.0f, 1.0f, 1.0f)),
            new Triangle(new vec3(1.0f, 0.0f, 1.0f), new vec3(0.0f, 1.0f, 1.0f), new vec3(0.0f, 0.0f, 1.0f)),
            new Triangle(new vec3(0.0f, 0.0f, 1.0f), new vec3(0.0f, 1.0f, 1.0f), new vec3(0.0f, 1.0f, 0.0f)),
            new Triangle(new vec3(0.0f, 0.0f, 1.0f), new vec3(0.0f, 1.0f, 0.0f), new vec3(0.0f, 0.0f, 0.0f)),
            new Triangle(new vec3(0.0f, 1.0f, 0.0f), new vec3(0.0f, 1.0f, 1.0f), new vec3(1.0f, 1.0f, 1.0f)),
            new Triangle(new vec3(0.0f, 1.0f, 0.0f), new vec3(1.0f, 1.0f, 1.0f), new vec3(1.0f, 1.0f, 0.0f)),
            new Triangle(new vec3(1.0f, 0.0f, 1.0f), new vec3(0.0f, 0.0f, 1.0f), new vec3(0.0f, 0.0f, 0.0f)),
            new Triangle(new vec3(1.0f, 0.0f, 1.0f), new vec3(0.0f, 0.0f, 0.0f), new vec3(1.0f, 0.0f, 0.0f))
        };
    }
}