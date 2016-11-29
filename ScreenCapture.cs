using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using ScreenCap.NativeMethods;

namespace ScreenCap
{
    public static class ScreenCapture
    {
        /// <summary>
        /// Creates an Image object containing a screen shot of the entire desktop 
        /// </summary>
        public static Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }

        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window 
        /// </summary>
        public static Image CaptureWindow(IntPtr handle)
        {
            if(handle == IntPtr.Zero)
                handle = User32.GetForegroundWindow();

            // get te hDC of the target window 
            var hdcSrc = User32.GetWindowDC(handle);
            // get the size 
            var windowRect = new Rect();
            User32.GetWindowRect(handle, ref windowRect);

            var clientRect = new Rect();
            User32.GetClientRect(handle, ref clientRect);

            var topLeft = new LpPoint { X = clientRect.Left, Y = clientRect.Top };
            User32.ClientToScreen(handle, ref topLeft);

            var bottomRight = new LpPoint { X = clientRect.Bottom, Y = clientRect.Right };
            User32.ClientToScreen(handle, ref bottomRight);

            var width = clientRect.Right - clientRect.Left;
            var height = clientRect.Bottom - clientRect.Top;

            var offsetX = topLeft.X - windowRect.Left;
            var offsety = topLeft.Y - windowRect.Top;

            // create a device context we can copy to 
            var hdcDest = Gdi32.CreateCompatibleDC(hdcSrc);

            // create a bitmap we can copy it to, 
            // using GetDeviceCaps to get the width/height 
            var hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, width, height);

            // select the bitmap object 
            var hOld = Gdi32.SelectObject(hdcDest, hBitmap);

            // bitblt over 
            Gdi32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, offsetX, offsety, Gdi32.Srccopy);

            // restore selection 
            Gdi32.SelectObject(hdcDest, hOld);

            // clean up 
            Gdi32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);

            // get a .NET image object for it 
            Image img = Image.FromHbitmap(hBitmap);

            // free up the Bitmap object 
            Gdi32.DeleteObject(hBitmap);

            return img;
        }

        public static void CaptureActiveWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            var img = CaptureWindow(handle);
            img.Save(filename, format);
        }

        public static void CaptureScreenToFile(string filename, ImageFormat format)
        {
            var img = CaptureScreen();
            img.Save(filename, format);
        }

        private static bool _fullscreen = true;
        private static string _file = "screenshot.bmp";
        private static ImageFormat _format = ImageFormat.Bmp;
        private static string _windowTitle = "";

        private static void ParseArguments()
        {
            var arguments = Environment.GetCommandLineArgs();
            if (arguments.Length == 1)
            {
                PrintHelp();
                Environment.Exit(0);
            }
            if (arguments[1].ToLower().Equals("/h") || arguments[1].ToLower().Equals("/help"))
            {
                PrintHelp();
                Environment.Exit(0);
            }

            _file = arguments[1];
            var formats =
                new Dictionary<string, ImageFormat>
                {
                    {"bmp", ImageFormat.Bmp},
                    {"emf", ImageFormat.Emf},
                    {"exif", ImageFormat.Exif},
                    {"jpg", ImageFormat.Jpeg},
                    {"jpeg", ImageFormat.Jpeg},
                    {"gif", ImageFormat.Gif},
                    {"png", ImageFormat.Png},
                    {"tiff", ImageFormat.Tiff},
                    {"wmf", ImageFormat.Wmf}
                };



            var ext = "";
            if (_file.LastIndexOf('.') > -1)
            {
                ext = _file.ToLower().Substring(_file.LastIndexOf('.') + 1, _file.Length - _file.LastIndexOf('.') - 1);
            }
            else
            {
                Console.WriteLine("Invalid file name - no extension");
                Environment.Exit(7);
            }

            try
            {
                _format = formats[ext];
            }
            catch (Exception e)
            {
                Console.WriteLine("Probably wrong file format:" + ext);
                Console.WriteLine(e.ToString());
                Environment.Exit(8);
            }

            if (arguments.Length <= 2)
                return;

            _windowTitle = arguments[2];
            _fullscreen = false;
        }

        private static void PrintHelp()
        {
            //clears the extension from the script name
            var scriptName = Environment.GetCommandLineArgs()[0];
            scriptName = scriptName.Substring(0, scriptName.Length);
            Console.WriteLine(scriptName + " captures the screen or the active window and saves it to a file.");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine(" " + scriptName + " filename  [WindowTitle]");
            Console.WriteLine("");
            Console.WriteLine("finename - the file where the screen capture will be saved");
            Console.WriteLine("     allowed file extensions are - Bmp,Emf,Exif,Gif,Icon,Jpeg,Png,Tiff,Wmf.");
            Console.WriteLine("WindowTitle - instead of capture whole screen you can point to a window ");
            Console.WriteLine("     with a title which will put on focus and captuted.");
            Console.WriteLine("     For WindowTitle you can pass only the first few characters.");
            Console.WriteLine("     If don't want to change the current active window pass only \"\"");
        }

        public static void Main()
        {
            ParseArguments();

            var handle = IntPtr.Zero;

            if (!_fullscreen && !_windowTitle.Equals(""))
            {
                try
                {
                    Console.WriteLine($"Looking for window '{_windowTitle}.'");

                    handle = User32.FindWindow(null, _windowTitle);

                    if (handle == IntPtr.Zero)
                    {
                        Console.WriteLine($"Could not find a window with title '{_windowTitle}'.");
                        Environment.Exit(-1);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occurred while attempting to get a handle to the window.");
                    Console.WriteLine(e);
                    Environment.Exit(-1);
                }
            }

            // Create directory if it doesn't exist
            var directoryName = Path.GetDirectoryName(_file);
            if(!string.IsNullOrWhiteSpace(directoryName))
                Directory.CreateDirectory(directoryName);

            try
            {
                if (_fullscreen)
                {
                    Console.WriteLine("Taking a capture of the whole screen to " + _file);
                    CaptureScreenToFile(_file, _format);
                }
                else
                {
                    Console.WriteLine("Taking a capture of the active window to " + _file);
                    CaptureActiveWindowToFile(handle, _file, _format);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Check if file path is valid " + _file);
                Console.WriteLine(e.ToString());
            }
        }
    }
}