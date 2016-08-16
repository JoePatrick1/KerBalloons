using UnityEngine;

namespace KerBalloons
{
    public class ModuleKerBalloon : PartModule
    {
        [KSPField(isPersistant = false)]
        public string CFGballoonObject;
        [KSPField(isPersistant = false)]
        public string CFGropeObject;
        [KSPField(isPersistant = false)]
        public string CFGcapObject;
        [KSPField(isPersistant = false)]
        public string CFGliftPointObject;
        [KSPField(isPersistant = false)]
        public string CFGballoonPointObject;

        public GameObject balloonObject;
        public GameObject ropeObject;
        public GameObject capObject;
        public GameObject liftPointObject;
        public GameObject balloonPointObject;

        [KSPField(isPersistant = false)]
        public float minAtmoPressure;
        [KSPField(isPersistant = false)]
        public float maxAtmoPressure ;
        [KSPField(isPersistant = false)]
        public float minScale;
        [KSPField(isPersistant = false)]
        public float maxScale;
        [KSPField(isPersistant = false)]
        public float minLift;
        [KSPField(isPersistant = false)]
        public float maxLift;
        [KSPField(isPersistant = false)]
        public string recommendedBody;
        [KSPField(isPersistant = false)]
        public float targetTWR;
        [KSPField(isPersistant = false)]
        public float liftLimit;
        [KSPField(isPersistant = false)]
        public bool speedLimiter;
        [KSPField(isPersistant = false)]
        public float maxSpeed;
        [KSPField(isPersistant = false)]
        public float maxSpeedTolerence;
        [KSPField(isPersistant = false)]
        public float speedAdjustStep;
        [KSPField(isPersistant = false)]
        public float speedAdjustMin;
        [KSPField(isPersistant = false)]
        public float speedAdjustMax;
        public float speedAdjust;

        [KSPField(isPersistant = true)]
        public bool isInflating;
        [KSPField(isPersistant = true)]
        public bool hasInflated;
        [KSPField(isPersistant = true)]
        public bool isInflated;
        [KSPField(isPersistant = true)]
        public bool isDeflating;
        [KSPField(isPersistant = true)]
        public bool hasBurst;
        [KSPField(isPersistant = true)]
        public bool isRepacked;
        [KSPField(isPersistant = false)]
        public float scaleInc;

        [KSPField(isPersistant = false)]
        public string bodyName;
        [KSPField(isPersistant = false)]
        public float bodyG;

        public override void OnStart(StartState state)
        {
            Debug.Log("ModuleKerBalloon Loaded");

            if (HighLogic.LoadedSceneIsFlight)
            {
                balloonObject = getChildGameObject(this.part.gameObject, CFGballoonObject);
                ropeObject = getChildGameObject(this.part.gameObject, CFGropeObject);
                capObject = getChildGameObject(this.part.gameObject, CFGcapObject);
                liftPointObject = getChildGameObject(this.part.gameObject, CFGliftPointObject);
                balloonPointObject = getChildGameObject(this.part.gameObject, CFGballoonPointObject);

                balloonObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                initialBalloonScale = balloonObject.transform.localScale;
                initialBalloonPos = balloonObject.transform.transform.localPosition;
                initialRopeScale = ropeObject.transform.localScale;

                if(hasInflated && !isInflated)
                {
                    balloonObject.SetActive(false);
                    ropeObject.SetActive(false);
                    capObject.SetActive(false);
                }else if(isInflating)
                {
                    repackBalloon();
                }
                else if (isDeflating)
                {
                    balloonObject.SetActive(false);
                    ropeObject.SetActive(false);

                    isInflated = false;
                    isDeflating = false;
                    isRepacked = false;
                }
            }
        }


