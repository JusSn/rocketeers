using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowMo : MonoBehaviour {

    public static SlowMo instance;

    private static float            NORMAL_SPEED = 1.0f;
    private static float            ZOOM_SPEED = 0.15f;
    private static float            ZOOM_THRESHOLD = 4f;
    private static float            MIN_FOV; // assigned when the 'begin' method is called
    private static float            MAX_FOV; // assigned when the 'begin' method is called
    private Vector3                 original_cam_pos;
    private Camera                  _camera;

    void Awake()
    {
        instance = this;
    }

    void Start(){
        _camera = MainCamera.S.GetComponent<Camera> ();
    }

    public static void Begin (float slow_mo_speed, Vector3 target_core){
        // the Z value would cause the _camera to clip and not show up properly,
        // so we permanently set the z value to -10f
        target_core.z = -10f;

        instance.original_cam_pos = instance._camera.transform.position;
        MAX_FOV = instance._camera.orthographicSize;
        MIN_FOV = Mathf.Max (instance._camera.orthographicSize - 3f, 6f);

        instance.StopAllCoroutines();
        instance.StartCoroutine (
                instance.LerpSlowMo (slow_mo_speed, target_core, ZOOM_SPEED));
    }

    public static void End(){
        instance.StopAllCoroutines();
        instance.StartCoroutine (
                instance.LerpSlowMo (NORMAL_SPEED, instance.original_cam_pos, -ZOOM_SPEED));
    }

    private IEnumerator LerpSlowMo (float target_speed, Vector3 _camera_target, float zoom_amt){

        while (Time.timeScale != target_speed){
            Time.timeScale = Mathf.Lerp (Time.timeScale, target_speed, 0.2f);
            ZoomOrtho_camera(_camera_target, zoom_amt);
            yield return null;
        }
    }

    // Ortographic _camera zoom towards a point (in world coordinates). Negative amount zooms in, positive zooms out
    void ZoomOrtho_camera(Vector3 zoomTowards, float amount)
    {
        // Calculate how much we will have to move towards the zoomTowards position
        float multiplier = (1.0f / _camera.orthographicSize * Mathf.Abs(amount));


        Vector3 new_loc = (zoomTowards - _camera.transform.position) * multiplier;
        // Move _camera
        _camera.transform.position += new_loc;

        if (Vector3.Distance (_camera.transform.position, zoomTowards) <= ZOOM_THRESHOLD) {

            // Limit _camera zoom
            _camera.orthographicSize = Mathf.Clamp (_camera.orthographicSize - amount, MIN_FOV, MAX_FOV);
        }
    }

}