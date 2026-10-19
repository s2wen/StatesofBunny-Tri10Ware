using UnityEngine;

public class RecenterCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private float edgeMargin = 0;

    private Camera cam;
    private Vector3 velocity;
    private bool recentering;

    void Awake()
    {
        target = GameObject.FindWithTag("Player").transform;
        cam = GetComponent<Camera>();
    }


    void LateUpdate()
    {
        if (target == null) return;

        Vector3 view = cam.WorldToViewportPoint(target.position);

        bool outside = view.x < edgeMargin || view.x > 1f - edgeMargin ||
                       view.y < edgeMargin || view.y > 1f - edgeMargin;

        if (outside) recentering = true;

        if (recentering)
        {
            Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

            if (Vector2.Distance(transform.position, desired) < 0.5f)
                recentering = false;
        }
    }
}
