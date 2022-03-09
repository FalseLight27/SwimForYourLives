using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;
using HarmonyLib;
using QModManager.API.ModLoading;
using Logger = QModManager.Utility.Logger;

namespace SwimForYourLives
{
    public class ListOfCreatures : MonoBehaviour
    {
        public static List<Creature> CreatureList = new List<Creature>();
        public GameObject closestLev;
        public float minDist;
        public float maxDist = 100f;
        
        public GameObject FindNearbyHostile(Creature me)
        {
            if (closestLev != null)
            {
                minDist = Vector3.Distance(me.transform.position, closestLev.gameObject.transform.position);
            }

            foreach (Creature lev in CreatureList)
            {

                float dist = Vector3.Distance(lev.transform.position, me.transform.position);
                var cft = lev.GetComponentInParent<CheckForThreats>();
                if (cft.isLeviathan)
                {
                    if (((lev.gameObject != me.gameObject && dist < maxDist && dist < minDist) || lev.gameObject != me.gameObject && me.GetCanSeeObject(lev.gameObject)) && lev.gameObject != null)
                    {
                        closestLev = lev.gameObject;
                        minDist = dist;

                    }

                }
            }

            return closestLev;
        }

    }

    public class CheckForThreats : MonoBehaviour
    {
        public GameObject target;
        public GameObject ecoTargetGameObject;
        public bool isLeviathan;
        public bool isReefBack;

    }

    [HarmonyPatch(typeof(Creature))]
    [HarmonyPatch("Start")]

    public class DistinguishLeviathan
    {

        [HarmonyPostfix]
        public static void AddMarker(Creature __instance)
        {
            var cft = __instance.GetComponentInParent<CheckForThreats>();
            bool isPeeper = __instance.GetComponentInChildren<Peeper>();
            __instance.gameObject.AddComponent<ListOfCreatures>();
            __instance.gameObject.AddComponent<CheckForThreats>();
            cft.isLeviathan = __instance.GetComponentInChildren<GhostLeviathan>() || __instance.GetComponentInChildren<GhostLeviatanVoid>() || __instance.GetComponentInChildren<ReaperLeviathan>() || __instance.GetComponentInChildren<SeaDragon>();
            cft.isReefBack = __instance.GetComponentInChildren<Reefback>();

            if (cft.isLeviathan)
            {
                ErrorMessage.AddMessage("LEVIATHAN SPAWNED");
            }

            else if (!cft.isLeviathan && !cft.isReefBack && isPeeper)
            {
                ErrorMessage.AddMessage("PREY SPAWNED");
            }
        }
    }



    [HarmonyPatch(typeof(Creature), nameof(Creature.UpdateBehaviour))]

    public class FleeBehavior
    {


        [HarmonyPostfix]
        public static void RunAway(Creature __instance)
        {
            var cft = __instance.GetComponentInChildren<CheckForThreats>();
            var loc = __instance.GetComponentInChildren<ListOfCreatures>();
            var swim = __instance.GetComponentInChildren<SwimBehaviour>();
            Vector3 vector;
            Vector3 targetPosition;

            Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 1");

            float swimVelocity = 7f;
            float maxDistToThreat = 30f;
            bool isPrey = !cft.isLeviathan && !cft.isReefBack;            

            Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 1.5");

            GameObject lev = loc.FindNearbyHostile(__instance);                                    

            Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 2");           

            if (isPrey) 

            {
                
                Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 3");

                if (lev != null)                    

                {
                    Logger.Log(Logger.Level.Debug, $"FLY YOU FOOL");
                    vector = __instance.transform.position - lev.transform.position;
                    Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 3.5");
                    targetPosition = __instance.transform.position + (vector.normalized * maxDistToThreat);
                    Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 4");

                    swim.SwimTo(targetPosition, swimVelocity);

                    Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 5");
                    Logger.Log(Logger.Level.Debug, $"I AM {vector} away from the closest leviathan"); //should be vector.distance!
                }                

                else
                {
                    
                    Logger.Log(Logger.Level.Debug, $"No leviathans in vicinity");
                    return;                    

                }                

            }
        }
    }

        



        



        [QModCore]
        public static class HostilityPatcher
        {

            [QModPatch]
            public static void Patch()
            {
                var harmony = new Harmony("com.falselight.swimforyourlives");
                harmony.PatchAll();
            }

        }
}




