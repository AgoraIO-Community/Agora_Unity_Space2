using UnityEngine;

/// <summary>
///   Synchronize the transform properties by sending changed data
/// to the SyncTransform event action.  Note this component should be
/// attached by code if the prefab is meant by spawned for both local
/// and remote object.  Controller should consider if sync is needed.
/// </summary>
public class TransformSynchronizer : MonoBehaviour
{
    public System.Action<string, Transform> SyncTransform;
    public string UserID { get; set; }

    Vector3 _position, _scale, _angle;
    // Start is called before the first frame update
    void Start()
    {
        _position = transform.localPosition;
        _scale = transform.localScale;
        _angle = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (_position != transform.localPosition ||
            _scale != transform.localScale ||
            _angle != transform.eulerAngles)
        {
            _position = transform.localPosition;
            _scale = transform.localScale;
            _angle = transform.eulerAngles;
            SyncTransform?.Invoke(UserID, transform);
        }
    }
}
