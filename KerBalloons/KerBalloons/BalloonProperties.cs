using System.Collections.Generic;
using UnityEngine;

namespace KerBalloons
{
    public class BalloonProperties
    { 
        public static float getLift(ModuleKerBalloon thisBalloon)
        {
            
            float atmoPressure = (float)FlightGlobals.getStaticPressure(thisBalloon.part.transform.position);
            float coefficient = (thisBalloon.minLift - thisBalloon.maxLift) / Mathf.Pow(thisBalloon.maxAtmoPressure, 2);
            float x = Mathf.Pow(atmoPressure - thisBalloon.maxAtmoPressure - thisBalloon.minAtmoPressure, 2);
            float yInt = thisBalloon.maxLift;

            float lift = coefficient * x + yInt;
            float max = lift;

            float liftLimit = (thisBalloon.vessel.GetTotalMass() * (float)FlightGlobals.getGeeForceAtPosition(thisBalloon.transform.position).magnitude) / lift;
            lift *= liftLimit * thisBalloon.targetTWR;
            

            if (thisBalloon.speedLimiter)
            {
                if (thisBalloon.vessel.verticalSpeed < thisBalloon.maxSpeed * (1 - thisBalloon.maxSpeedTolerence))
                {
                    thisBalloon.speedAdjust += thisBalloon.speedAdjustStep * (thisBalloon.maxSpeed - (float)thisBalloon.vessel.verticalSpeed);
                }
                else if (thisBalloon.vessel.verticalSpeed > thisBalloon.maxSpeed * (1 + thisBalloon.maxSpeedTolerence))
                {
                    thisBalloon.speedAdjust -= thisBalloon.speedAdjustStep * ((float)thisBalloon.vessel.verticalSpeed - thisBalloon.maxSpeed);
                }
                thisBalloon.speedAdjust = Mathf.Clamp(thisBalloon.speedAdjust, thisBalloon.speedAdjustMin, thisBalloon.speedAdjustMax);
                lift *= thisBalloon.speedAdjust;
            }
            if(thisBalloon.isInflated || thisBalloon.isInflating) lift /= getInflatedBalloons(thisBalloon.vessel).Count;
            lift = Mathf.Clamp(lift, 0, max);
            return lift;
        }


        public static float getScale(ModuleKerBalloon thisBalloon)
        {
            float atmoPressure = (float)FlightGlobals.getStaticPressure(thisBalloon.part.transform.position);
            float coefficient = (thisBalloon.maxScale - thisBalloon.minScale) / Mathf.Pow(thisBalloon.maxAtmoPressure, 2);
            float x = Mathf.Pow(atmoPressure - thisBalloon.maxAtmoPressure - thisBalloon.minAtmoPressure, 2);
            float yInt = thisBalloon.minScale;

            float scale = coefficient * x + yInt;

            return scale;
        }

        public static List<ModuleKerBalloon> getInflatedBalloons(Vessel vessel)
        {
            List<ModuleKerBalloon> balloons = new List<ModuleKerBalloon>();
            foreach(Part part in vessel.parts)
            {
                if (part.GetComponent<ModuleKerBalloon>())
                {
                    ModuleKerBalloon balloon = part.GetComponent<ModuleKerBalloon>();
                    if ((balloon.isInflated || balloon.isInflating) && !balloon.hasBurst)
                    {
                        balloons.Add(balloon);
                    }
                }
            }
            return balloons;
        }
    }
}
