using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class ModMenuManager : MonoBehaviour
{
    public static ModMenuManager instance;

    private void Awake()
    {
        if (ModMenuManager.instance != null)
        {
            Object.Destroy(base.gameObject);
            return;
        }
        ModMenuManager.instance = this;
        Object.DontDestroyOnLoad(base.gameObject);
        QualitySettings.vSyncCount = 0;
        this.RefreshScenes();
        this.toastTimer = 8f;
    }

    private void OnGUI()
    {
        float nativeWidth = 1920f;
        float nativeHeight = 1080f;
        float screenWidth = (float)Screen.width;
        float screenHeight = (float)Screen.height;
        float horizRatio = screenWidth / nativeWidth;
        float vertRatio = screenHeight / nativeHeight;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(horizRatio, vertRatio, 1f));

        GUI.skin.button.fontSize = 28;
        GUI.skin.label.fontSize = 28;
        GUI.skin.toggle.fontSize = 28;
        GUI.skin.textField.fontSize = 28;
        GUI.skin.box.fontSize = 32;
        if (this.flyMode || this.noclip || this.mitaPOVActive)
        {
            GUI.backgroundColor = new Color(1, 1, 1, 0.5f);
            if (GUI.RepeatButton(new Rect(nativeWidth - 200f, nativeHeight - 400f, 150f, 150f), "UP")) { mobileFlyY = 1f; }
            else if (GUI.RepeatButton(new Rect(nativeWidth - 200f, nativeHeight - 220f, 150f, 150f), "DOWN")) { mobileFlyY = -1f; }
            else { mobileFlyY = 0f; }
            GUI.backgroundColor = Color.white;
        }

        string btnText = this.showMenu ? "Close Menu" : "Open Mod Menu";
        if (GUI.Button(new Rect(20f, 20f, 300f, 80f), btnText)) { this.ToggleMenuState(); }

        if (this.isEasterEggRunning)
        {
            GUI.color = new Color(0.6f, 0f, 0f, Random.Range(0.2f, 0.5f));
            GUI.DrawTexture(new Rect(0f, 0f, nativeWidth, nativeHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;
            return;
        }

        if (this.toastTimer > 0f)
        {
            this.toastTimer -= Time.deltaTime;
            GUIStyle toastStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 24 };
            toastStyle.normal.textColor = Color.green;
            GUI.Box(new Rect(nativeWidth / 2f - 250f, 20f, 500f, 60f), "");
            GUI.Label(new Rect(nativeWidth / 2f - 250f, 20f, 500f, 60f), "MOD LOADED! (Tap Open to Start)", toastStyle);
        }
        
        if (!this.showMenu) return;
        
        GUI.Box(new Rect(20f, 120f, 500f, 900f), "MiSide Mod Menu - Mobile");
        float currentY = 170f;
        currentY = this.DrawGroup(currentY, "Movement & Physics", ref this.showCheatsCategory, new Func<float, float>(this.DrawMovement));
        currentY = this.DrawGroup(currentY, "Mita Control", ref this.showMitaCategory, new Func<float, float>(this.DrawMitaControl));
        currentY = this.DrawGroup(currentY, "Visuals & Tools", ref this.showVisualsCategory, new Func<float, float>(this.DrawVisuals));
        currentY = this.DrawGroup(currentY, "Spawner", ref this.showSpawnerCategory, new Func<float, float>(this.DrawSpawner));
        currentY = this.DrawGroup(currentY, "Scene Loader", ref this.showSceneCategory, new Func<float, float>(this.DrawSceneLoader));
        currentY = this.DrawGroup(currentY, "Performance", ref this.showInfoCategory, new Func<float, float>(this.DrawPerformance));
        GUI.Label(new Rect(40f, 1030f, 400f, 40f), "Mod Created by: c___s");
    }

    private float DrawGroup(float y, string title, ref bool toggle, Func<float, float> drawFunc)
    {
        // Larger buttons for easier tapping
        if (GUI.Button(new Rect(40f, y, 460f, 55f), toggle ? ("[-] " + title) : ("[+] " + title)))
        {
            toggle = !toggle;
        }
        y += 65f;
        if (toggle)
        {
            y = drawFunc(y);
        }
        return y;
    }

    private float DrawMovement(float y)
    {
        this.speedHack = GUI.Toggle(new Rect(40f, y, 200f, 40f), this.speedHack, " Speed Hack");
        if (this.speedHack)
        {
            this.speedMultiplier = GUI.HorizontalSlider(new Rect(250f, y + 10f, 200f, 40f), this.speedMultiplier, 1f, 10f);
        }
        y += 50f;
        this.flyMode = GUI.Toggle(new Rect(40f, y, 200f, 40f), this.flyMode, " Fly Mode");
        this.noclip = GUI.Toggle(new Rect(250f, y, 200f, 40f), this.noclip, " Noclip");
        y += 50f;
        GUI.Label(new Rect(40f, y, 200f, 40f), string.Format("Time: {0:F1}x", this.timeScale));
        this.timeScale = GUI.HorizontalSlider(new Rect(250f, y + 10f, 200f, 40f), this.timeScale, 0f, 3f);
        return y + 60f;
    }

    private float DrawVisuals(float y)
    {
        this.deleteMode = GUI.Toggle(new Rect(40f, y, 250f, 40f), this.deleteMode, " Delete on Touch");
        y += 50f;
        if (this.gameCameraScript && this.gameCameraScript.playerCam)
        {
            GUI.Label(new Rect(40f, y, 150f, 40f), string.Format("FOV: {0}", (int)this.gameCameraScript.playerCam.fieldOfView));
            float num = GUI.HorizontalSlider(new Rect(200f, y + 10f, 250f, 40f), this.gameCameraScript.playerCam.fieldOfView, 30f, 130f);
            this.gameCameraScript.playerCam.fieldOfView = num;
            this.gameCameraScript.currentFOV = num;
        }
        return y + 60f;
    }

    private float DrawSpawner(float y)
    {
        if (GUI.Button(new Rect(40f, y, 220f, 50f), "Clone Player")) this.SpawnClone(true);
        if (GUI.Button(new Rect(280f, y, 220f, 50f), "Clone Mita")) this.SpawnClone(false);
        y += 60f;

        if (GUI.Button(new Rect(40f, y, 460f, 50f), this.showObjectList ? "Hide List" : "List Scene Objects"))
        {
            this.showObjectList = !this.showObjectList;
            if (this.showObjectList) this.RefreshObjectList();
        }
        y += 60f;

        if (this.showObjectList)
        {
            this.objectSearch = GUI.TextField(new Rect(40f, y, 460f, 50f), this.objectSearch);
            y += 60f;
            this.FilterObjectList();

            float rowHeight = 50f;
            float viewHeight = 250f;
            Rect scrollRect = new Rect(40f, y, 460f, viewHeight);
            Rect contentRect = new Rect(0f, 0f, 430f, this.filteredObjects.Count * rowHeight);

            this.objectListScroll = GUI.BeginScrollView(scrollRect, this.objectListScroll, contentRect);
            for (int i = 0; i < this.filteredObjects.Count; i++)
            {
                if (this.filteredObjects[i] != null && GUI.Button(new Rect(0f, i * rowHeight, 430f, 45f), this.filteredObjects[i].name))
                {
                    this.CloneObject(this.filteredObjects[i]);
                }
            }
            GUI.EndScrollView();
            y += viewHeight + 10f;
        }
        return y;
    }

    private float DrawSceneLoader(float y)
    {
        if (this.sceneNames == null) return y;
        
        float rowHeight = 55f;
        float viewHeight = 300f;
        Rect scrollRect = new Rect(40f, y, 460f, viewHeight);
        Rect contentRect = new Rect(0f, 0f, 430f, this.sceneNames.Length * rowHeight);

        this.scrollPosition = GUI.BeginScrollView(scrollRect, this.scrollPosition, contentRect);
        for (int i = 0; i < this.sceneNames.Length; i++)
        {
            if (GUI.Button(new Rect(0f, i * rowHeight, 430f, 50f), this.sceneNames[i]))
            {
                SceneManager.LoadScene(i);
            }
        }
        GUI.EndScrollView();
        return y + viewHeight + 10f;
    }

    private float DrawMitaControl(float y)
    {
        if (!this.mitaManager)
        {
            GUI.Label(new Rect(40f, y, 300f, 40f), "Mita not found.");
            if (GUI.Button(new Rect(350f, y, 100f, 40f), "Find")) this.FindReferences();
            return y + 50f;
        }
        this.mitaPOVActive = GUI.Toggle(new Rect(40f, y, 400f, 40f), this.mitaPOVActive, " Possess Mita");
        y += 50f;
        if (GUI.Button(new Rect(40f, y, 460f, 50f), "Teleport Player to Mita"))
        {
            this.ApplyHighPriorityBlendshape("SmileWide", 100f);
            Vector3 targetPos = this.mitaManager.transform.position + this.mitaManager.transform.forward * 1.5f;
            this.player.transform.position = new Vector3(targetPos.x, this.mitaManager.transform.position.y, targetPos.z);
            this.player.transform.LookAt(this.mitaManager.transform.position);
        }
        y += 60f;
        if (GUI.Button(new Rect(40f, y, 140f, 50f), "Idle")) this.SetMitaState(MovementState.Idle);
        if (GUI.Button(new Rect(200f, y, 140f, 50f), "Follow")) this.SetMitaState(MovementState.Following);
        if (GUI.Button(new Rect(360f, y, 140f, 50f), "Chase")) this.SetMitaState(MovementState.Chasing);
        return y + 60f;
    }

    private float DrawPerformance(float y)
    {
        GUI.Label(new Rect(40f, y, 460f, 40f), string.Format("FPS: {0:F1} | RAM: {1} MB", this.fps, GC.GetTotalMemory(false) / 1048576L));
        y += 45f;

        Rect graphRect = new Rect(40f, y, 460f, 80f);
        GUI.Box(graphRect, "");
        float barWidth = graphRect.width / (float)this.frameHistory.Length;
        for (int k = 0; k < this.frameHistory.Length - 1; k++)
        {
            float val = this.frameHistory[(this.historyIndex + k) % this.frameHistory.Length];
            float h = Mathf.Clamp(val * 1.5f, 0f, graphRect.height);
            GUI.color = (val > 16.6f) ? Color.red : Color.green;
            GUI.DrawTexture(new Rect(graphRect.x + (float)k * barWidth, graphRect.yMax - h, barWidth, h), Texture2D.whiteTexture);
        }
        GUI.color = Color.white;
        return y + 100f;
    }

    private void Update()
    {
        this.OptimizeMapRuntime();
        this.frameTime = Time.unscaledDeltaTime * 1000f;
        this.fps = 1f / Time.unscaledDeltaTime;
        this.frameHistory[this.historyIndex] = this.frameTime;
        this.historyIndex = (this.historyIndex + 1) % this.frameHistory.Length;

        if (Input.GetKeyDown(KeyCode.F11) || Input.GetKeyDown((KeyCode)282)) this.ToggleMenuState();

        if (this.showMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return; 
        }

        Time.timeScale = this.timeScale;
        if (this.deleteMode && Input.GetMouseButtonDown(0)) this.DeleteObject();
        if (Input.GetMouseButtonDown(2)) this.ForceInteract();
        
        this.HandleMitaPOV();
        if (!this.mitaPOVActive) this.HandleCheats();
    }

    private void SpawnClone(bool isPlayer)
    {
        GameObject target = (isPlayer ? (this.player.person ?? this.player.gameObject) : this.mitaManager.gameObject);
        if (!target) return;
        Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 3f;
        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 2f, Vector3.down, out hit, 10f)) spawnPos = hit.point;
        
        GameObject clone = Object.Instantiate<GameObject>(target, spawnPos, target.transform.rotation);
        clone.name = target.name + "_Clone";
        clone.SetActive(true);
        if (isPlayer)
        {
            foreach (Camera c in clone.GetComponentsInChildren<Camera>()) Object.Destroy(c);
            foreach (PlayerManager p in clone.GetComponents<PlayerManager>()) Object.Destroy(p);
        }
        else
        {
            MitaLifeManager lm = clone.GetComponent<MitaLifeManager>() ?? clone.GetComponentInChildren<MitaLifeManager>();
            if (lm) Object.Destroy(lm);
            MitaMove mm = clone.GetComponent<MitaMove>();
            MitaFollow mf = clone.GetComponent<MitaFollow>();
            if (mm) { mm.enabled = true; mm.SetMovementState(MovementState.Following); }
            if (mf) { mf.enabled = true; mf.followTarget = (this.player ? this.player.followTarget : null); mf.ToggleMovement(true); }
        }
    }

    private void FindReferences()
    {
        if (this.player == null)
        {
            this.player = PlayerManager.instance;
            if (this.player != null)
            {
                this.move = this.player.move;
                this.rb = this.player.rb;
                this.originalWalkSpeed = ((this.move != null) ? this.move.walkSpeed : 2f);
            }
        }
        if (this.gameCameraScript == null) this.gameCameraScript = Object.FindObjectOfType<PlayerCamera>();
        if (this.mitaManager == null) this.mitaManager = Object.FindObjectOfType<MitaManager>();
    }

    private void DeleteObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f) && !hit.collider.GetComponent<PlayerManager>())
        {
            Object.Destroy(hit.collider.gameObject);
        }
    }

    private void HandleCheats()
    {
        if (!this.move) return;
        if (!this.flyMode && !this.noclip)
        {
            float targetSpeed = (this.speedHack ? (this.originalWalkSpeed * this.speedMultiplier) : this.originalWalkSpeed);
            if (Math.Abs(this.move.walkSpeed - targetSpeed) > 0.1f) this.move.walkSpeed = targetSpeed;
        }
        if ((this.flyMode || this.noclip) && this.rb)
        {
            this.move.enabled = false;
            this.rb.useGravity = false;
            this.rb.velocity = Vector3.zero;

            // Use Joystick for movement, Mobile buttons for Y axis
            float joyH = (move.joystick != null) ? move.joystick.Horizontal : 0;
            float joyV = (move.joystick != null) ? move.joystick.Vertical : 0;
            
            Vector3 dir = Camera.main.transform.forward * joyV + Camera.main.transform.right * joyH + Vector3.up * mobileFlyY;
            this.rb.MovePosition(this.rb.position + dir * this.flySpeed * Time.deltaTime);
        }
        else if (this.rb)
        {
            this.rb.useGravity = true;
            this.move.enabled = true;
        }
    }

    private void CloneObject(GameObject o)
    {
        if (!o) return;
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 3f;
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out hit, 10f)) pos = hit.point;
        GameObject clone = Object.Instantiate<GameObject>(o, pos, o.transform.rotation);
        clone.name = o.name + "_Clone";
        clone.SetActive(true);
        if (o.GetComponent<PlayerManager>())
        {
            foreach (Camera c in clone.GetComponentsInChildren<Camera>()) Object.Destroy(c);
            Object.Destroy(clone.GetComponent<PlayerManager>());
        }
    }

    private void RefreshScenes()
    {
        this.sceneNames = new string[SceneManager.sceneCountInBuildSettings];
        for (int i = 0; i < this.sceneNames.Length; i++)
        {
            this.sceneNames[i] = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
        }
    }

    private void SetMitaState(MovementState s)
    {
        if (this.mitaManager && this.mitaManager.move) this.mitaManager.move.SetMovementState(s);
    }

    private void LateUpdate()
    {
        if (this.isEasterEggRunning && this.mitaManager && this.gameCameraScript)
        {
            this.ApplyHighPriorityBlendshape("SmileWide", 100f);
            this.gameCameraScript.playerCam.transform.LookAt(this.mitaManager.transform.position + Vector3.up * 1.6f);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (this.mitaPOVActive && this.mitaManager && this.mitaCamera)
        {
            this.mitaCamera.transform.rotation = this.mitaManager.transform.rotation * Quaternion.Euler(this.mitaCamRotX, 0f, 0f);
        }
    }

    private void HandleMitaPOV()
    {
        if (this.mitaManager == null) return;
        if (!this.mitaPOVActive)
        {
            if (this.mitaCamera && this.mitaCamera.enabled)
            {
                this.mitaCamera.enabled = false;
                if (this.mitaManager.GetComponent<MitaTurn>()) this.mitaManager.GetComponent<MitaTurn>().enabled = true;
                if (this.gameCameraScript) { this.ApplyHighPriorityBlendshape("SmileWide", 0f); this.gameCameraScript.playerCam.enabled = true; }
            }
            return;
        }

        if (!this.mitaCamera) { this.mitaCamera = this.mitaManager.GetComponentInChildren<Camera>(); return; }

        if (!this.mitaCamera.enabled)
        {
            this.mitaCamera.enabled = true;
            if (this.gameCameraScript) this.gameCameraScript.playerCam.enabled = false;
        }

        if (this.move) this.move.enabled = false;

        // --- Mobile Mita Controls ---
        float joyH = (move.joystick != null) ? move.joystick.Horizontal : 0;
        float joyV = (move.joystick != null) ? move.joystick.Vertical : 0;
        
        // Use Joystick to move Mita's body
        Vector3 mitaDir = (this.mitaManager.transform.forward * joyV + this.mitaManager.transform.right * joyH).normalized;
        this.mitaManager.transform.position += mitaDir * 4f * Time.deltaTime;
        
        if (this.mitaManager.move && this.mitaManager.move.animator) 
            this.mitaManager.move.animator.SetFloat("Speed", mitaDir.magnitude);

        // Simple look rotation for mobile
        if (Input.touchCount > 0 && Input.GetTouch(0).position.x > Screen.width / 2)
        {
            Vector2 touchDelta = Input.GetTouch(0).deltaPosition;
            this.mitaManager.transform.Rotate(Vector3.up * touchDelta.x * 0.1f);
            this.mitaCamRotX = Mathf.Clamp(this.mitaCamRotX - touchDelta.y * 0.1f, -80f, 80f);
        }
    }

    private void ForceInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50f))
        {
            hit.collider.SendMessage("EInteract", 1, SendMessageOptions.DontRequireReceiver);
            hit.collider.SendMessage("Interact", 1, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void ApplyHighPriorityBlendshape(string shapeName, float weight)
    {
        if (!this.mitaManager) return;
        if (!this.mitaFaceMesh)
        {
            foreach (SkinnedMeshRenderer smr in this.mitaManager.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (smr.sharedMesh.blendShapeCount > 0) { this.mitaFaceMesh = smr; break; }
            }
        }
        if (this.mitaFaceMesh)
        {
            if (this.smileIndex == -2) this.smileIndex = this.mitaFaceMesh.sharedMesh.GetBlendShapeIndex(shapeName);
            if (this.smileIndex != -1) this.mitaFaceMesh.SetBlendShapeWeight(this.smileIndex, weight);
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += this.OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= this.OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.player = null; this.mitaManager = null; this.gameCameraScript = null; this.mitaCamera = null; this.mitaFaceMesh = null;
        this.mitaPOVActive = false;
        this.FindReferences();
        this.RefreshScenes();
    }

    private void RefreshObjectList()
    {
        this.sceneObjects = Object.FindObjectsOfType<GameObject>();
        this.FilterObjectList();
    }

    private void FilterObjectList()
    {
        this.filteredObjects.Clear();
        if (this.sceneObjects == null) return;
        string search = this.objectSearch.ToLower();
        bool empty = string.IsNullOrEmpty(this.objectSearch);
        foreach (GameObject go in this.sceneObjects)
        {
            if (go != null && (empty || go.name.ToLower().Contains(search))) this.filteredObjects.Add(go);
        }
    }

    private void OptimizeMapRuntime()
    {
        if (this.player == null) return;
        this._refreshTimer += Time.deltaTime;
        if (this._refreshTimer > 10f || this._cacheRends.Count == 0)
        {
            this._refreshTimer = 0f;
            this._cacheRends.Clear();
            this._cacheRends.AddRange(Object.FindObjectsOfType<Renderer>());
        }
        int num = Mathf.Min(100, this._cacheRends.Count);
        Vector3 pPos = this.player.transform.position;
        for (int i = 0; i < num; i++)
        {
            this._lastCheckIndex = (this._lastCheckIndex + 1) % this._cacheRends.Count;
            Renderer r = this._cacheRends[this._lastCheckIndex];
            if (r != null && !r.CompareTag("Player") && !r.name.Contains("Mita"))
            {
                r.enabled = (r.transform.position - pPos).sqrMagnitude < 2025f;
            }
        }
    }
    
    private void ToggleMenuState()
    {
        this.showMenu = !this.showMenu;
        if (this.showMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            this.FindReferences();
        }
    }

    private bool showMenu;
    private bool speedHack;
    private float speedMultiplier = 2f;
    private float originalWalkSpeed;
    private bool flyMode;
    private float flySpeed = 10f;
    private PlayerManager player;
    private kiriMoveBasic move;
    private Rigidbody rb;
    private bool noclip;
    private string[] sceneNames;
    private Vector2 scrollPosition;
    private PlayerCamera gameCameraScript;
    private bool showCheatsCategory;
    private bool showVisualsCategory;
    private bool showSceneCategory;
    private bool showSpawnerCategory;
    private bool showObjectList;
    private GameObject[] sceneObjects;
    private Vector2 objectListScroll;
    private string objectSearch = "";
    private bool deleteMode;
    private bool showMitaCategory;
    private MitaManager mitaManager;
    private bool mitaPOVActive;
    private float mitaCamRotX;
    private float timeScale = 1f;
    private Camera mitaCamera;
    private float toastTimer;
    private SkinnedMeshRenderer mitaFaceMesh;
    private int smileIndex = -2;
    private bool isEasterEggRunning;
    private float fps;
    private float[] frameHistory = new float[100];
    private bool showInfoCategory;
    private float frameTime;
    private int historyIndex;
    private List<GameObject> filteredObjects = new List<GameObject>();
    private float searchTimer;
    private List<Renderer> _cacheRends = new List<Renderer>();
    private int _lastCheckIndex;
    private float _refreshTimer;
    public float mobileFlyY;
}