        public void FixedUpdate()
        {

            if (HighLogic.LoadedSceneIsFlight)
            {
                //print("hasInflated: " + hasInflated + " | isInflated: " + isInflated + " | isInflating: " + isInflating + " | isDeflating: " + isDeflating + " | hasBurst: " + hasBurst + " | isRepacked: " + isRepacked);
                float currentPressure = (float)FlightGlobals.getStaticPressure(this.part.transform.position);
                if (hasInflated && !hasBurst)
                {
                    if (isInflated)
                    {
                        float lift = BalloonProperties.getLift(this);
                        this.part.Rigidbody.AddForceAtPosition(vessel.upAxis * lift,liftPointObject.transform.position);
                        Vector3 scale = new Vector3(BalloonProperties.getScale(this), BalloonProperties.getScale(this), BalloonProperties.getScale(this));
                        balloonObject.transform.localScale = scale;

                        ropeObject.transform.rotation = Quaternion.Slerp(ropeObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), BalloonProperties.getLift(this) / 10);
                        balloonObject.transform.rotation = Quaternion.Slerp(balloonObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), BalloonProperties.getLift(this) / 8);

                        balloonObject.transform.position = balloonPointObject.transform.position;

                        if (currentPressure < minAtmoPressure || currentPressure > maxAtmoPressure) hasBurst = true;
                    }
                    else if (isDeflating)
                    {
                        if (scaleInc > 0)
                        {
                            scaleInc -= BalloonProperties.getScale(this) / 100;
                            balloonObject.transform.localScale = new Vector3(scaleInc, scaleInc, scaleInc);

                            float progress = scaleInc / BalloonProperties.getScale(this);

                            float lift = BalloonProperties.getLift(this) * progress;
                            this.part.Rigidbody.AddForceAtPosition(vessel.upAxis * lift, liftPointObject.transform.position);

                            ropeObject.transform.localScale = new Vector3(1, 1, progress);
                            balloonObject.transform.position = balloonPointObject.transform.position;
                        }
                        else
                        {
                            balloonObject.SetActive(false);
                            ropeObject.SetActive(false);

                            isInflated = false;
                            isDeflating = false;
                            isRepacked = false;
                        }
                    }else if(!isInflated && !isInflating && !isDeflating && !isRepacked)
                    {
                        Events["repackBalloon"].active = true;
                    }
                }
                else if (isInflating && !hasBurst)
                {
                    if (scaleInc < BalloonProperties.getScale(this))
                    {
                        scaleInc += BalloonProperties.getScale(this)/200;
                        balloonObject.transform.localScale = new Vector3(scaleInc, scaleInc, scaleInc);

                        float progress = scaleInc / BalloonProperties.getScale(this);

                        float lift = BalloonProperties.getLift(this) * progress;
                        this.part.Rigidbody.AddForceAtPosition(vessel.upAxis * lift, liftPointObject.transform.position);


                        ropeObject.transform.rotation = Quaternion.Slerp(ropeObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), BalloonProperties.getLift(this) / 10);
                        balloonObject.transform.rotation = Quaternion.Slerp(balloonObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), BalloonProperties.getLift(this) / 8);

