using System;
using System.Collections.Generic;
using UnityEngine;
using KSP;
using KspNalCommon;

namespace PlanItinearyForRovers {



	public class PlanItinearyForRoversProperties : CommonPluginProperties {
		public bool canGetIsDebug() {
			return true;
		}

		public int getInitialWindowId() {
			return 3470924;
		}

		public string getPluginDirectory() {
			return "PlanItinearyForRovers";
		}

		public string getPluginLogName() {
			return "PlanItinearyForRovers";
		}

		public bool isDebug() {
			return true;
		}

		public GUISkin kspSkin() {
			return HighLogic.Skin;
		}
	}



	[KSPAddon(KSPAddon.Startup.Flight, true)]
	public class PlanItinearyForRoversMain : MonoBehaviour2 {

		public List<GameObject> gameObjects = new List<GameObject>();

		public void Start() {
			PluginCommons.init(new PlanItinearyForRoversProperties());

			GameEvents.onPlanetariumTargetChanged.Add(new EventData<MapObject>.OnEvent(onPlanetariumTargetChanged));

			PluginLogger.logDebug("Start " + DateTime.Now );
		}

		public void OnDestroy() {
			//GameEvents.onGUIApplicationLauncherReady.Remove(new EventVoid.OnEvent(SetupToolbar));
			//GameEvents.onGUIApplicationLauncherUnreadifying.Remove(new EventData<GameScenes>.OnEvent(TeardownToolbar));
			//GameEvents.onHideUI.Remove(OnHideUI);
			//GameEvents.onShowUI.Remove(OnShowUI);
			GameEvents.onPlanetariumTargetChanged.Remove(new EventData<MapObject>.OnEvent(onPlanetariumTargetChanged));

			foreach (GameObject go in gameObjects) {
				go.DestroyGameObject();
			}
			//UnloadToolbar();

			//Config.Save();
		}

		CelestialBody body;

		public void OnGUI() {
			if (Event.current.type == EventType.MouseDown) {
				PluginLogger.logDebug("OnGUI");

				PluginLogger.logDebug("removing old");
				removeGameObject("MouseRayLine");

				Camera camera = Camera.main;
				Ray mouseRay;
				Vector3 worldStart;
				Vector3 worldDirection;
				if (MapView.MapIsEnabled) {

					body = Planetarium.fetch.CurrentMainBody;
					camera = PlanetariumCamera.Camera;

					mouseRay = camera.ScreenPointToRay(Input.mousePosition);
					mouseRay.origin = mouseRay.origin;

					PluginLogger.logDebug(mouseRay);

					Vector3 toDrawStart = mouseRay.origin;
					Vector3 toDrawEnd = mouseRay.origin + mouseRay.direction * (float)body.Radius;

					PluginLogger.logDebug(toDrawStart + "->" + toDrawEnd);
					PluginLogger.logDebug(ScaledSpace.LocalToScaledSpace(body.position));


					GameObject lineObj = addGameObject(new GameObject("MouseRayLine"));
					lineObj.layer = 9;
					LineRenderer line = lineObj.AddComponent<LineRenderer>();
					line.useWorldSpace = true;
					line.transform.localPosition = Vector3.zero;
					line.transform.localEulerAngles = Vector3.zero;

					// Make it render a red to yellow triangle, 1 meter wide and 2 meters long
					line.material = new Material(Shader.Find("Particles/Additive"));
					line.SetColors(Color.green, Color.red);
					line.SetWidth((float)10 / 1000 * PlanetariumCamera.fetch.Distance, (float)10 / 1000 * PlanetariumCamera.fetch.Distance);

					line.SetVertexCount(2);
					//line.SetPosition(0, ScaledSpace.LocalToScaledSpace(body.position + new Vector3(1, 0, 0) * (float)body.Radius *5));
					//line.SetPosition(1, ScaledSpace.LocalToScaledSpace(body.position));

					//line.SetPosition(0, toDrawStart);
					line.SetPosition(0, toDrawStart + new Vector3(0, -1, 0));
					line.SetPosition(1, toDrawEnd);

					worldStart = ScaledSpace.ScaledToLocalSpace(mouseRay.origin);
					worldDirection = mouseRay.direction;


				} else {
					//Input.mousePosition = new Vector2(Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y)
					mouseRay = camera.ScreenPointToRay(Input.mousePosition);

					PluginLogger.logDebug(Input.mousePosition);
					PluginLogger.logDebug(camera.ScreenToWorldPoint(Event.current.mousePosition));
					PluginLogger.logDebug(mouseRay);

					PluginLogger.logDebug(camera.transform.position);



					//mouseRay.origin = ScaledSpace.ScaledToLocalSpace(mouseRay.origin);

					GameObject lineObj = addGameObject(new GameObject("MouseRayLine"));
					LineRenderer line = lineObj.AddComponent<LineRenderer>();
					line.useWorldSpace = true;
					line.transform.localPosition = Vector3.zero;
					line.transform.localEulerAngles = Vector3.zero;

					// Make it render a red to yellow triangle, 1 meter wide and 2 meters long
					line.material = new Material(Shader.Find("Particles/Additive"));
					line.SetColors(Color.red, Color.red);
					line.SetWidth(1, 1);

					line.SetVertexCount(2);
					line.SetPosition(0, mouseRay.origin);
					line.SetPosition(1, mouseRay.origin + mouseRay.direction * 1000);

					worldStart = mouseRay.origin;
					worldDirection = mouseRay.direction;
				}




				Vector3d intersectionPoint = new Vector3d();
				Vector3d bodyToMouse = worldStart - body.position;
				float radius = (float)body.pqsController.radiusMax;
				//if (body.pqsController.RayIntersection(worldStart, worldDirection, out intersectionPoint)) {
				if(PQS.LineSphereIntersection(bodyToMouse, mouseRay.direction, radius, out intersectionPoint)){
					PluginLogger.logDebug("intersect!");
				} else {
					PluginLogger.logDebug("No hit!");
				}

				//mouseRay.origin = ScaledSpace.ScaledToLocalSpace(mouseRay.origin);
			}
		}

		private void removeGameObject(string name) {
			List<GameObject> toRemove = new List<GameObject>();
			foreach (GameObject go in gameObjects) {
				if (go.name == name) {
					go.DestroyGameObject();
					toRemove.Add(go);
				}
			}
			foreach (GameObject go in toRemove) {
				gameObjects.Remove(go);
			}
		}

		private GameObject addGameObject(GameObject go) {
			gameObjects.Add(go);
			return go;
		}

		public void onPlanetariumTargetChanged(MapObject map) {
			PluginLogger.logDebug("onPlanetariumTargetChanged");

			//map.celestialBody.pqsController.RayIntersection(worldStart, worldDirection, intersectionDistance);
		}
	}
}

