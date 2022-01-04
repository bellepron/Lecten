using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

[RequireComponent(typeof(BallThrow))]
public class BallController : MonoBehaviour, ILevelStartObserver, IWinObserver
{
    private UnityAction behaviour;
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

    [Header("Shoot Adjustments")]


    [Header("Sounds")]
    AudioSource audioSource;
    [SerializeField] AudioClip bounceOnAsphalt_Sound;

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

        behaviour += ControlOrShoot;
    }

    public void LevelStart()
    {
        StartCoroutine(MyUpdate());
    }

    #region Refleting
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 3) // Ground
        {
            _velocity = Vector3.down * velocityMag;
            ReflectBall(_rb, other.contacts[0].normal);

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
        }
        if (other.gameObject.layer == 7) // PointDetector1
        {
            if (isPointDetector0Triggered)
                GameManager.Instance.Notify_WinObservers();
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
        _rb.constraints = RigidbodyConstraints.None;
        _rb.AddTorque(new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)));

        while (updating)
        {
            // goTransform.eulerAngles = Vector3.zero;
            goTransform.LookAt(new Vector3(basketsBallTarget.position.x, transform.position.y, basketsBallTarget.position.z));
            behaviour.Invoke();

            yield return null;
        }
    }

    #region Operation
    private void ControlOrShoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_previousX = Input.mousePosition.x;
            deltaX = 0;
            m_previousY = Input.mousePosition.y;
            deltaY = 0;
            touchTimeStart = Time.time;
        }
        if (Input.GetMouseButton(0))
        {
            Debug.Log("Choosing");

            deltaX = (Input.mousePosition.x - m_previousX);
            deltaY = (Input.mousePosition.y - m_previousY);
        }
        if (deltaY > decisionPixel)
        {
            behaviour -= ControlOrShoot;
            behaviour += Shoot;
        }
        else if (Mathf.Abs(deltaX) > decisionPixel)
        {
            behaviour -= ControlOrShoot;
            behaviour += Control;
        }
    }
    #endregion

    #region Control
    private void Control()
    {
        Debug.Log("Controlling");

        if (Input.GetMouseButton(0))
        {
            deltaX = (Input.mousePosition.x - m_previousX);
            if (deltaX < 0)
            {
                _rb.AddForce(goTransform.right * -swipeSensivity * Time.deltaTime, ForceMode.VelocityChange);
            }
            if (deltaX > 0)
            {
                _rb.AddForce(goTransform.right * swipeSensivity * Time.deltaTime, ForceMode.VelocityChange);
            }
            m_previousX = Input.mousePosition.x;

            deltaY = (Input.mousePosition.y - m_previousY);
            if (deltaY < 0)
            {
                _rb.AddForce(goTransform.forward * -swipeSensivity * Time.deltaTime, ForceMode.Acceleration);
            }
            if (deltaY > 0)
            {
                _rb.AddForce(goTransform.forward * swipeSensivity * Time.deltaTime, ForceMode.Acceleration);
            }
            m_previousY = Input.mousePosition.y;
        }

        if (Input.GetMouseButtonUp(0))
        {
            deltaX = 0;
            deltaY = 0;

            behaviour += ControlOrShoot;
            behaviour -= Control;
        }
    }
    #endregion

    #region Shoot
    private void Shoot()
    {
        Debug.Log("Shooting");

        if (Input.GetMouseButtonUp(0))
        {
            deltaY = Input.mousePosition.y - m_previousY;
            deltaY /= Screen.height * 2.5f;

            touchTimeFinish = Time.time;
            timeInterval = touchTimeFinish - touchTimeStart;

            ballThrow.Throw(CalculateShootPower(), basketsBallTarget, 1.5f);

            deltaY = 0;

            behaviour += ControlOrShoot;
            behaviour -= Shoot;
        }
    }
    #endregion

    float CalculateShootPower()
    {
        // float shootPower = deltaY / timeInterval;
        float shootPower = deltaY;
        shootPower = Mathf.Clamp(shootPower, decisionPixel / Screen.height, 1.2f);

        if (shootPower > (decisionPixel / Screen.height) && shootPower < 1.2f)
        {
            shootPower = 1;
        }

        Debug.Log(shootPower);
        return shootPower;
    }

    public void EasyWin()
    {
        transform.DOJump(basketsBallTarget.position, 1.5f, 1, 1.5f).SetEase(Ease.OutSine);
    }

    public void WinScenario()
    {
        updating = false;

        StartCoroutine(DecreaseVelocityMagnitude());
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