                        ropeObject.transform.localScale = new Vector3(1, 1, progress);
                        balloonObject.transform.position = balloonPointObject.transform.position;
                    }
                    else
                    {
                        hasInflated = true;
                        isInflated = true;
                        isInflating = false;
                    }
                }
                else if (hasBurst && (isInflated || isInflating || isDeflating))
                {
                    this.part.Effect("burst");
                    isInflated = false;
                    isInflating = false;
                    isDeflating = false;
                    balloonObject.SetActive(false);
                    ropeObject.SetActive(false);
                    Events["inflateBalloon"].active = false;
                    Events["deflateBalloon"].active = false;
                    Actions["inflateAction"].active = false;
                    Actions["deflateAction"].active = false;
                }
            }
            
        }


        public Vector3 initialBalloonScale;
        public Vector3 initialBalloonPos;
        public Vector3 initialRopeScale;

        [KSPEvent(active = false, guiActive = false, guiActiveEditor = false, guiActiveUnfocused = true, unfocusedRange = 4, externalToEVAOnly = true,  guiName = "Repack Balloon")]
        public void repackBalloon()
        {
            isInflated = false;
            isInflating = false;
            isDeflating = false;
            hasBurst = false;
            hasInflated = false;
            isRepacked = true;

            balloonObject.transform.localScale = initialBalloonScale;
            balloonObject.transform.localPosition = initialBalloonPos;
            ropeObject.transform.localScale = initialBalloonScale;

            capObject.SetActive(true);
            balloonObject.SetActive(true);
            ropeObject.SetActive(true);

            Events["repackBalloon"].active = false;
            Events["inflateBalloon"].active = true;
            Events["deflateBalloon"].active = false;
            Actions["inflateAction"].active = true;
            Actions["deflateAction"].active = false;
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUnfocused = false, guiName = "Inflate Balloon")]
        public void inflateBalloon()
        {
            if (!isInflated)
            {
                float currentPressure = (float)FlightGlobals.getStaticPressure(this.part.transform.position);
                if (currentPressure > minAtmoPressure && currentPressure < maxAtmoPressure)
                {
                    Debug.Log("Inflating Balloon!");
                    this.part.Effect("inflate");
                    speedAdjust = 1;
                    isInflating = true;
                    capObject.SetActive(false);
                    Events["inflateBalloon"].active = false;
                    Events["deflateBalloon"].active = true;
                }
                else
                {
                    
                    if (currentPressure <= 0)
                    {
                        ScreenMessages.PostScreenMessage("Cannot inflate balloon in vacuum", 3, ScreenMessageStyle.UPPER_CENTER);
                    }
                    else if (currentPressure < minAtmoPressure)
                    {
                        ScreenMessages.PostScreenMessage("Cannot Inflate: Air pressure too low", 3, ScreenMessageStyle.UPPER_CENTER);
                    }
                    else if (currentPressure> maxAtmoPressure)
                    {
                        ScreenMessages.PostScreenMessage("Cannot Inflate: Air pressure too high", 3, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
            }
        }

        [KSPEvent(active = false, guiActive = true, guiActiveEditor = false, guiActiveUnfocused = false, guiName = "Deflate Balloon")]
        public void deflateBalloon()
        {
            if (isInflated)
            {
                Debug.Log("Deflating Balloon!");
                if (!hasBurst) { this.part.Effect("deflate"); }
                Events["deflateBalloon"].active = false;
                isInflated = false;
                isDeflating = true;
            }
        }

        [KSPAction("Inflate Balloon")]
        public void inflateAction(KSPActionParam param)
        {
            inflateBalloon();
            Actions["inflateAction"].active = false;
        }

        [KSPAction("Deflate Balloon")]
        public void deflateAction(KSPActionParam param)
        {
            deflateBalloon();
            Actions["deflateAction"].active = false;
        }

        static public GameObject getChildGameObject(GameObject fromGameObject, string withName)
        {
            Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>();
            foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
            return null;
        }

        public override string GetInfo()
        {
            //I know about FlightGlobal.Bodies() but for some reason when I use it in this function the game freezes on load
            //Also it can't be put in OnStart() because that isn't called until the part is created
            bodyName = recommendedBody;
            if (recommendedBody == "Sun") bodyG = 17.1f;
            if (recommendedBody == "Kerbin") bodyG = 9.81f;
            if (recommendedBody == "Mun") bodyG = 1.63f;
            if (recommendedBody == "Minmus") bodyG = 0.491f;
            if (recommendedBody == "Moho") bodyG = 2.70f;
            if (recommendedBody == "Eve") bodyG = 16.7f;
            if (recommendedBody == "Duna") bodyG = 2.94f;
            if (recommendedBody == "Ike") bodyG = 1.10f;
            if (recommendedBody == "Jool") bodyG = 7.85f;
            if (recommendedBody == "Laythe") bodyG = 7.85f;
            if (recommendedBody == "Vall") bodyG = 2.31f;
            if (recommendedBody == "Bop") bodyG = 0.589f;
            if (recommendedBody == "Tylo") bodyG = 7.85f;
            if (recommendedBody == "Gilly") bodyG = 0.049f;
            if (recommendedBody == "Pol") bodyG = 0.373f;
            if (recommendedBody == "Dres") bodyG = 1.13f;
            if (recommendedBody == "Eeloo") bodyG = 1.69f;

            string moreInfoText;
            moreInfoText = "Recommended Body: " + bodyName;
            moreInfoText = moreInfoText + "\nMin pressure: " + minAtmoPressure.ToString() + "kPa";
            moreInfoText = moreInfoText + "\nMax pressure: " + maxAtmoPressure.ToString() + "kPa";
            moreInfoText = moreInfoText + "\nMax lift: " + maxLift.ToString() + "kN";
            moreInfoText = moreInfoText + "\nMax payload " + "(" + bodyName + "):\n" + (Mathf.Floor((maxLift / bodyG) * 1000) / 1000).ToString() + "t" + " (at " + maxAtmoPressure + "kPa)";// + " (" + (Mathf.Floor((minLift / bodyG) * 1000) / 1000).ToString() + "t)";
            //moreInfoText = moreInfoText + "\n  At max pressure: " + (Mathf.Floor((maxLift/bodyG)*1000)/1000).ToString() + "t";
            //moreInfoText = moreInfoText + "\n  At min pressure: " + (Mathf.Floor((minLift / bodyG) * 1000) / 1000).ToString() + "t";
            return moreInfoText;
        }

    }
}
