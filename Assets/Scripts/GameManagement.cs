using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MonitorBreak;
using MonitorBreak.Bebug;

[IntializeAtRuntime]
public class GameManagement : MonoBehaviour
{
    DebugOutput debugOutput;
    DebugOutput.PartHandle fpsHandle;
    DebugOutput.PartHandle otherFpsInfoHandle;

    private int currentFrameSinceUpdate;
    private float currentFrameLimit;

    private float currentFPSCount;

    private void Awake()
    {
        List<DebugOutput.Section> sections = new List<DebugOutput.Section>
        {
            new DebugOutput.Section("Stats:", 115)
        };

        debugOutput = new DebugOutput(sections, Vector2.zero, true);
		debugOutput.render = false;

        fpsHandle = debugOutput.AddPart(0, new DebugOutput.Part(DebugOutput.Part.PartType.Text, "0 FPS"));
        otherFpsInfoHandle = debugOutput.AddPart(0, new DebugOutput.Part(DebugOutput.Part.PartType.Text, "0 FPS"));
    }

    private void Update()
    {
        const float smoothing = 0.5f;

        currentFPSCount = (currentFPSCount * smoothing) + ((1.0f / Time.unscaledDeltaTime) * (1.0f - smoothing));

        if (currentFrameSinceUpdate >= currentFrameLimit)
        {
            currentFrameSinceUpdate = 0;

            currentFrameLimit = currentFPSCount * 0.25f;

            //Update stats
            string colour = "white";
            float averageFPS = Time.frameCount / Time.timeSinceLevelLoad;

            if (currentFPSCount < averageFPS * 0.9f)
            {
                colour = "red";
            }
            else if (currentFPSCount > averageFPS * 1.1f)
            {
                colour = "green";
            }

            debugOutput.UpdateText(fpsHandle, "<color=" + colour + ">" + ((int)currentFPSCount).ToString() + "</color> FPS");

            debugOutput.UpdateText(otherFpsInfoHandle, Time.frameCount + " | " + averageFPS);
        }
        else
        {
            currentFrameSinceUpdate++;
        }
    }

    [MonitorBreak.Bebug.ConsoleCMD("RELOAD")]
    public static void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
