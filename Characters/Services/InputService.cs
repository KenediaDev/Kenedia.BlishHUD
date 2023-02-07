using Blish_HUD;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Services
{
    public static class ExtendedInputService
    {
        public static void MouseWiggle()
        {
            System.Drawing.Point p = Blish_HUD.Controls.Intern.Mouse.GetPosition();

            Blish_HUD.Controls.Intern.Mouse.SetPosition(p.X, p.Y, false);
            Blish_HUD.Controls.Intern.Mouse.SetPosition(p.X, p.Y, true);
        }

        public static async Task<bool> WaitForNoKeyPressed(double maxDelay = 5000)
        {
            double start = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;

            while (GameService.Input.Keyboard.KeysDown.Count > 0)
            {
                await Task.Delay(250);
                if (GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds - start >= maxDelay) return false;
            }

            await Task.Delay(25);
            return true;
        }
    }
}
