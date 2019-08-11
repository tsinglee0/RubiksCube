using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class SceneSwitcher : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    static void InitSceneSwitcher()
    {
        var switchers = FindObjectsOfType<SceneSwitcher>();

        if(null != switchers)
            for(int i = 0; i < switchers.Length; i++)
                UnityEngine.Object.Destroy(switchers[i]);
            
        GameObject go = new GameObject("[OnlySceneSwitcher]");
        UnityEngine.Object.DontDestroyOnLoad(go);

        go.AddComponent<SceneSwitcher>();
    }

    // Basic Switcher Params
    int curSceneIndex;
    int allScenesCount;
    AsyncOperation loadingAction;
    bool switchable => allScenesCount > 1 && null == loadingAction;

    // Mouse Press Event Params
    float MoveMouseThreshold = 50;
    Vector3 pressedMousePos;
    PressGUI pressDataGUI;

    void Start()
    {
        allScenesCount = SceneManager.sceneCountInBuildSettings;
        Debug.LogWarning("场景总数：" + allScenesCount);
        curSceneIndex = SceneManager.GetActiveScene().buildIndex;
        loadingAction = null;
        SceneManager.activeSceneChanged += OnSceneChanged;

        pressedMousePos = Vector3.right * float.MinValue;
        pressDataGUI = new PressGUI(1, 1, OnClearMousePressData);
    }

    private void OnSceneChanged(Scene oldScene, Scene curScene)
    {
        curSceneIndex = curScene.buildIndex;
        loadingAction = null;
    }

    void OnLoadingEnd(AsyncOperation action)
    {
        if(action.isDone)
            return;
        
        Debug.LogError("LoadScene Faild !");
        loadingAction = null;
    }

    void GoNextScene()
    {
        var targetSceneId = (curSceneIndex + 1) % allScenesCount;
        if(curSceneIndex == targetSceneId) return;
        loadingAction = SceneManager.LoadSceneAsync(targetSceneId, LoadSceneMode.Single);
        loadingAction.completed += OnLoadingEnd;
    }

    void GoPrevScene()
    {
        var targetSceneId = (allScenesCount + curSceneIndex - 1) % allScenesCount;
        if(curSceneIndex == targetSceneId) return;
        loadingAction = SceneManager.LoadSceneAsync(targetSceneId, LoadSceneMode.Single);
        loadingAction.completed += OnLoadingEnd;
    }

    void GoFirstScene()
    {
        var targetSceneId = 0;
        if(curSceneIndex == targetSceneId) return;
        loadingAction = SceneManager.LoadSceneAsync(targetSceneId, LoadSceneMode.Single);
        loadingAction.completed += OnLoadingEnd;
    }

    void GoLastScene()
    {
        var targetSceneId = allScenesCount - 1;
        if(curSceneIndex == targetSceneId) return;
        loadingAction = SceneManager.LoadSceneAsync(targetSceneId, LoadSceneMode.Single);
        loadingAction.completed += OnLoadingEnd;
    }

    void UpdateBasicKeySwitcher()
    {
        // Key Event
        if(Input.GetKeyDown(KeyCode.RightArrow))
            GoNextScene();
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
            GoPrevScene();
        else if(Input.GetKeyDown(KeyCode.UpArrow))
            GoFirstScene();
        else if(Input.GetKeyDown(KeyCode.DownArrow))
            GoLastScene();
    }

    void  Update()
    {
        if(!switchable)
            return;

        // Key Event
        UpdateBasicKeySwitcher();

        // Mouse Event
        MouseSwitcherUpdate();
    }

    void OnGUI()
    {
        pressDataGUI.OnGUI();
    }

    void MouseSwitcherUpdate()
    {
        if(Input.GetMouseButtonDown(0))
        {
            OnClearMousePressData();
            pressedMousePos = Input.mousePosition;
            // event type check
            pressDataGUI.HandleEvent(Input.mousePosition);            
        }
        else if(Input.GetMouseButtonUp(0))
        {
            OnClearMousePressData();
        }
        else if(pressedMousePos.x >= 0 && Input.mousePosition != pressedMousePos)
        {
            var distance = Vector3.Distance(pressedMousePos, Input.mousePosition);
            if(distance > MoveMouseThreshold)
            {
                OnClearMousePressData();
            }
        }
    }

    void OnClearMousePressData(int eventType = 0)
    {
        pressedMousePos.x = float.MinValue;
        pressDataGUI.RemoveEvent();

        if(eventType == 0)
            return;

        switch(eventType)
        {
            case 1: GoPrevScene(); break;
            case 2: GoNextScene(); break;
            case 3: GoFirstScene(); break;
            case 4: GoLastScene(); break;
        }
    }

    class PressGUI
    {
        int eventType;
        Action guiAction;
        Action<int> guiCompleted;

        float timeStart;
        float timeElapsed;
        float maxDuration;
        float curDuration => timeElapsed - timeStart; 

        bool shouldDrawGUI => null != guiAction && timeElapsed > timeStart;

        float leftSplitPos => Screen.width / 3;
        float rightSplitPos => Screen.width - leftSplitPos;
        float middleSplitPos => Screen.height / 2;

        public PressGUI(float startTime, float drawTime, Action<int> onGuiComplete)
        {
            timeStart = startTime;
            maxDuration = drawTime;
            guiCompleted = onGuiComplete;
        }

        public void RemoveEvent()
        {
            OnChangeEvent(0);
        }

        public void HandleEvent(Vector3 mousePosition)
        {
            int newEventType = eventType;

            if(mousePosition.y > middleSplitPos)
            {
                if(mousePosition.x > leftSplitPos && mousePosition.x < rightSplitPos)
                    newEventType = 3;
            }
            else if(mousePosition.x < leftSplitPos)
            {
                newEventType = 1;
            }
            else if(mousePosition.x < rightSplitPos)
            {
                newEventType = 4;
            }
            else
            {
                newEventType = 2;
            }

            OnChangeEvent(newEventType);
        }

        void OnChangeEvent(int newEventType)
        {
            if(eventType == newEventType)
                return;

            eventType = newEventType;
            timeElapsed = 0;

            switch(newEventType)
            {
                case 1: guiAction = OnGUIPrev; break;
                case 2: guiAction = OnGUINext; break;
                case 3: guiAction = OnGUIFirst; break;
                case 4: guiAction = OnGUILast; break;
                default: guiAction = null; break;
            }
        }

        public void OnGUI()
        {
            if(null != guiAction)
                timeElapsed += Time.deltaTime;

            if(!shouldDrawGUI)
                return;

            guiAction.Invoke();

            if(curDuration >= maxDuration)
            { 
                guiCompleted?.Invoke(eventType);
            }               
        }

        void OnGUIPrev()
        {
            var centerX = (int)leftSplitPos / 2;
            var centerY = Screen.height - (int)middleSplitPos/ 2;
            var radiusO = (int)Mathf.Min(centerX, (int)middleSplitPos / 2) / 2 - 6;
            var radiusI = radiusO * 2 / 3;
            var radiusA = radiusI - 6;

            var uvRect = new Rect(0, 0, 1, 1);

            // Center Triagle
            float slope = 1f / Mathf.Sqrt(3);
            int height = (int)(Mathf.Sqrt(3f) / 2f * radiusA);

            for(int offsetX = -radiusA; offsetX < radiusA / 2; offsetX+= 2)
            {
                for(int offsetY = height; offsetY > -height; offsetY-=3)
                {
                    var x = centerX + offsetX;
                    var y = centerY + offsetY;

                    if(offsetY > 0)
                    {
                        if(offsetY < slope * radiusA + offsetX * slope)
                            GUI.DrawTextureWithTexCoords(new Rect(x, y, 3, 3), Texture2D.whiteTexture, uvRect);
                    }
                    else
                    {
                        if(offsetY > -slope * radiusA - offsetX * slope)
                            GUI.DrawTextureWithTexCoords(new Rect(x, y, 3, 3), Texture2D.whiteTexture, uvRect);
                    }
                }
            }

            // Draw Circle
            float endX = Mathf.Lerp(radiusO, -radiusO, curDuration / (maxDuration * 0.8f));
            float minDis = radiusI * radiusI;
            float maxDis = radiusO * radiusO;

            for(var xOffset = radiusO; xOffset > endX; xOffset--)
            {
                for(var yOffset = radiusO; yOffset > 0; yOffset--)
                {
                    var dis = xOffset * xOffset + yOffset * yOffset;

                    if(dis < minDis || dis > maxDis)
                        continue;   

                    var x = centerX + xOffset;

                    GUI.DrawTextureWithTexCoords(new Rect(x, centerY + yOffset, 3, 3), Texture2D.whiteTexture, uvRect);
                    GUI.DrawTextureWithTexCoords(new Rect(x, centerY - yOffset, 3, 3), Texture2D.whiteTexture, uvRect);
                }
            }
        }

        void OnGUINext()
        {
            var centerX = Screen.width - (Screen.width - (int)rightSplitPos) / 2;
            var centerY = Screen.height - (int)middleSplitPos/ 2;
            var radiusO = (int)Mathf.Min(centerX, (int)middleSplitPos / 2) / 2 - 6;
            var radiusI = radiusO * 2 / 3;
            var radiusA = radiusI - 6;

            var uvRect = new Rect(0, 0, 1, 1);

            // Center Triagle
            float slope = 1f / Mathf.Sqrt(3);
            int height = (int)(Mathf.Sqrt(3f) / 2f * radiusA);

            for(int offsetX = -radiusA / 2; offsetX < radiusA; offsetX+= 2)
            {
                for(int offsetY = height; offsetY > -height; offsetY-=3)
                {
                    var x = centerX + offsetX;
                    var y = centerY + offsetY;

                    if(offsetY > 0)
                    {
                        if(offsetY < slope * radiusA - offsetX * slope)
                            GUI.DrawTextureWithTexCoords(new Rect(x, y, 3, 3), Texture2D.whiteTexture, uvRect);
                    }
                    else
                    {
                        if(offsetY > -slope * radiusA + offsetX * slope)
                            GUI.DrawTextureWithTexCoords(new Rect(x, y, 3, 3), Texture2D.whiteTexture, uvRect);
                    }
                }
            }

            // Draw Circle
            float endX = Mathf.Lerp(-radiusO, radiusO, curDuration / (maxDuration * 0.8f));
            float minDis = radiusI * radiusI;
            float maxDis = radiusO * radiusO;

            for(var xOffset = -radiusO; xOffset < endX; xOffset++)
            {
                for(var yOffset = radiusO; yOffset > 0; yOffset--)
                {
                    var dis = xOffset * xOffset + yOffset * yOffset;

                    if(dis < minDis || dis > maxDis)
                        continue;   

                    var x = centerX + xOffset;

                    GUI.DrawTextureWithTexCoords(new Rect(x, centerY + yOffset, 3, 3), Texture2D.whiteTexture, uvRect);
                    GUI.DrawTextureWithTexCoords(new Rect(x, centerY - yOffset, 3, 3), Texture2D.whiteTexture, uvRect);
                }
            }
        }

        void OnGUIFirst()
        {
            var centerX = Screen.width / 2;
            var centerY = (int)middleSplitPos/ 2;
            var radiusO = (int)Mathf.Min(centerX, (int)middleSplitPos / 2) / 2 - 6;
            var radiusI = radiusO * 2 / 3;
            var radiusA = radiusI - 6;

            var uvRect = new Rect(0, 0, 1, 1);

            // Center Triagle
            float slope = 1f / Mathf.Sqrt(3);
            int height = (int)(Mathf.Sqrt(3f) / 2f * radiusA);

            for(int offsetX = -radiusA; offsetX < radiusA / 2; offsetX+= 2)
            {
                if(offsetX > 0 && offsetX < radiusA / 4f)
                    continue;

                for(int offsetY = height; offsetY > -height; offsetY-=3)
                {
                    var x = centerX + offsetX;
                    var y = centerY + offsetY;

                    if(offsetY > 0)
                    {
                        if(offsetY < slope * radiusA + offsetX * slope)
                            GUI.DrawTextureWithTexCoords(new Rect(x, y, 3, 3), Texture2D.whiteTexture, uvRect);
                    }
                    else
                    {
                        if(offsetY > -slope * radiusA - offsetX * slope)
                            GUI.DrawTextureWithTexCoords(new Rect(x, y, 3, 3), Texture2D.whiteTexture, uvRect);
                    }
                }
            }

            // Draw Circle
            float endX = Mathf.Lerp(radiusO, -radiusO, curDuration / (maxDuration * 0.8f));
            float minDis = radiusI * radiusI;
            float maxDis = radiusO * radiusO;

            for(var xOffset = radiusO; xOffset > endX; xOffset--)
            {
                for(var yOffset = radiusO; yOffset > 0; yOffset--)
                {
                    var dis = xOffset * xOffset + yOffset * yOffset;

                    if(dis < minDis || dis > maxDis)
                        continue;   

                    var x = centerX + xOffset;

                    GUI.DrawTextureWithTexCoords(new Rect(x, centerY + yOffset, 3, 3), Texture2D.whiteTexture, uvRect);
                    GUI.DrawTextureWithTexCoords(new Rect(x, centerY - yOffset, 3, 3), Texture2D.whiteTexture, uvRect);
                }
            }
        }

        void OnGUILast()
        {
            var centerX = Screen.width / 2;
            var centerY = Screen.height - (int)middleSplitPos/ 2;
            var radiusO = (int)Mathf.Min(centerX, (int)middleSplitPos / 2) / 2 - 6;
            var radiusI = radiusO * 2 / 3;
            var radiusA = radiusI - 6;

            var uvRect = new Rect(0, 0, 1, 1);

            // Center Triagle
            float slope = 1f / Mathf.Sqrt(3);
            int height = (int)(Mathf.Sqrt(3f) / 2f * radiusA);

            for(int offsetX = -radiusA / 2; offsetX < radiusA; offsetX+= 2)
            {
                if(offsetX < 0 && offsetX > -radiusA / 4f)
                    continue;

                for(int offsetY = height; offsetY > -height; offsetY-=3)
                {
                    var x = centerX + offsetX;
                    var y = centerY + offsetY;

                    if(offsetY > 0)
                    {
                        if(offsetY < slope * radiusA - offsetX * slope)
                            GUI.DrawTextureWithTexCoords(new Rect(x, y, 3, 3), Texture2D.whiteTexture, uvRect);
                    }
                    else
                    {
                        if(offsetY > -slope * radiusA + offsetX * slope)
                            GUI.DrawTextureWithTexCoords(new Rect(x, y, 3, 3), Texture2D.whiteTexture, uvRect);
                    }
                }
            }

            // Draw Circle
            float endX = Mathf.Lerp(-radiusO, radiusO, curDuration / (maxDuration * 0.8f));
            float minDis = radiusI * radiusI;
            float maxDis = radiusO * radiusO;

            for(var xOffset = -radiusO; xOffset < endX; xOffset++)
            {
                for(var yOffset = radiusO; yOffset > 0; yOffset--)
                {
                    var dis = xOffset * xOffset + yOffset * yOffset;

                    if(dis < minDis || dis > maxDis)
                        continue;   

                    var x = centerX + xOffset;

                    GUI.DrawTextureWithTexCoords(new Rect(x, centerY + yOffset, 3, 3), Texture2D.whiteTexture, uvRect);
                    GUI.DrawTextureWithTexCoords(new Rect(x, centerY - yOffset, 3, 3), Texture2D.whiteTexture, uvRect);
                }
            }
        }
    }
}
