using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.WSA.Input;

public class MenuScript : MonoBehaviour {

    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private Sprite handUp;
    [SerializeField]
    private Sprite handDown;

    private GameObject cursorOff;
    private GameObject cursorOn;

    private GestureRecognizer gestureRecognizer;

    private GameObject target;

    // Use this for initialization
    void Start () {
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.Hold);

        gestureRecognizer.Tapped += GestureRecognizer_Tapped;
        gestureRecognizer.HoldStarted += GestureRecognizer_StartHold;
        gestureRecognizer.HoldCompleted += GestureRecognizer_CompleteHold;
        gestureRecognizer.HoldCanceled += GestureRecogniser_CancelHold;

        // Start looking for gestures.
        gestureRecognizer.StartCapturingGestures();

        Instantiate(menu, Camera.main.transform.position + Camera.main.transform.forward * 200, Quaternion.identity);
        cursorOff = transform.GetComponentInChildren<Light>().gameObject;
        cursorOn = transform.GetComponentInChildren<Image>().gameObject;

        cursorOn.SetActive(false);
        cursorOff.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;
        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity);

        if(hit.collider == null)
        {
            target = null;
            cursorOff.SetActive(true);
            cursorOn.SetActive(false);
        }
        else
        {
            if(!cursorOn.activeSelf)
                cursorOn.GetComponent<Image>().sprite = handUp;
            target = hit.transform.gameObject;
            cursorOn.SetActive(true);
            cursorOff.SetActive(false);
        }
	}

    public void LoadScene(string name)
    {
        string sceneName = "";

        if (name == "New Game")
        {
            sceneName = "BoxerAI";
        }
        else if (name == "Training")
        {
            sceneName = "PunchingBag";
        }
        else if (name == "Calibration")
        {
            sceneName = "Calibration";
        }
        else if(name == "Input")
        {

        }

        SceneManager.LoadScene(sceneName);
    }

    private void GestureRecognizer_Tapped(TappedEventArgs args)
    {
        if(target)
        {
            cursorOn.GetComponent<Image>().sprite = handDown;
            LoadScene(target.name);
        }
    }

    private void GestureRecognizer_StartHold(HoldStartedEventArgs args)
    {
        if(target)
        {
            cursorOn.GetComponent<Image>().sprite = handDown;
        }
    }

    private void GestureRecognizer_CompleteHold(HoldCompletedEventArgs args)
    {
        if (target)
        {
            LoadScene(target.name);
        }
    }

    private void GestureRecogniser_CancelHold(HoldCanceledEventArgs args)
    {
        cursorOff.SetActive(true);
        cursorOn.SetActive(false);
    }

    void OnDestroy()
    {
        gestureRecognizer.StopCapturingGestures();
        gestureRecognizer.Tapped -= GestureRecognizer_Tapped;
        gestureRecognizer.HoldStarted -= GestureRecognizer_StartHold;
        gestureRecognizer.HoldCompleted -= GestureRecognizer_CompleteHold;
        gestureRecognizer.HoldCanceled -= GestureRecogniser_CancelHold;
    }
}
