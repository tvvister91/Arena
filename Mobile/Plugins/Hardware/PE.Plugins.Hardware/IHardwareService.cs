using System.Drawing;

namespace PE.Plugins.Hardware
{
    public interface IHardwareService
    {
        void UpdateStatusBarColor(int a, int r, int g, int b);

        void ShowStatusBar(bool hide, bool animated = false);

        bool HasTopNotch();

        float[] GetSafeArea();

        bool IsEmulator();
    }
}
