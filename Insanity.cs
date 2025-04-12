using MelonLoader;
using System.Collections;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs;
using UnityEngine.AI;
using ScheduleOne.Persistence;
using ScheduleOne.VoiceOver;
using FishNet.Object;
using ScheduleOne.Property;
using ScheduleOne.Building.Doors;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;

[assembly: MelonInfo(typeof(Insanity.Insanity), Insanity.BuildInfo.Name, Insanity.BuildInfo.Version, Insanity.BuildInfo.Author, Insanity.BuildInfo.DownloadLink)]
[assembly: MelonColor()]
[assembly: MelonOptionalDependencies("FishNet.Runtime")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace Insanity
{
    public static class BuildInfo
    {
        public const string Name = "Insanity";
        public const string Description = "Nothing is real";
        public const string Author = "XOWithSauce";
        public const string Company = null;
        public const string Version = "1.1";
        public const string DownloadLink = null;
    }

    public class Insanity : MelonMod
    {
        public static List<object> coros = new();
        private bool registered = false;
        private Player playerMain;
        private AudioSource doorKnock;
        private AudioClip footSteps;
        public List<string> npcIds = new()
        {
            "ming",
            "jessi_waters"
        };

        public Dictionary<string, Dictionary<string, Vector3>> CharacterLocations = new()
        {
            {
                "Motel Room", new Dictionary<string, Vector3>()
                {
                    { "MotelWindow", new Vector3(-67.4f, 1.3f, 86f) },
                    { "MotelDoor", new Vector3(-67.04f, 1.3f, 82.6f) }
                }
            },
            {
                "Sweatshop", new Dictionary<string, Vector3>()
                {
                    { "SweatWindowMain", new Vector3(-55.16f, 0.3f, 140.1f) },
                    { "SweatDoor", new Vector3(-64.29f, -0.4f, 141.84f) },
                    { "SweatStreet1", new Vector3(-57.23f, -3.4f, 123.22f) },
                    { "SweatHighCorner", new Vector3(-56.18f, 0.3f, 115.25f) }
                }
            },
            {
                "Bungalow", new Dictionary<string, Vector3>()
                {
                    { "BungalowDoorMain", new Vector3(-169.18f, -3.7f, 113.82f) },
                    { "BungalowDoorBack", new Vector3(-176.87f, -3.7f, 110.26f) },
                    { "BungalowWindowKitchen1", new Vector3(-176.77f, -3.6f, 118.66f) },
                }
            },
            {
                "Barn", new Dictionary<string, Vector3>()
                {
                    { "BarnUnderStairs", new Vector3(200.8111f, 0.1f, -10.9124f) },
                    { "BarnFloating", new Vector3(187.9814f, 3.8f, -13.52317f) },
                    { "BarnWoods", new Vector3(174.0903f, y: -0.1f, z: 3.852638f) },
                    { "BarnRoad", new Vector3(x: 159.279f, y: -0.2f, z: -12.15506f) }
                }
            },
            {
                "Docks Warehouse", new Dictionary<string, Vector3>()
                {
                    { "DocksOutCorner", new Vector3(x: -83.82198f, y: -2.1f, z: -35.59542f) },
                    { "DocksOnPillar", new Vector3(x: -88.41868f, y: 0.5f, z: -61.17904f) },
                    { "DocksOnRoof", new Vector3(x: -47.33374f, y: 4.1f, z: -68.83347f) },
                }
            },
            {
                "Taco Ticklers", new Dictionary<string, Vector3>()
                {
                    { "TacoCan", new Vector3(-27.67904f, 0.23f, 75.21258f) },
                    { "TacoBackDoor", new Vector3(-31.239f, 0.23f, 84.10281f) },
                    { "TacoFrontWindow", new Vector3(-26.84878f, 0.23f, 63.86666f) },
                    { "TacoRunToStaff", new Vector3(-34.48309f, 0.23f, 76.22535f) },

                }
            },
            {
                "Laundromat", new Dictionary<string, Vector3>()
                {
                    { "LaundromatDoor", new Vector3(-23.53883f, 0.3f, 24.97166f) },
                    { "LaundromatCreep", new Vector3(-24.18913f, 0.6f, 23.33677f) },
                    { "LaundromatBoxes", new Vector3(-10.6364f, 0.8f, 27.53411f) },
                }
            },
            {
                "Car Wash", new Dictionary<string, Vector3>()
                {
                    { "CarWashBusStop", new Vector3(-23.94017f, 0.2f, -24.21171f) },
                    { "CarWashBackDoor", new Vector3(-3.334698f, 0.35f, -22.07097f) },
                    { "CarWashRunToWindow", new Vector3(-6.237535f, 0.35f, -15.59119f ) },
                }
            },
            {
                "Post Office", new Dictionary<string, Vector3>()
                {
                    { "PostOfficeWindow1", new Vector3(52.59536f, 0.3f, -1.74566f) },
                    { "PostOfficeWindow2", new Vector3(42.58666f, 0.3f, -1.472795f) },
                    { "PostOfficeBack", new Vector3(45.09676f, 0.2f, -8.674371f ) },
                    { "PostOfficeStreet", new Vector3(41.80392f, 0.2f, 8.459332f ) },
                }
            },
        };
        public Dictionary<string, Dictionary<string, List<Vector3>>> CharacterSpecials = new()
        {
            {
                "Taco Ticklers", new Dictionary<string, List<Vector3>>()
                {
                    { "TacoRunToStaff", new List<Vector3>()
                        {
                        new Vector3(-34.48309f, 0.23f, 76.22535f),
                        new Vector3(-34.48309f, 0.23f, 79.53841f)
                        }
                    },
                }
            },
            {
                "Car Wash", new Dictionary<string, List<Vector3>>()
                {
                    { "CarWashRunToWindow", new List<Vector3>()
                        {
                        new Vector3(-9.058268f, 0.35f, -10.47236f),
                        new Vector3(-6.237535f, 0.35f, -15.59119f)
                        }
                    },
                }
            },
        };

        private void OnLoadCompleteCb()
        {
            if (registered) return;
            coros.Add(MelonCoroutines.Start(this.Setup()));
            registered = true;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (buildIndex == 1)
            {
                if (LoadManager.Instance != null && !registered)
                {
                    LoadManager.Instance.onLoadComplete.AddListener(OnLoadCompleteCb);
                }
            }
            else
            {
                if (LoadManager.Instance != null && registered)
                {
                    LoadManager.Instance.onLoadComplete.RemoveListener(OnLoadCompleteCb);
                }
                registered = false;

                foreach (object coro in coros)
                {
                    MelonCoroutines.Stop(coro);
                }
                coros.Clear();
            }
        }
        public IEnumerator Setup()
        {
            yield return new WaitForSeconds(5f);

            playerMain = GameObject.FindObjectOfType<Player>(true);
            yield return new WaitForSeconds(1f);

            doorKnock = GameObject.FindObjectOfType<DoorKnocker>(true).KnockingSound;
            yield return new WaitForSeconds(1f);

            footSteps = GameObject.FindObjectOfType<FootstepSounds>(true).soundGroups[0].clips[0];
            yield return new WaitForSeconds(1f);

            coros.Add(MelonCoroutines.Start(RunInsanity()));
        }

        private NPC SpawnAndInitializeNPC(string characterId, KeyValuePair<string, Vector3> kvp)
        {
            NPC baseNpc = NPCManager.GetNPC(characterId);

            GameObject go = GameObject.Instantiate(baseNpc.gameObject, NPCManager.Instance.NPCContainer);
            if (go == null)
            {
                //MelonLogger.Warning($"Failed to instantiate clone for {characterId}.");
                return null;
            }

            NPC myNpc = go.GetComponent<NPC>();
            string uniqueId = $"test_{characterId}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            myNpc.ID = uniqueId;
            myNpc.FirstName = "";
            myNpc.LastName = "";
            NavMeshAgent nma = myNpc.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (nma != null)
            {
                UnityEngine.Object.Destroy(nma);
            }
            Rigidbody rb = myNpc.GetComponent<Rigidbody>();
            if (rb != null)
            {
                UnityEngine.Object.Destroy(rb);
            }
            NetworkObject netObj = myNpc.GetComponent<FishNet.Object.NetworkObject>();
            if (netObj != null)
                netObj.enabled = false;

            myNpc.behaviour.enabled = false;
            myNpc.CanOpenDoors = false;
            myNpc.Movement.enabled = false;
            myNpc.PlayerConversant = playerMain.NetworkObject;

            go.SetActive(true);
            myNpc.SetVisible(true);
            myNpc.Avatar.Effects.SetSicklySkinColor();
            myNpc.Avatar.Effects.SetZombified(true);
            myNpc.Avatar.EmotionManager.AddEmotionOverride("Zombie", "deal_rejected", 30f, 0);
            // Register with NPCManager
            NPCManager.NPCRegistry.Add(myNpc);
            go.transform.position = kvp.Value;
            Vector3 direction = playerMain.transform.position - myNpc.transform.position;
            direction.y = 0;
            go.transform.rotation = Quaternion.LookRotation(direction);

            return myNpc;
        }

        public IEnumerator RunInsanity()
        {
            yield return new WaitForSeconds(10f);
            for (; ; )
            {
                yield return new WaitForSeconds(30f);
                if (!registered) yield break;

                //MelonLogger.Msg("PollInsanity");
                int currTime = TimeManager.Instance.CurrentTime;
                EDay currentDay = TimeManager.Instance.CurrentDay;
                if (currTime > 2000) // Start at 8 pm
                {
                    for (; ; )
                    {
                        yield return new WaitForSeconds(UnityEngine.Random.Range(25f, 45f));
                        if (!registered) yield break;

                        //MelonLogger.Msg("Evaluate Event");
                        EDay updatedDay = TimeManager.Instance.CurrentDay;
                        int updatedTime = TimeManager.Instance.CurrentTime;
                        if (currentDay != updatedDay && updatedTime > 0500)
                        {
                            //MelonLogger.Msg("End condition met");
                            break;
                        }

                        // Evaluate property
                        if (playerMain.CurrentProperty != null)
                        {
                            if (CharacterLocations.ContainsKey(playerMain.CurrentProperty.PropertyName))
                            {
                                string characterId = npcIds[UnityEngine.Random.Range(0, npcIds.Count)];
                                CharacterLocations.TryGetValue(playerMain.CurrentProperty.PropertyName, out Dictionary<string, Vector3> positions);

                                int roll = UnityEngine.Random.Range(0, positions.Count);
                                KeyValuePair<string, Vector3> kvpair = positions.ElementAt(roll);

                                NPC spawnedNpc = SpawnAndInitializeNPC(characterId, kvpair);
                                if (spawnedNpc != null)
                                {
                                    //MelonLogger.Msg($"{characterId} spawned at {kvpair.Key} ({kvpair.Value}).");
                                    MelonCoroutines.Start(EventCoro(spawnedNpc, kvpair, playerMain.CurrentProperty));
                                }
                            }
                        }

                        // Evaluate business
                        if (playerMain.CurrentBusiness != null)
                        {
                            string characterId = npcIds[UnityEngine.Random.Range(0, npcIds.Count)];
                            if (CharacterSpecials.ContainsKey(playerMain.CurrentBusiness.PropertyName) && UnityEngine.Random.Range(0f, 1f) > 0.8f)
                            {
                                CharacterSpecials.TryGetValue(playerMain.CurrentBusiness.PropertyName, out Dictionary<string, List<Vector3>> positions);
                                KeyValuePair<string, List<Vector3>> kvpair = positions.ElementAt(0);

                                KeyValuePair<string, Vector3> kvStart = new KeyValuePair<string, Vector3>(kvpair.Key, kvpair.Value[0]);
                                NPC spawnedNpc = SpawnAndInitializeNPC(characterId, kvStart);
                                if (spawnedNpc != null)
                                {
                                    //MelonLogger.Msg($"{characterId} spawned at {kvpair.Key} ({kvpair.Value}).");
                                    MelonCoroutines.Start(MoveAlongPath(spawnedNpc, kvpair.Value, 5f));
                                }
                            }
                            else if (CharacterLocations.ContainsKey(playerMain.CurrentBusiness.PropertyName))
                            {
                                CharacterLocations.TryGetValue(playerMain.CurrentBusiness.PropertyName, out Dictionary<string, Vector3> positions);

                                int roll = UnityEngine.Random.Range(0, positions.Count);
                                KeyValuePair<string, Vector3> kvpair = positions.ElementAt(roll);

                                NPC spawnedNpc = SpawnAndInitializeNPC(characterId, kvpair);
                                if (spawnedNpc != null)
                                {
                                    //MelonLogger.Msg($"{characterId} spawned at {kvpair.Key} ({kvpair.Value}).");
                                    Property property = playerMain.CurrentBusiness as Property;
                                    MelonCoroutines.Start(EventCoro(spawnedNpc, kvpair, property));
                                }
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator EventCoro(NPC npc, KeyValuePair<string, Vector3> kvPair, Property property)
        {

            if (kvPair.Key.Contains("Door"))
            {
                if (UnityEngine.Random.Range(0, 100) > 50)
                    npc.CanOpenDoors = true;

                GameObject knocking = new GameObject("knockingSound");
                AudioSource audioSource = knocking.AddComponent<AudioSource>();
                audioSource.clip = doorKnock.clip;
                audioSource.spatialBlend = 1f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                try
                {
                    knocking.transform.position = npc.transform.position;
                }
                catch (Exception ex)
                {
                    MelonLogger.Error(ex);
                }
                audioSource.Play();
                yield return new WaitForSeconds(0.77f);
                audioSource.pitch = UnityEngine.Random.Range(0.7f, 1f);
                audioSource.Play();
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.9f, 1.6f));
                audioSource.pitch = UnityEngine.Random.Range(0.6f, 0.8f);
                audioSource.volume = 0.8f;
                audioSource.Play();

                GameObject.Destroy(knocking, audioSource.clip.length);
            }


            int ranEvents = 0;
            for (; ; )
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(3f, 6f));
                if (!registered) yield break;

                if (ranEvents >= 2) break;
                ranEvents++;

                if (UnityEngine.Random.Range(0f, 1f) > 0.8f) { continue; }

                try
                {
                    if (npc.transform.position != kvPair.Value)
                    {
                        //MelonLogger.Msg("Invalid pos");
                        npc.transform.position = kvPair.Value;
                    }
                    Vector3 direction = playerMain.transform.position - npc.transform.position;
                    direction.y = 0;
                    npc.transform.rotation = Quaternion.LookRotation(direction);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error(ex);
                }


                int roll = UnityEngine.Random.Range(0, 8);
                switch (roll)
                {
                    case 0:
                        //MelonLogger.Msg("AudioEvent 1");
                        try
                        {
                            npc.PlayVO(EVOLineType.Grunt);
                        }
                        catch (Exception ex)
                        {
                            MelonLogger.Error(ex);
                        }
                        continue;

                    case 1:
                        //MelonLogger.Msg("AudioEvent 2");
                        try
                        {
                            npc.PlayVO(EVOLineType.Die);
                        }
                        catch (Exception ex)
                        {
                            MelonLogger.Error(ex);
                        }
                        continue;

                    case 2:
                        //MelonLogger.Msg("AudioEvent 3");
                        try
                        {
                            npc.PlayVO(EVOLineType.Scared);
                        }
                        catch (Exception ex)
                        {
                            MelonLogger.Error(ex);
                        }
                        continue;

                    case 3:
                        //MelonLogger.Msg("LightEvent 1");
                        Light[] propertyLights = property.transform.GetComponentsInChildren<Light>(true);
                        List<Color> colors = new();
                        List<float> intensiti = new();
                        foreach (Light l in propertyLights)
                        {
                            yield return new WaitForSeconds(0.005f);
                            colors.Add(l.color);
                            intensiti.Add(l.intensity);
                            if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                                l.color = Color.red;
                            else
                                l.intensity = 0.1f;
                        }
                        yield return new WaitForSeconds(3f);
                        if (!registered) yield break;
                        for (int i = 0; i < propertyLights.Length; i++)
                        {
                            yield return new WaitForSeconds(0.005f);
                            propertyLights[i].color = colors[i];
                            propertyLights[i].intensity = intensiti[i];
                        }
                        continue;

                    case 4:
                        //MelonLogger.Msg("LightEvent 2");
                        Light[] propertyLights2 = property.transform.GetComponentsInChildren<Light>(true);
                        List<float> intensities = new();

                        foreach (Light l in propertyLights2)
                        {
                            yield return new WaitForSeconds(0.005f);
                            intensities.Add(l.intensity);
                            l.intensity = 0.1f;
                        }
                        yield return new WaitForSeconds(3f);
                        if (!registered) yield break;

                        for (int i = 0; i < propertyLights2.Length; i++)
                        {
                            yield return new WaitForSeconds(0.005f);
                            propertyLights2[i].intensity = intensities[i];
                        }

                        continue;

                    case 5:
                        if (UnityEngine.Random.Range(0, 100) > 10 || Singleton<MusicPlayer>.Instance.IsPlaying)
                            continue;

                        //MelonLogger.Msg("SchizoMusicEvent");
                        Singleton<MusicPlayer>.Instance.SetMusicDistorted(true, 5f);
                        Singleton<MusicPlayer>.Instance.SetTrackEnabled("Schizo music", true);
                        Singleton<AudioManager>.Instance.SetDistorted(true, 5f);
                        Singleton<PostProcessingManager>.Instance.SaturationController.AddOverride(110f, 7, "Schizophrenic");
                        continue;

                    case 6:
                        //MelonLogger.Msg("RedEyes Event");
                        try
                        {
                            npc.transform.rotation = Quaternion.LookRotation(playerMain.transform.position - npc.transform.position);
                            npc.Avatar.SetEmission(Color.red);
                            npc.Avatar.Effects.SetEyeLightEmission(1f, Color.red);
                        }
                        catch (Exception ex)
                        {
                            MelonLogger.Error(ex);
                        }
                        continue;

                    case 7:
                        //MelonLogger.Msg("Footstep event");
                        Vector3 backward = -playerMain.transform.forward.normalized;
                        int stepCount = UnityEngine.Random.Range(5, 9);
                        GameObject footsteps = new GameObject($"stepSounds");
                        AudioSource audioSource = footsteps.AddComponent<AudioSource>();
                        audioSource.clip = footSteps;
                        audioSource.spatialBlend = 1f;
                        audioSource.rolloffMode = AudioRolloffMode.Linear;
                        footsteps.transform.position = playerMain.transform.position + backward * 2;
                        float volMax = 0.8f;
                        float volMin = 0.7f;
                        for (int i = 0; i < stepCount; i++)
                        {
                            if (!registered) yield break;
                            audioSource.pitch = UnityEngine.Random.Range(0.8f, 1f);
                            audioSource.volume = UnityEngine.Random.Range(volMin, volMax);
                            volMax -= 0.07f;
                            volMin -= 0.07f;
                            audioSource.Play();
                            footsteps.transform.position += backward * 2f;

                            yield return new WaitForSeconds(UnityEngine.Random.Range(0.7f, 0.9f));
                        }
                        GameObject.Destroy(footsteps);

                        continue;

                }
            }

            //MelonLogger.Msg("Cleaning up");
            Singleton<MusicPlayer>.Instance.SetMusicDistorted(false, 5f);
            Singleton<MusicPlayer>.Instance.SetTrackEnabled("Schizo music", false);
            Singleton<AudioManager>.Instance.SetDistorted(false, 5f);
            Singleton<PostProcessingManager>.Instance.SaturationController.RemoveOverride("Schizophrenic");

            NPCManager.NPCRegistry.Remove(npc);
            GameObject.Destroy(npc.gameObject);
        }

        private IEnumerator MoveAlongPath(NPC npc, List<Vector3> path, float durationPerSegment)
        {
            //MelonLogger.Msg("Moving npc");
            if (path == null || path.Count < 2)
                yield break;

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 start = path[i];
                Vector3 end = path[i + 1];
                float elapsed = 0f;

                while (elapsed < durationPerSegment)
                {
                    yield return new WaitForSeconds(0.02f);
                    if (!registered) yield break;

                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / durationPerSegment);
                    try
                    {
                        npc.transform.position = Vector3.Lerp(start, end, t);
                        Vector3 direction = playerMain.transform.position - npc.transform.position;
                        direction.y = 0;
                        npc.transform.rotation = Quaternion.LookRotation(direction);
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error(ex);
                    }
                }
                try
                {
                    npc.transform.position = end;
                    Vector3 direction = playerMain.transform.position - npc.transform.position;
                    direction.y = 0;
                    npc.transform.rotation = Quaternion.LookRotation(direction);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error(ex);
                }
                yield return new WaitForSeconds(1f);
                try
                {
                    NPCManager.NPCRegistry.Remove(npc);
                    GameObject.Destroy(npc.gameObject);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error(ex);
                }

            }

        }

    }
}
