using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpringAnimation.Core;

namespace SpringAnimation.Window
{
    public class Viewer : Form
    {
        private static readonly TimeSpan TargetFps = TimeSpan.FromSeconds(1 / 2.5f);
        private static int NUM_FRAMES = 10;

        private readonly Panel viewer_;

        private static volatile int CURRENT_INDEX = 0;

        public Viewer()
        {
            var thread = new Thread(Draw);
            thread.Start();
        }

        public void Draw()
        {
            var graphics = CreateGraphics();
            Stopwatch stopWatch = Stopwatch.StartNew();
            var currentFrame = 0;
            //var springFunction = springFunctions[randomIndex];

            SpringFunction springFunction = null;
            string springFunctionName = "";
            while (true)
            {
                if (currentFrame == 0 || ReferenceEquals(null, springFunction))
                {
                    var springFunctionAndName = RandomSpringFunction();
                    springFunction = springFunctionAndName.Item1;
                    springFunctionName = springFunctionAndName.Item2;
                }
                currentFrame = (currentFrame + 1) % NUM_FRAMES;
                graphics.Clear(Color.White);
                var startTime = stopWatch.Elapsed;


                using (var firstFrame = new Bitmap(Image.FromFile("Content/Running1.png")))
                {
                    using (var lastFrame = new Bitmap(Image.FromFile("Content/Running2.png")))
                    {
                        using (var resultFrame = new Bitmap(firstFrame.Width, firstFrame.Height))
                        {
                            for (int x = 0; x < firstFrame.Width; ++x)
                            {
                                for (int y = 0; y < lastFrame.Height; ++y)
                                {
                                    var originalColor = firstFrame.GetPixel(x, y);
                                    var targetColor = lastFrame.GetPixel(x, y);
                                    var percent = springFunction(0, 1, currentFrame, NUM_FRAMES - 1);
                                    byte a = (byte) (originalColor.A + (targetColor.A - originalColor.A) * percent);
                                    byte r = (byte) (originalColor.R + (targetColor.R - originalColor.R) * percent);
                                    byte g = (byte) (originalColor.G + (targetColor.G - originalColor.G) * percent);
                                    byte b = (byte) (originalColor.B + (targetColor.B - originalColor.B) * percent);
                                    var resultColor = Color.FromArgb(a, r, g, b);
                                    resultFrame.SetPixel(x, y, resultColor);
                                }
                            }
                            graphics.DrawImage(resultFrame, new Point(0, 0));
                            using (var font = new Font(FontFamily.GenericMonospace, 12))
                            {
                                using (var brush = new SolidBrush(Color.Red))
                                {
                                    graphics.DrawString($"{springFunctionName} {currentFrame}", font, brush, 0, 100);
                                }
                            }
                        }
                    }
                }

                TimeSpan endTime;
                do
                {
                    endTime = stopWatch.Elapsed;
                } while (endTime < startTime + TargetFps);
            }
        }

        private static Tuple<SpringFunction, string> RandomSpringFunction()
        {
            var springFunctions = Spring.SpringFunctionsAndNames;
            var springFunctionAndName = springFunctions[CURRENT_INDEX++ % springFunctions.Count];
            return springFunctionAndName;
        }
    }
}
