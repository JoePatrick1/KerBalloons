using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP;

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
        public float targetTWR; //=1.1
        [KSPField(isPersistant = false)]
        public float liftLimit; //=1
        [KSPField(isPersistant = false)]
        public bool speedLimiter; //=true
        [KSPField(isPersistant = false)]
        public float maxSpeed; //=5
        [KSPField(isPersistant = false)]
        public float maxSpeedTolerence; //=0.05
        [KSPField(isPersistant = false)]
        public float speedAdjustStep; //=0.01
        [KSPField(isPersistant = false)]
        public float speedAdjustMin; //=0.9
        [KSPField(isPersistant = false)]
        public float speedAdjustMax; //=1.1

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
        public float scaleInc;
        [KSPField(isPersistant = true)]
        public float initMass;

        [KSPField(isPersistant = false)]
        public string bodyName;
        [KSPField(isPersistant = false)]
        public float bodyG;
 

        public override void OnStart(StartState state)
        {
            Debug.Log("KerBalloons Loaded");

            if (HighLogic.LoadedSceneIsFlight)
            {
                balloonObject = getChildGameObject(this.part.gameObject, CFGballoonObject);
                ropeObject = getChildGameObject(this.part.gameObject, CFGropeObject);
                capObject = getChildGameObject(this.part.gameObject, CFGcapObject);
                liftPointObject = getChildGameObject(this.part.gameObject, CFGliftPointObject);
                balloonPointObject = getChildGameObject(this.part.gameObject, CFGballoonPointObject);

                balloonObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            }
        }


        public void FixedUpdate()
        {

            if (HighLogic.LoadedSceneIsFlight)
            {

                float currentPressure = (float)FlightGlobals.getStaticPressure(this.part.transform.position);
                if (hasInflated && !hasBurst)
                {
                    if (isInflated)
                    {
                        float lift = getLift(currentPressure);
                        this.part.Rigidbody.AddForceAtPosition(vessel.upAxis * lift,liftPointObject.transform.position);
                        Vector3 scale = new Vector3(getScale(currentPressure), getScale(currentPressure), getScale(currentPressure));
                        balloonObject.transform.localScale = scale;

                        ropeObject.transform.rotation = Quaternion.Slerp(ropeObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), getLift(currentPressure) / 10);
                        balloonObject.transform.rotation = Quaternion.Slerp(balloonObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), getLift(currentPressure) / 8);

                        balloonObject.transform.position = balloonPointObject.transform.position;

                        if (currentPressure < minAtmoPressure) hasBurst = true;
                    }
                    else if (isDeflating)
                    {
                        if (scaleInc > 0)
                        {
                            scaleInc -= getScale(currentPressure) / 100;
                            balloonObject.transform.localScale = new Vector3(scaleInc, scaleInc, scaleInc);

                            float progress = scaleInc / getScale(currentPressure);

                            float lift = getLift(currentPressure) * progress;
                            this.part.Rigidbody.AddForceAtPosition(vessel.upAxis * lift, liftPointObject.transform.position);

                            ropeObject.transform.localScale = new Vector3(1, 1, progress);
                            balloonObject.transform.position = balloonPointObject.transform.position;
                        }
                        else
                        {
                            isInflated = false;
                            isDeflating = false;
                        }
                    }
                }
                else if (isInflating && !hasBurst)
                {
                    if (scaleInc < getScale(currentPressure))
                    {
                        scaleInc += getScale(currentPressure)/200;
                        balloonObject.transform.localScale = new Vector3(scaleInc, scaleInc, scaleInc);

                        float progress = scaleInc / getScale(currentPressure);

                        float lift = getLift(currentPressure) * progress;
                        this.part.Rigidbody.AddForceAtPosition(vessel.upAxis * lift, liftPointObject.transform.position);


                        ropeObject.transform.rotation = Quaternion.Slerp(ropeObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), getLift(currentPressure) / 10);
                        balloonObject.transform.rotation = Quaternion.Slerp(balloonObject.transform.rotation, Quaternion.LookRotation(vessel.upAxis, vessel.upAxis), getLift(currentPressure) / 8);

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
                    Destroy(balloonObject);
                    Destroy(ropeObject);
                    Events["inflateBalloon"].active = false;
                    Events["deflateBalloon"].active = false;
                    Actions["inflateAction"].active = false;
                    Actions["deflateAction"].active = false;
                }
            }
            
        }

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiActiveUnfocused = false, guiName = "Inflate Balloon")]
        public void inflateBalloon()
        {
            if (!isInflated)
            {
                if (FlightGlobals.getStaticPressure() > minAtmoPressure)
                {
                    Debug.Log("Inflating Balloon!");
                    this.part.Effect("inflate");
                    isInflating = true;
                    Destroy(capObject);
                    Events["inflateBalloon"].active = false;
                    Events["deflateBalloon"].active = true;
                    initMass = this.part.vessel.GetTotalMass();
                }
                else
                {
                    if (FlightGlobals.getStaticPressure() <= 0)
                    {
                        ScreenMessages.PostScreenMessage("Cannot inflate balloon in vacuum", 3, ScreenMessageStyle.UPPER_CENTER);
                    }
                    else if (FlightGlobals.getStaticPressure() < minAtmoPressure)
                    {
                        ScreenMessages.PostScreenMessage("Air pressure too low", 3, ScreenMessageStyle.UPPER_CENTER);
                    }
                    else if (FlightGlobals.getStaticPressure() > maxAtmoPressure)
                    {
                        ScreenMessages.PostScreenMessage("Air pressure too high", 3, ScreenMessageStyle.UPPER_CENTER);
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

        public float speedAdjust = 1;
        public float getLift(float atmoPressure)
        {
            float coefficient = (minLift-maxLift) / Mathf.Pow(maxAtmoPressure,2);
            float x = Mathf.Pow(atmoPressure - maxAtmoPressure - minAtmoPressure,2);
            float yInt = maxLift;

            float lift = coefficient * x + yInt;
            liftLimit = (this.vessel.GetTotalMass() * bodyG) / lift;
            lift *= liftLimit*targetTWR;

            if (speedLimiter)
            {
                if (this.vessel.srf_velocity.magnitude < maxSpeed*(1-maxSpeedTolerence)) speedAdjust += speedAdjustStep * (maxSpeed - (float)this.vessel.srf_velocity.magnitude);
                else if (this.vessel.srf_velocity.magnitude > maxSpeed*(1+maxSpeedTolerence)) speedAdjust -= speedAdjustStep * ((float)this.vessel.srf_velocity.magnitude - maxSpeed);
                speedAdjust = Mathf.Clamp(speedAdjust, speedAdjustMin, speedAdjustMax);
                lift *= speedAdjust;
            }
            

            return lift;
        }

        public float getScale(float atmoPressure)
        {
            float coefficient = (maxScale - minScale) / Mathf.Pow(maxAtmoPressure, 2);
            float x = Mathf.Pow(atmoPressure - maxAtmoPressure - minAtmoPressure, 2);
            float yInt = minScale;

            float scale = coefficient * x + yInt;
            return scale;
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
            moreInfoText = moreInfoText + "\nMax payload " + "(" + bodyName + "): " + (Mathf.Floor((minLift / bodyG) * 1000) / 1000).ToString() + "t"; ;
            //moreInfoText = moreInfoText + "\n  At max pressure: " + (Mathf.Floor((maxLift/bodyG)*1000)/1000).ToString() + "t";
            //moreInfoText = moreInfoText + "\n  At min pressure: " + (Mathf.Floor((minLift / bodyG) * 1000) / 1000).ToString() + "t";
            return moreInfoText;
        }

    }
}
