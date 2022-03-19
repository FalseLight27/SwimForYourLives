using System;
using System.Collections.Generic;
using System.Collections;
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
        public float maxDist = 60f;
        
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
            var cft = __instance.gameObject.EnsureComponent<CheckForThreats>();
            __instance.gameObject.EnsureComponent<ListOfCreatures>();
            bool isPeeper = __instance.GetComponentInChildren<Peeper>();            
            
            cft.isLeviathan = __instance.GetComponentInChildren<GhostLeviathan>() || __instance.GetComponentInChildren<GhostLeviatanVoid>() || __instance.GetComponentInChildren<ReaperLeviathan>() || __instance.GetComponentInChildren<SeaDragon>();
            cft.isReefBack = __instance.GetComponentInChildren<Reefback>();

            if (cft.isLeviathan)
            {
                ListOfCreatures.CreatureList.Add(__instance);
                
            }
            
        }
    }

    [HarmonyPatch(typeof(Creature), nameof(Creature.OnKill))]

    public class Remover
    {


        [HarmonyPostfix]
        public static void Remove(Creature __instance)
        {
            var cft = __instance.gameObject.EnsureComponent<CheckForThreats>();

            if (cft.isLeviathan)
            {
                ListOfCreatures.CreatureList.Remove(__instance);
              
            }
        }
    }

    [HarmonyPatch(typeof(Creature), nameof(Creature.OnDestroy))]

    public class Remover2
    {


        [HarmonyPostfix]
        public static void Remove(Creature __instance)
        {
            var cft = __instance.gameObject.EnsureComponent<CheckForThreats>();

            if (cft.isLeviathan)
            {
                ListOfCreatures.CreatureList.Remove(__instance);

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
            var gas = __instance.GetComponentInChildren<GasoPod>();

            
            Vector3 vector;
            Vector3 targetPosition;

            Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 1");

            float swimVelocity = 7f;
            float maxDistToThreat = 50f;
            bool isPrey = !cft.isLeviathan && !cft.isReefBack;            

            Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 1.5");

            GameObject lev = loc.FindNearbyHostile(__instance);                                    

            Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 2");

            IEnumerator DropGasPods()

            {
                float randomCD = UnityEngine.Random.Range(3f, 6f);
                float panicPlaceHolder = UnityEngine.Random.Range(0, 2f);
                if (panicPlaceHolder > 1f || vector.magnitude < 30f)
                {
                    gas.DropGasPods();
                }

                yield return new WaitForSeconds(randomCD);

                gas.DropGasPods();
            }

            if (isPrey) 

            {
                
                Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 3");

                if (lev != null)                    

                {
                    Logger.Log(Logger.Level.Debug, $"FLY YOU FOOL");
                    vector = __instance.transform.position - lev.transform.position;
                    vector.y = Mathf.Clamp(vector.y, __instance.transform.position.y * 10, __instance.transform.position.y *-10);
                    Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 3.5");
                    targetPosition = __instance.transform.position + vector.normalized * maxDistToThreat;
                    Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 4");

                    
                    swim.SwimTo(targetPosition, swimVelocity);

                    if (vector.magnitude < 50f)
                    {
                        __instance.Scared.UpdateTrait(Time.deltaTime);

                        if (gas)

                        {
                            CoroutineHost.StartCoroutine(DropGasPods());
                        }
                    }

                    Logger.Log(Logger.Level.Debug, $"RUNAWAY PASSED CHECK 5");
                    Logger.Log(Logger.Level.Debug, $"I AM {Vector3.Distance(__instance.transform.position, lev.transform.position)} away from the closest leviathan");
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




