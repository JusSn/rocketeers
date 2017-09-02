using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using InControl;

public class MichiGamesCabinetManager : MonoBehaviour
{

    static MichiGamesCabinetManager _instance;
    static bool initialized = false;

    void Start()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            _instance = this;
        }
        Screen.SetResolution(1280, 768, true);
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.ActiveDevice.GetControl(InputControlType.Back).IsPressed)
            Application.Quit();
        _ProcessPlayerInactivity();
    }

    bool inactivity_detected = false;
    public static void ReportUserActivity()
    {
        _instance.most_recent_activity = Time.time;
    }

    float previous_changed_tick = 0;
    public bool _DetectIncontrolControllerActivity()
    {
        if (InputManager.ActiveDevice == null)
            return false;

        if (InputManager.ActiveDevice.LastChangeTick != previous_changed_tick)
        {
            previous_changed_tick = InputManager.ActiveDevice.LastChangeTick;
            return true;
        }
        if (InputManager.ActiveDevice.AnyButton.HasChanged)
            return true;
        if (InputManager.ActiveDevice.Direction.HasChanged)
            return true;
        if (InputManager.ActiveDevice.LeftStick.HasChanged)
            return true;
        if (InputManager.ActiveDevice.RightStick.HasChanged)
            return true;
        return false;
    }

    float most_recent_activity = 0;
    float seconds_until_inactivity_quit = 180;
    void _ProcessPlayerInactivity()
    {
        bool activity_detected = _DetectIncontrolControllerActivity();

        if (Input.anyKeyDown || activity_detected)
        {
            most_recent_activity = Time.time;
        }

        float time_since_activity = Time.time - most_recent_activity;
        float seconds_until_quit = (int)(seconds_until_inactivity_quit - time_since_activity);

        if (time_since_activity > seconds_until_inactivity_quit - 30)
        {
            inactivity_detected = true;
        }
        else
        {
            inactivity_detected = false;
        }

        if (time_since_activity > seconds_until_inactivity_quit) // (3 minutes) 
        {
            Application.Quit();
        }
    }
}
