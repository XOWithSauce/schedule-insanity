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
        public const string Version = "1.0";
        public const string DownloadLink = null;
    }

    public class Insanity : MelonMod
    {
        public static List<object> coros = new();
        private bool registered = false;
        private Player playerMain;
        private AudioSource doorKnock;
        private AudioClip footSteps;


        public Dictionary<string, Dictionary<string, Vector3>> CharacterLocations = new()
        {
            {
                "MotelRoom", new Dictionary<string, Vector3>()
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
                    { "BungalowDoorMain", new Vector3(-169.18f, -3.3f, 113.82f) },
                    { "BungalowDoorBack", new Vector3(-176.87f, -3.3f, 110.26f) },
                    { "BungalowWindowKitchen1", new Vector3(-176.77f, -3.5f, 118.66f) },
                }
            },
            {
                "Barn", new Dictionary<string, Vector3>()
                {
                    { "BarnUnderStairs", new Vector3(200.8111f, 0.1f, -10.9124f) },
                    { "BarnFloating", new Vector3(187.9814f, 3.8f, -13.52317f) },
                    { "BarnWoods:", new Vector3(174.0903f, y: -0.1f, z: 3.852638f) },
                    { "BarnRoad", new Vector3(x: 159.279f, y: -0.2f, z: -12.15506f) }
                }
            },
            {
                "DocksWarehouse", new Dictionary<string, Vector3>()
                {
                    { "DocksOutCorner", new Vector3(x: -83.82198f, y: -1.7f, z: -35.59542f) },
                    { "DocksOnPillar", new Vector3(x: -88.41868f, y: 0.5f, z: -61.17904f) },
                    { "DocksOnRoof:", new Vector3(x: -47.33374f, y: 4.7f, z: -68.83347f) },
                }
            }
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
            yield return new WaitForSeconds(10f);
            playerMain = GameObject.FindObjectOfType<Player>(true);
            doorKnock = GameObject.FindObjectOfType<DoorKnocker>(true).KnockingSound;
            footSteps = GameObject.FindObjectOfType<FootstepSounds>(true).soundGroups.FirstOrDefault().clips.FirstOrDefault();

            MelonCoroutines.Start(RunInsanity());
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
            myNpc.FirstName = uniqueId;
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
            myNpc.dialogueHandler.enabled = false;

            NetworkObject netObj = myNpc.GetComponent<FishNet.Object.NetworkObject>();
            if (netObj != null) 
                netObj.enabled = false;

            myNpc.behaviour.enabled = false;
            myNpc.CanOpenDoors = false;
            myNpc.Movement.enabled = false;

            go.SetActive(true);
            myNpc.SetVisible(true);
            // Register with NPCManager
            NPCManager.NPCRegistry.Add(myNpc);
            go.transform.position = kvp.Value;
            Vector3 direction = playerMain.transform.position - myNpc.transform.position;
            direction.y = 0;
            go.transform.rotation = Quaternion.LookRotation(direction);
            
            return myNpc;
        }

        private IEnumerator EndEventFor(NPC npcInstance, object coro)
        {
            yield return new WaitForSeconds(15f);
            Singleton<MusicPlayer>.Instance.SetMusicDistorted(false, 5f);
            Singleton<MusicPlayer>.Instance.SetTrackEnabled("Schizo music", false);
            Singleton<AudioManager>.Instance.SetDistorted(false, 5f);
            Singleton<PostProcessingManager>.Instance.SaturationController.RemoveOverride("Schizophrenic");

            MelonCoroutines.Stop(coro);
            NPCManager.NPCRegistry.Remove(npcInstance);
            GameObject.Destroy(npcInstance.gameObject);
        }

        public IEnumerator RunInsanity()
        {
            yield return new WaitForSeconds(10f);
            for (; ; )
            {
                yield return new WaitForSeconds(30f);
                //MelonLogger.Msg("PollInsanity");
                int currTime = TimeManager.Instance.CurrentTime;
                EDay currentDay = TimeManager.Instance.CurrentDay;
                if (currTime > 2000) // Start at 8 pm
                {
                    for (; ; )
                    {
                        yield return new WaitForSeconds(30f);
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
                            if (playerMain.CurrentProperty.PropertyName == "Sweatshop")
                            {
                                string characterId = "ming";
                                CharacterLocations.TryGetValue("Sweatshop", out Dictionary<string, Vector3> positions);

                                int roll = UnityEngine.Random.Range(0, positions.Count);
                                KeyValuePair<string, Vector3> kvpair = positions.ElementAt(roll);

                                NPC spawnedNpc = SpawnAndInitializeNPC(characterId, kvpair);
                                if (spawnedNpc != null)
                                {
                                    //MelonLogger.Msg($"{characterId} spawned at {kvpair.Key} ({kvpair.Value}).");
                                    object coro = MelonCoroutines.Start(EventCoro(spawnedNpc, kvpair, playerMain.CurrentProperty));
                                    MelonCoroutines.Start(EndEventFor(spawnedNpc, coro));
                                }
                            }

                            else if (playerMain.CurrentProperty.PropertyName == "Motel Room")
                            {
                                string characterId = "jessi_waters";
                                CharacterLocations.TryGetValue("MotelRoom", out Dictionary<string, Vector3> positions);

                                int roll = UnityEngine.Random.Range(0, positions.Count);
                                KeyValuePair<string, Vector3> kvpair = positions.ElementAt(roll);

                                NPC spawnedNpc = SpawnAndInitializeNPC(characterId, kvpair);
                                if (spawnedNpc != null)
                                {
                                    //MelonLogger.Msg($"{characterId} spawned at {kvpair.Key} ({kvpair.Value}).");
                                    object coro = MelonCoroutines.Start(EventCoro(spawnedNpc, kvpair, playerMain.CurrentProperty));
                                    MelonCoroutines.Start(EndEventFor(spawnedNpc, coro));
                                }
                            }

                            else if (playerMain.CurrentProperty.PropertyName == "Bungalow")
                            {
                                string characterId = "peter_file";
                                CharacterLocations.TryGetValue("Bungalow", out Dictionary<string, Vector3> positions);

                                int roll = UnityEngine.Random.Range(0, positions.Count);
                                KeyValuePair<string, Vector3> kvpair = positions.ElementAt(roll);

                                NPC spawnedNpc = SpawnAndInitializeNPC(characterId, kvpair);
                                if (spawnedNpc != null)
                                {
                                    //MelonLogger.Msg($"{characterId} spawned at {kvpair.Key} ({kvpair.Value}).");
                                    object coro = MelonCoroutines.Start(EventCoro(spawnedNpc, kvpair, playerMain.CurrentProperty));
                                    MelonCoroutines.Start(EndEventFor(spawnedNpc, coro));
                                }
                            }

                            else if (playerMain.CurrentProperty.PropertyName == "Barn")
                            {
                                string characterId = "ming";
                                CharacterLocations.TryGetValue("Barn", out Dictionary<string, Vector3> positions);

                                int roll = UnityEngine.Random.Range(0, positions.Count);
                                KeyValuePair<string, Vector3> kvpair = positions.ElementAt(roll);

                                NPC spawnedNpc = SpawnAndInitializeNPC(characterId, kvpair);
                                if (spawnedNpc != null)
                                {
                                    //MelonLogger.Msg($"{characterId} spawned at {kvpair.Key} ({kvpair.Value}).");
                                    object coro = MelonCoroutines.Start(EventCoro(spawnedNpc, kvpair, playerMain.CurrentProperty));
                                    MelonCoroutines.Start(EndEventFor(spawnedNpc, coro));
                                }
                            }

                            else if (playerMain.CurrentProperty.PropertyName == "Docks Warehouse")
                            {
                                string characterId = "dean_webster";
                                CharacterLocations.TryGetValue("DocksWarehouse", out Dictionary<string, Vector3> positions);

                                int roll = UnityEngine.Random.Range(0, positions.Count);
                                KeyValuePair<string, Vector3> kvpair = positions.ElementAt(roll);

                                NPC spawnedNpc = SpawnAndInitializeNPC(characterId, kvpair);
                                if (spawnedNpc != null)
                                {
                                    //MelonLogger.Msg($"{characterId} spawned at {kvpair.Key} ({kvpair.Value}).");
                                    object coro = MelonCoroutines.Start(EventCoro(spawnedNpc, kvpair, playerMain.CurrentProperty));
                                    MelonCoroutines.Start(EndEventFor(spawnedNpc, coro));
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
                } catch (Exception ex)
                {
                    MelonLogger.Error(ex);
                }
                audioSource.Play();
                yield return new WaitForSeconds(0.77f);
                audioSource.pitch = UnityEngine.Random.Range(0.7f, 1f);
                audioSource.Play();
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.9f, 1.6f));
                audioSource.pitch = UnityEngine.Random.Range(0.5f, 0.8f);
                audioSource.volume = 0.8f;
                audioSource.Play();

                GameObject.Destroy(knocking, audioSource.clip.length);
            }
            try
            {
                npc.Avatar.Effects.SetSicklySkinColor();
                npc.Avatar.Effects.SetZombified(true);
                npc.Avatar.EmotionManager.AddEmotionOverride("Zombie", "deal_rejected", 30f, 0);
            } catch (Exception ex)
            {
                MelonLogger.Error(ex);
            }

            for (; ; )
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 20f));
                if (UnityEngine.Random.Range(0f, 1f) > 0.9f) { continue; }
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
                switch(roll)
                {
                    case 0:
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
                        Light light = propertyLights[UnityEngine.Random.Range(0, propertyLights.Length)];

                        Color orig = light.color;
                        float intens = light.intensity;
                        light.intensity = 0.1f;
                        yield return new WaitForSeconds(1f);
                        light.color = Color.red;
                        light.intensity = 1f;
                        yield return new WaitForSeconds(1f);
                        light.color = Color.white;
                        light.intensity = 1f;
                        yield return new WaitForSeconds(1f);
                        light.color = Color.red;
                        light.intensity = 1f;
                        yield return new WaitForSeconds(1f);
                        light.color = orig;
                        light.intensity = intens;
                        yield return new WaitForSeconds(1f);
                        continue;

                    case 4:
                        //MelonLogger.Msg("LightEvent 2");
                        Light[] propertyLights2 = property.transform.GetComponentsInChildren<Light>(true);
                        List<float> intensities = new();
                        foreach(Light l in propertyLights2)
                        {
                            yield return new WaitForSeconds(0.001f);
                            intensities.Add(l.intensity);
                            l.intensity = 0.1f;
                        }
                        yield return new WaitForSeconds(4f);
                        for(int i = 0; i < propertyLights2.Length; i++)
                        {
                            yield return new WaitForSeconds(0.001f);
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
                        try
                        {
                            npc.transform.rotation = Quaternion.LookRotation(playerMain.transform.position - npc.transform.position);
                        } catch (Exception ex)
                        {
                            MelonLogger.Error(ex);
                        }
                        continue;

                    case 7:
                        Vector3 backward = -playerMain.transform.forward.normalized;
                        int stepCount = UnityEngine.Random.Range(3, 7);
                        GameObject footsteps = new GameObject($"stepSounds");
                        AudioSource audioSource = footsteps.AddComponent<AudioSource>();
                        audioSource.clip = footSteps;
                        audioSource.spatialBlend = 1f;
                        audioSource.rolloffMode = AudioRolloffMode.Linear;
                        footsteps.transform.position = playerMain.transform.position + backward * 2;

                        for (int i = 0; i < stepCount; i++)
                        {
                            audioSource.pitch = UnityEngine.Random.Range(0.5f, 1f);
                            audioSource.volume = UnityEngine.Random.Range(0.5f, 0.8f);
                            audioSource.Play();
                            footsteps.transform.position += backward * 1.2f;

                            yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 0.6f));
                        }

                        GameObject.Destroy(footsteps);

                        continue;

                }
            }
        }

    }
}
