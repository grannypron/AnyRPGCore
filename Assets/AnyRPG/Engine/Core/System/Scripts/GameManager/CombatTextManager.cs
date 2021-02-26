using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using CustomTools.ObjectPooling.Scripts.ObjectPool;

namespace AnyRPG {
    public class CombatTextManager : MonoBehaviour {

        #region Singleton
        private static CombatTextManager instance;

        public static CombatTextManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CombatTextManager>();
                }

                return instance;
            }
        }

        #endregion

        [SerializeField]
        private GameObject combatTextPrefab;

        [SerializeField]
        private Canvas combatTextCanvas;

        private List<CombatTextController> combatTextControllers = new List<CombatTextController>();

        private List<CombatTextController> inUseCombatTextControllers = new List<CombatTextController>();

        public Canvas MyCombatTextCanvas { get => combatTextCanvas; set => combatTextCanvas = value; }

        private void Awake() {
            //Debug.Log("NamePlateManager.Awake(): " + NamePlateManager.MyInstance.gameObject.name);
            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateCombatText();
        }

        public void LateUpdate() {
            if (SystemConfigurationManager.MyInstance.UseThirdPartyCameraControl == true
                && CameraManager.MyInstance.ThirdPartyCamera.activeInHierarchy == true
                && PlayerManager.MyInstance.PlayerUnitSpawned == true) {
                UpdateCombatText();
            }
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            List<CombatTextController> removeList = new List<CombatTextController>();
            removeList.AddRange(inUseCombatTextControllers);
            foreach (CombatTextController combatTextController in removeList) {
                returnControllerToPool(combatTextController);
            }
        }

        private void UpdateCombatText() {
            if (CameraManager.MyInstance?.MyActiveMainCamera == null) {
                return;
            }
            if (UIManager.MyInstance.CutSceneBarController.CurrentCutscene != null) {
                return;
            }
            foreach (CombatTextController combatTextController in inUseCombatTextControllers) {
                combatTextController.RunCombatTextUpdate();
            }
        }

        public CombatTextController GetCombatTextController() {
            CombatTextController controller = (CombatTextController)ObjectPooler.Instance.GetComponent<ObjectPooler>().SpawnFromPool(PooledObjectType.CombatText, new Vector3(0,0,0), new Quaternion(0,0,0,0)).GetComponent<CombatTextController>();
            // The pooled object is not a child of the canvas right now since we are instantiating it, so assign it
            controller.transform.SetParent(combatTextCanvas.gameObject.transform, true);// I have tried this with true, false, and just assigning controller.transform.parent = 
            // TODO: no logic here yet to use the ones that are in use if the pool runs out - make that an option in the pooler
            inUseCombatTextControllers.Add(controller);
            return controller;
        }

        public void returnControllerToPool(CombatTextController combatTextController) {
            StartCoroutine(ReturnAtEndOFFrame(combatTextController));
        }

        public IEnumerator ReturnAtEndOFFrame(CombatTextController combatTextController) {
            yield return new WaitForEndOfFrame();
            if (inUseCombatTextControllers.Contains(combatTextController)) {
                inUseCombatTextControllers.Remove(combatTextController);
                combatTextControllers.Add(combatTextController);
            }
            ObjectPooler.Instance.GetComponent<ObjectPooler>().Despawn(combatTextController.gameObject);
        }

        public void SpawnCombatText(Interactable target, int damage, CombatTextType combatType, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name + "; damage: " + damage + "; type: " + combatType);
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //GameObject _gameObject = Instantiate(combatTextPrefab, target.transform.position, Quaternion.identity, combatTextCanvas.transform);
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                //Debug.Log("About to Set MainTarget on combat text");
                combatTextController.InitializeCombatTextController(target,
                    null,
                    GetDisplayText(combatType, damage),
                    combatType,
                    combatMagnitude,
                    abilityEffectContext
                    );
            }
        }

        private string GetDisplayText(CombatTextType combatType, int damage) {
            if (combatType == CombatTextType.miss) {
                return "(Miss)";
            } else if (combatType == CombatTextType.immune) {
                return "(Immune)";
            } else {
                return damage.ToString();
            }
        }

        public void SpawnCombatText(Interactable target, StatusEffect statusEffect, bool gainEffect) {
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name);
            //GameObject _gameObject = Instantiate(combatTextPrefab, target.transform.position, Quaternion.identity, combatTextCanvas.transform);
            //Debug.Log("About to Set MainTarget on combat text");
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                combatTextController.InitializeCombatTextController(target,
                    statusEffect.Icon,
                    statusEffect.DisplayName,
                    (gainEffect == true ? CombatTextType.gainBuff : CombatTextType.loseBuff)
                    );
            }
        }

        public void CleanupEventSubscriptions() {
            SystemEventManager.StopListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            SystemEventManager.StopListening("OnLevelUnload", HandleAfterCameraUpdate);

        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }


    }

}