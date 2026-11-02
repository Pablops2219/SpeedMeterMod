using Beetle_Utils;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(SpeedMeterModDemo.Core), "SpeedMeterModDemo", "0.3.0", "pablops", null)]
[assembly: MelonGame("Bugfight Studio", "Beetleball")]
[assembly: MelonAdditionalDependencies("BeetleUtils")]
[assembly: MelonAdditionalCredits("Spike")]

namespace SpeedMeterModDemo
{
    public class Core : MelonMod
    {
        MelonPreferences_Entry<bool> showSpeedMeter;
        MelonPreferences_Entry<bool> showSpeedPeaks;
        MelonPreferences_Entry<bool> showActualSpeed;
        MelonPreferences_Entry<float> needleSmooth;
        MelonPreferences_Entry<float> maxSpeed;
        MelonPreferences_Entry<float> funnySpeedMultiplier;
        MelonPreferences_Entry<float> speedPeakResetTimer;
        public string assetFolder;
        public override void OnInitializeMelon()
        {
            MelonPreferences_Category SMCategory = MelonPreferences.CreateCategory("SpeedMeterMod");
            showSpeedMeter = SMCategory.CreateEntry<bool>("ShowSpeedMeterOnStartup", true);
            showSpeedPeaks = SMCategory.CreateEntry<bool>("ShowSpeedPeaks", false);
            showActualSpeed = SMCategory.CreateEntry<bool>("ShowActualSpeed", false);
            needleSmooth = SMCategory.CreateEntry<float>("NeedleSmooth", 5f);
            maxSpeed = SMCategory.CreateEntry<float>("MaxSpeed", 240f);
            funnySpeedMultiplier = SMCategory.CreateEntry<float>("FunnySpeedMultiplier", 1f);
            speedPeakResetTimer = SMCategory.CreateEntry<float>("SpeedPeakResetTimer", 3000f);


            string assemblyFolder = Path.GetDirectoryName(MelonAssembly.Assembly.Location);
            assetFolder = Path.Combine(assemblyFolder, "assets");
            LoggerInstance.Msg("SpeedMeterModDemo Initialized.");
        }

