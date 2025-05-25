using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Shared;
using System.Diagnostics;
using KeyEventArgs = SFML.Window.KeyEventArgs;
using View = SFML.Graphics.View;

namespace Royale2D
{
    public static class Game
    {
        public static RenderWindow window;
        
        public static Drawer menuDrawer;
        public static Drawer worldDrawer;
        public static Drawer hudDrawer;

        public static GlobalInputState input = new GlobalInputState();

        public const int ScreenW = 256;
        public const int ScreenH = 224;
        public const int HalfScreenW = ScreenW / 2;
        public const int HalfScreenH = ScreenH / 2;

        public static uint windowW => window.Size.X;
        public static uint windowH => window.Size.Y;
        public static uint windowScale => Options.main.GetWindowScale();

        // Only use these 4 values below for rendering code. Not netcode safe
        public const float spfConst = 1 / 60f;
        public static float spf;
        public static float fps;
        public static double time;

        public static int frameCount;

        // REFACTOR put consts in a better location
        public const string appId = "Royale2D";
        public const int ConnectionTimeoutSeconds = 15;
        public const int MaxConnections = 10;

        public static Stopwatch updatePerfStopwatch = new Stopwatch();
        public static Stopwatch renderPerfStopwatch = new Stopwatch();
        public static RollingAverage updateTimeRA = new RollingAverage();
        public static RollingAverage renderTimeRA = new RollingAverage();

        public static void Init()
        {
            LookupTables.Init();
            Assets.Init();
            Damagers.Init();
            Items.Init();
            Menu.ChangeMenu(new MainMenu());

            window = CreateWindow();

            menuDrawer = new Drawer(Options.main.uiQuality);
            worldDrawer = new Drawer();
            hudDrawer = new Drawer(Options.main.uiQuality);
        }

        public static RenderWindow CreateWindow()
        {
            RenderWindow newWindow;

            if (!Options.main.fullScreen)
            {
                newWindow = new RenderWindow(new VideoMode(ScreenW * windowScale, ScreenH * windowScale), "Royale2D");
            }
            else
            {
                newWindow = new RenderWindow(new VideoMode(VideoMode.DesktopMode.Width, VideoMode.DesktopMode.Height), "Royale2D", Styles.Fullscreen);
            }

            if (Debug.main?.unlimitedFPS != true)
            {
                newWindow.SetVerticalSyncEnabled(true);
                newWindow.SetFramerateLimit(60);
            }

            // newWindow.SetMouseCursorVisible(false);

            var image = new SFML.Graphics.Image(Assets.assetPath.AppendFolder("images/icon.png").fullPath);
            newWindow.SetIcon(image.Size.X, image.Size.Y, image.Pixels);

            newWindow.Closed += new EventHandler(OnClosed);
            newWindow.Resized += new EventHandler<SizeEventArgs>(OnWindowResized);
            newWindow.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPressed);
            newWindow.KeyReleased += new EventHandler<KeyEventArgs>(OnKeyReleased);

            return newWindow;
        }

        public static void StartGameLoop()
        {
            Clock fpsClock = new Clock();

            while (window.IsOpen)
            {
                spf = MyMath.Clamp(fpsClock.Restart().AsSeconds(), 0, 1);
                if (spf > 0)
                {
                    fps = MyMath.Clamp(1 / spf, 0, 10000);
                }

                window.DispatchEvents();
                window.Clear(SFML.Graphics.Color.Transparent);

                updatePerfStopwatch.Restart();
                Update();
                updatePerfStopwatch.Stop();
                updateTimeRA.AddValue(updatePerfStopwatch.ElapsedMilliseconds);

                renderPerfStopwatch.Restart();
                Render();
                renderPerfStopwatch.Stop();
                renderTimeRA.AddValue(renderPerfStopwatch.ElapsedMilliseconds);

                window.Display();

#if DEBUG
                if (input.IsKeyPressed(Keyboard.Key.F12))
                {
                    Texture screenshotTexture = new Texture(Game.window.Size.X, Game.window.Size.Y);
                    screenshotTexture.Update(Game.window);
                    SFML.Graphics.Image screenshot = screenshotTexture.CopyToImage();
                    SFML.Graphics.Image scaledDownImage = Helpers.ScaleDownImage(screenshot, ScreenW, ScreenH);
                    scaledDownImage.SaveToFile(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/r2d_screenshot.png");
                }
#endif

                time += spf;
                frameCount++;
            }
        }

        static void Update()
        {
            Debug.main?.Update();
            input.Update();
            Menu.current?.Update();
            Match.current?.Update();
            MusicManager.main.Update();
        }

        static void Render()
        {
            worldDrawer.PreRender();
            hudDrawer.PreRender();
            menuDrawer.PreRender();

            Menu.current?.Render();
            Match.current?.Render();
            // REFACTOR pass in drawable for others too?
            Debug.main?.RenderToScreen(menuDrawer);

            worldDrawer.PostRender();
            hudDrawer.PostRender();
            menuDrawer.PostRender();
        }

        public static void OnFullScreenChange()
        {
            window.Close();
            window.Dispose();
            window = CreateWindow();
            UpdateViewport();
        }

        public static void UpdateViewport()
        {
            worldDrawer.RefreshViewport();
            hudDrawer.RefreshViewport();
            menuDrawer.RefreshViewport();
        }

        public static void OnWindowScaleChange()
        {
            window.Size = new Vector2u(ScreenW * windowScale, ScreenH * windowScale);
        }

        public static bool HasFocus()
        {
            return window.HasFocus();
        }

        // Only for global sounds, not actor/world ones
        public static void PlaySound(string soundName)
        {
            var sound = Assets.GetSound(soundName);
            sound.Play();
        }

        private static void OnWindowResized(object sender, SizeEventArgs e)
        {
            UpdateViewport();
        }

        private static void OnKeyPressed(object sender, KeyEventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            //if (e.Code == Keyboard.Key.Escape)
            //    window.Close();
        }

        private static void OnKeyReleased(object sender, KeyEventArgs e)
        {
        }

        private static void OnJoystickButtonReleased(object sender, JoystickButtonEventArgs e)
        {
        }

        private static void OnJoystickConnected(object sender, JoystickConnectEventArgs e)
        {
        }

        private static void OnJoystickDisconnected(object sender, JoystickConnectEventArgs e)
        {
        }

        private static void OnClosed(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            Match.current?.Leave("User shut down game client.");
            window.Close();
        }
    }
}
