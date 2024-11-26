using System;
using System.Collections;
using UnityEngine;
using UnityEngine.ProBuilder;

public class SoundBullet : MonoBehaviour
{

    public float InitialElementScale = 0.50f;
    public float InitialElementsScaleExpansionRate = 1.2f;
    public int ElementCount = 3;
    public float ElementsDistanceSpacing = 1.2f;
    private GameObject[] elements;
    [Range(0f, 20f)]
    public float ScaleExpandingRate = 0.1f; //during travel
    public float TemporalExpandingRateInSeconds = 0.5f;  //during travel
    private TimingController timingController;
    public float TravelSpeed = 0.5f;
    public float LifespanInSeconds = 5f;
    public Ray Ray;
    public Vector3 Direction;
    public GameObject BaseBulletModel;
    public bool CanTravel = false;
    public bool Expands = true;
    public bool Dissipates = true;
    private Rigidbody _rigidbody;
    private Vector3 originalScale;

    //public bool Bounces?
    //public bool Richochet?

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        timingController = GetComponent<TimingController>();
        timingController.Target = TemporalExpandingRateInSeconds;
        timingController.OnTime += (object sender, EventArgs e) => Expand();
        StartCoroutine(TimingController.Time(TimeType.SCALEDTIME, LifespanInSeconds, () => StartCoroutine(Dissipate())));
    }

    private Vector3 AdjustPosition(Vector3 originalPoint, Vector3 scale, Vector3 direction)
    {
        if (direction.z >= 0 || direction.x >= 0) originalPoint.x -= scale.x;
        else originalPoint.z -= scale.z;

        if (direction.z >= 0 || direction.x < 0) originalPoint.y += scale.y;
        else originalPoint.y -= scale.y;

        return originalPoint;
    }

    public void InstantiateElements()
    {
        originalScale = new Vector3(0.3f, 0.1f, 0.3f) * InitialElementScale;
        Vector3 scale = originalScale;
        float positionX = 1.2f;
        Vector3 point;
        Direction = Ray.direction.normalized; //new Vector3(90f * Direction.z, 0, 90f * Direction.x)
        elements = new GameObject[ElementCount];
        for (int elem = 0; elem < ElementCount; elem++)
        {
            point = AdjustPosition(Ray.GetPoint(positionX), scale, Direction);
            GameObject segment = Instantiate(BaseBulletModel, point, Quaternion.Euler(new Vector3(90f * Direction.z, 0, 90f * Direction.x)), gameObject.transform);
            segment.transform.localScale = new Vector3(scale.x, originalScale.y, scale.z);
            scale = scale + (Vector3.one * InitialElementsScaleExpansionRate);
            positionX += ElementsDistanceSpacing;
            elements[elem] = segment;
        }
    }

    //gameObject, RigidBodies, VertexColor
    private void Iterate(Action<GameObject> callback)
    {
        foreach (GameObject obj in elements)
            callback(obj);
    }


    private void Expand()
    {
        if (Expands) Iterate((obj) => 
        {
            //Debug.Log("Expand");
            Vector3 oldScale = obj.transform.localScale;
            obj.transform.localScale = new Vector3(oldScale.x + (originalScale.x * ScaleExpandingRate * Time.deltaTime), originalScale.y, oldScale.z + (originalScale.z * ScaleExpandingRate * Time.deltaTime));
        });
    }

    public void StartTravelling()
    {
        CanTravel = true;
    }

    private void Travel()
    {
        if (CanTravel)
            _rigidbody.AddForce(TravelSpeed * Time.deltaTime * Direction);
    }

    /// <summary>
    /// - Destroy(this);
    /// - Fadeaway animation
    /// </summary>
    private IEnumerator Dissipate()
    {
        //fading
        Destroy(gameObject);
        yield return null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        Travel();
    }

}
