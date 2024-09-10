using StereoKit;
using TestWebAr.Scritps.Objects;

public class VolumeSkyLog : DefaultSkyLog {

    public VolumeSkyLog(bool volumeSlider = false, bool buttons = false, bool hotKeyPanel = false) : base(volumeSlider, buttons, hotKeyPanel) {

    }

    protected override void InitSkyLogBrowsers()
    {
        browserAmount = 0;
        for (int i = 0; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Quat lookDirection = Quat.LookDir(0, 0, 1);

                Vec3 windowPosition = new Vec3(0.66f * j, 0.12f - (float)(0.45 * -i), -0.7f);

                Vec2 scale = new Vec2(0.65f, 0.65f);

                if (j != 0)
                {
                    Vec3 directionVector = Input.Head.position - windowPosition;
                    directionVector.y = 0;
                    directionVector = directionVector.Normalized;
                    lookDirection = Quat.LookAt(Vec3.Forward, directionVector);

                    windowPosition.z = windowPosition.z + 0.15f;
                }

                browserList.Add(
                        new Browser(
                            "https://skylog-demo.broadteam.eu/",
                            browserAmount.ToString(),
                            new Pose(windowPosition, lookDirection),
                            scale.x,
                            scale.y
                            )
                        );

                while (browserList[browserAmount].browser == null) { }
                while (!browserList[browserAmount].browser.IsBrowserInitialized) { }

                BindVolumeSlider(browserList[i]);

                browserList[browserAmount].BindBrowserSelect(SelectBrowser);
                browserAmount++;
            }
        }
    }
}