using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(BallThrow))]
public class BallController : MonoBehaviour, ILevelStartObserver, IWinObserver
{
    [SerializeField] BallSettings ballSettings;

    private Rigidbody _rb;
    private Vector3 _velocity;
    [SerializeField] float velocityMag = 3;
    [SerializeField] Transform basketsBallTarget;
    [SerializeField] Transform goTransform;

    [Header("Decision")]
    float decisionPixel = 125.0f;

    [Header("Control")]
    private float m_previousX;
    public float deltaX = 0;
    public float swipeSensivity;
    bool updating;

    [Header("Shoot")]
    private float m_previousY;
    public float deltaY = 0;
    public float swipeSensivityY;

    float touchTimeStart, touchTimeFinish, timeInterval;
    BallThrow ballThrow;
    [SerializeField] MeshCollider basketCollider;

    [Header("Shoot Adjustments")]


    [Header("Sounds")]
    AudioSource audioSource;
    [SerializeField] AudioClip bounceOnAsphalt_Sound;
    [SerializeField] AudioClip score_Sound;

    // Game
    public bool isPointDetector0Triggered;

    // Particle
    [SerializeField] ParticleSystem dustParticle;

    void Start()
    {
        _rb = this.GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        audioSource = GetComponent<AudioSource>();
        ballThrow = GetComponent<BallThrow>();

        GameManager.Instance.Add_LevelStartObserver(this);
        GameManager.Instance.Add_WinObserver(this);
    }

    public void LevelStart()
    {
        _rb.constraints = RigidbodyConstraints.None;
        _rb.AddTorque(new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)));

        StartCoroutine(MyUpdate());
    }

    #region Refleting
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 3) // Ground
        {
            _velocity = Vector3.down * velocityMag;
            // ReflectBall(_rb, other.contacts[0].normal);
            _rb.AddForce(-1 * _velocity, ForceMode.VelocityChange);

            DustParticlePlay(other);

            audioSource.PlayOneShot(bounceOnAsphalt_Sound, 0.3f);
        }
        else
        {
            _velocity = _rb.velocity.normalized * 3;
            ReflectBall(_rb, other.contacts[0].normal);
        }
    }
    private void ReflectBall(Rigidbody rb, Vector3 reflectVector)
    {
        _velocity = Vector3.Reflect(_velocity, reflectVector);
        _rb.AddForce(_velocity, ForceMode.VelocityChange);
    }

    void DustParticlePlay(Collision other)
    {
        dustParticle.transform.position = other.contacts[0].point;
        dustParticle.transform.eulerAngles = new Vector3(-90, 0, 0);
        dustParticle.Play();
    }
    #endregion

    #region Point
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6) // PointDetector0
        {
            isPointDetector0Triggered = true;
            StartCoroutine(PointProtection());
            // basketCollider.enabled = true;
            basketCollider.gameObject.transform.DOScale(new Vector3(1.3f, 1.1f, 1.3f), 0.1f).OnComplete(() =>
                            basketCollider.gameObject.transform.DOScale(new Vector3(1.0f, 1.0f, 1.0f), 0.1f));
        }
        if (other.gameObject.layer == 7) // PointDetector1
        {
            if (isPointDetector0Triggered)
            {
                GameManager.Instance.Notify_WinObservers();
            }
            audioSource.PlayOneShot(score_Sound, 0.4f);
        }
    }
    IEnumerator PointProtection()
    {
        yield return new WaitForSeconds(0.4f);
        isPointDetector0Triggered = false;
    }
    #endregion

    IEnumerator MyUpdate()
    {
        yield return null;
        updating = true;

        while (updating)
        {
            goTransform.LookAt(new Vector3(basketsBallTarget.position.x, transform.position.y, basketsBallTarget.position.z));
            Control();

            yield return null;
        }
    }




    #region Control
    private void Control()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_previousX = Input.mousePosition.x;
            m_previousY = Input.mousePosition.y;

            touchTimeStart = Time.time;
        }

        if (Input.GetMouseButton(0))
        {
            deltaX = (Input.mousePosition.x - m_previousX);
            if (deltaX < -30f)
            {
                _rb.AddForce(goTransform.right * -swipeSensivity * Time.deltaTime, ForceMode.Impulse);
            }
            if (deltaX > 30f)
            {
                _rb.AddForce(goTransform.right * swipeSensivity * Time.deltaTime, ForceMode.Impulse);
            }

            deltaY = (Input.mousePosition.y - m_previousY);
            if (deltaY < -30f)
            {
                _rb.AddForce(goTransform.forward * -swipeSensivity * Time.deltaTime, ForceMode.Impulse);
            }
            if (deltaY > 30f)
            {
                _rb.AddForce(goTransform.forward * swipeSensivity * Time.deltaTime, ForceMode.Impulse);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            deltaX = 0;
            deltaY = 0;

            touchTimeFinish = Time.time;
            timeInterval = touchTimeFinish - touchTimeStart;

            if (timeInterval < 0.2f)
            {
                Shoot();
            }
        }
    }
    #endregion

    #region Shoot
    private void Shoot()
    {
        Debug.Log("Shoot");

        updating = false;
        Vector3 distanceV = basketsBallTarget.position - transform.position;
        distanceV.y = 0;

        if (distanceV.magnitude < 9)
        {
            // basketCollider.enabled = false;

            ballThrow.Throw(0, basketsBallTarget.position, 1.5f);
            StartCoroutine(TakeControlBack(1.5f));
        }
        else
        {
            Vector3 target = transform.position + distanceV.normalized * 5f;
            ballThrow.Throw(0, target, 1.2f);
            StartCoroutine(TakeControlBack(1.2f));
        }
    }
    #endregion

    IEnumerator TakeControlBack(float time)
    {
        yield return new WaitForSeconds(time);

        StartCoroutine(MyUpdate());
    }

    public void WinScenario()
    {
        updating = false;

        StartCoroutine(DecreaseVelocityMagnitude());

        StartCoroutine(Stop());
    }

    IEnumerator Stop()
    {
        yield return new WaitForSeconds(0.05f);
        updating = false;
    }

    IEnumerator DecreaseVelocityMagnitude()
    {
        bool a = true;
        while (a)
        {
            velocityMag -= Time.deltaTime * 3;
            if (velocityMag <= 0)
            {
                velocityMag = 0;
                a = false;
            }

            yield return null;
        }
    }
}