        public override void OnUpdate()
        {
            if (hudRoot == null || !hudRoot.gameObject.activeInHierarchy)
            {
                hudRoot = null;
                FindHudRoot();
            }
            UpdateLocalBeetle();
            TrackSpeed();
            HandleInput();
            UpdateSpeedometer();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.F4))
                ToggleSpeedometer();

        }

        private void ToggleSpeedometer()
        {
            if (speedmeterGO == null)
            {
                LoggerInstance.Msg("Creating Speedometer HUD elements.");
                InitializeSpeedmeter();
            }

            if (!showSpeedMeter.Value)
            {
                LoggerInstance.Msg("Speedometer on.");
                speedmeterGO.SetActive(true);
                showSpeedMeter.Value = true;
            }
            else
            {
                LoggerInstance.Msg("Speedometer off.");
                speedmeterGO.SetActive(false);
                showSpeedMeter.Value = false;
            }
        }

        private BeetleActor localBeetle;

        private void UpdateLocalBeetle()
        {
            var beetle = BeetleUtils.GetLocalBeetle();

            if (beetle == null)
            {
                localBeetle = null;
                return;
            }

            if (localBeetle != beetle)
            {
                localBeetle = beetle;
                //LoggerInstance.Msg("Changing beetle");
            }
        }

        float speedPeak = 0;
        float actualSpeed = 0;
        float timeSinceLastPeak = 0;

        public void TrackSpeed()
        {
            if (localBeetle == null)
                return;

            if (localBeetle.HasBall)
            {
                var ballHandling = localBeetle.BallHandling;
                if (ballHandling == null)
                    return;

                DungBall heldBall = ballHandling.HeldBall;
                if (heldBall == null)
                    return;

                actualSpeed = heldBall.Velocity.magnitude;
            }
            else
            {
                actualSpeed = localBeetle.Velocity.magnitude;
            }

            if (!showSpeedPeaks.Value)
                return;

            // Update speed peak
            if (actualSpeed > speedPeak)
            {
                speedPeak = actualSpeed;
                timeSinceLastPeak = 0;
            }
            else
            {
                timeSinceLastPeak += Time.fixedDeltaTime * 1000;
            }

            if (actualSpeed < speedPeak &&
                timeSinceLastPeak > speedPeakResetTimer.Value) // Reset peak if speed drops significantly
            {
                speedPeak = 0;
            }
        }

        //----------------------------------------------------------------------------------------------------
        //HUD METHODS--------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------

        private RawImage speedmeterImage;
        private RawImage needleImage;
        private GameObject speedmeterGO;
        private GameObject needleGO;

        private void InitializeSpeedmeter()
        {
            FindHudRoot();
            CreateSpeedmeterInHud();
            CreateNeedleInHud(speedmeterGO.transform);
        }


        private void CreateSpeedmeterInHud()
        {
            speedmeterGO = new GameObject("Speedometer");
            speedmeterGO.transform.SetParent(hudRoot, false);

            speedmeterImage = speedmeterGO.AddComponent<RawImage>();
            speedmeterImage.texture = LoadTexture("speedmeter.png");

            RectTransform rt = speedmeterImage.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);

            rt.sizeDelta = new Vector2(200, 165);
            rt.anchoredPosition = new Vector2(20, -20);
        }


        private void CreateNeedleInHud(Transform speedometer)
        {
            needleGO = new GameObject("Needle");
            needleGO.transform.SetParent(speedometer, false);

            needleImage = needleGO.AddComponent<RawImage>();
            needleImage.texture = LoadTexture("needle.png");

            RectTransform rt = needleImage.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0f);

            rt.sizeDelta = new Vector2(10, 90);
            rt.anchoredPosition = new Vector2(0, -rt.sizeDelta.y * 0.2f);

            rt.localEulerAngles = new Vector3(0, 0, 120f);
            rt.SetAsLastSibling();
        }

        float currentAngle;
        const float MIN_ANGLE = 120f;
        const float MAX_ANGLE = -120f;

        private void UpdateNeedle()
        {
            if (needleImage == null)
                return;

            float t = Mathf.InverseLerp(0f, maxSpeed.Value, actualSpeed * funnySpeedMultiplier.Value);
            float targetAngle = Mathf.Lerp(MIN_ANGLE, MAX_ANGLE, t);

            if (needleSmooth.Value == 0)
                currentAngle = targetAngle; // No smoothing
            else
                currentAngle = Mathf.Lerp(
                    currentAngle,
                    targetAngle,
                    Time.deltaTime * needleSmooth.Value
                );

            needleImage.rectTransform.localEulerAngles =
                new Vector3(0, 0, currentAngle);
        }

        private void UpdateSpeedometer()
        {
            if (onMainMenu)
                return;

            if (localBeetle == null)
                return;

            if (!showSpeedMeter.Value)
                return;

            if (hudRoot == null)
                FindHudRoot();

            if (hudRoot == null)
                return;

            if (speedmeterGO == null)
                InitializeSpeedmeter();

            if (needleImage == null)
                return;

            UpdateNeedle();
        }


        public override void OnGUI()
        {
            if (onMainMenu) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.white;

            if (showSpeedPeaks.Value)
            {
                GUI.Label(new Rect(20, 200, 300, 30), $"Speed Peak: {this.speedPeak:F1}", style);
            }

            if (showActualSpeed.Value)
            {
                GUI.Label(new Rect(20, 230, 300, 30), $"Speed: {this.actualSpeed:F1}", style);
            }
        }


        //----------------------------------------------------------------------------------------------------
        //HELPER METHODS--------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------


        public bool onMainMenu = true;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            //LoggerInstance.Msg($"Scene {sceneName} with build index {buildIndex} has been loaded!");
            if (0 <= buildIndex && buildIndex <= 2)
            {
                onMainMenu = true;
            }
            else
            {
                onMainMenu = false;
            }
        }

        private Texture2D LoadTexture(string fileName)
        {
            string path = Path.Combine(assetFolder, fileName);

            if (!File.Exists(path))
            {
                MelonLogger.Error($"Texture not found: {path}");
                return null;
            }

            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(tex, data);
            return tex;
        }

        public Transform hudRoot;


        private void FindHudRoot()
        {
            var canvases = GameObject.FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas.name == "Canvas" || canvas.name == "Canvas_CustomLobby")
                {
                    Transform playerHud = canvas.transform.Find("PlayerHUD");
                    if (playerHud != null)
                    {
                        hudRoot = playerHud.Find("Root");
                        if (hudRoot != null)
                        {
                            //MelonLogger.Msg("HUD Root found");
                            return;
                        }
                    }
                }
            }
        }

    }
}