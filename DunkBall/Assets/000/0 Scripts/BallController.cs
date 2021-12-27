using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class BallController : MonoBehaviour, ILevelStartObserver, IWinObserver
{
    private UnityAction behaviour;
    private Rigidbody _rb;
    private Vector3 _velocity;
    [SerializeField] float velocityMag = 3;
    [SerializeField] Transform basketsBallTarget;
    [SerializeField] Transform goTransform;

    // Control
    private float m_previousX;
    public float deltaX = 0;
    public float swipeSensivity;
    bool updating;

    // Shoot
    float touchTimeStart, touchTimeFinish, timeInterval;

    private float m_previousY;
    public float deltaY = 0;
    public float swipeSensivityY;

    // Game
    public bool canWin;

    // Particle
    [SerializeField] ParticleSystem dustParticle;

    void Start()
    {
        _rb = this.GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeAll;
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
            dustParticle.transform.position = other.contacts[0].point;
            dustParticle.transform.eulerAngles = new Vector3(-90, 0, 0);
            dustParticle.Play();
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
    #endregion

    #region Point
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6) // PointDetector0
        {
            canWin = true;
            StartCoroutine(PointProtection());
        }
        if (other.gameObject.layer == 7) // PointDetector1
        {
            if (canWin)
                GameManager.Instance.Notify_WinObservers();
        }
    }
    IEnumerator PointProtection()
    {
        yield return new WaitForSeconds(0.4f);
        canWin = false;
    }
    #endregion

    IEnumerator MyUpdate()
    {
        yield return null;
        updating = true;
        _rb.constraints = RigidbodyConstraints.None;

        while (updating)
        {
            // goTransform.eulerAngles = Vector3.zero;
            goTransform.LookAt(new Vector3(basketsBallTarget.position.x, 0, basketsBallTarget.position.z));
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
        if (deltaY > 150)
        {
            behaviour -= ControlOrShoot;
            behaviour += Shoot;
        }
        else if (Mathf.Abs(deltaX) > 150)
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
                _rb.AddForce(goTransform.right * -swipeSensivity * Time.deltaTime, ForceMode.Impulse);
            }
            if (deltaX > 0)
            {
                _rb.AddForce(goTransform.right * swipeSensivity * Time.deltaTime, ForceMode.Impulse);
            }
            m_previousX = Input.mousePosition.x;
        }

        if (Input.GetMouseButtonUp(0))
        {
            deltaX = 0;

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
            _rb.velocity = Vector3.zero;
            deltaY = Input.mousePosition.y - m_previousY;
            // *
            touchTimeFinish = Time.time;
            timeInterval = touchTimeFinish - touchTimeStart;
            Vector3 horizontalDir = new Vector3(basketsBallTarget.position.x, 0, basketsBallTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
            float distance = horizontalDir.magnitude;
            Vector3 shootDir = (basketsBallTarget.position + new Vector3(0, 1.1f * distance, 0)) - transform.position;

            // float multiply = deltaY * swipeSensivityY / timeInterval;

            // if (multiply < 150)
            //     multiply = 150;
            // if (multiply > 500)
            //     multiply = 500;
            // Debug.Log(multiply);
            float shootPower = distance * deltaY / 30;
            if (shootPower < 400)
                shootPower = 400;
            if (shootPower > 610)
                shootPower = 610;
            Debug.Log(shootPower);

            _rb.AddForce(shootDir.normalized * shootPower);
            shootPower = 0;

            deltaY = 0;
            behaviour += ControlOrShoot;
            behaviour -= Shoot;
        }
    }
    #endregion

    public void EasyWin()
    {
        transform.DOJump(basketsBallTarget.position, 1.2f, 1, 1).SetEase(Ease.Linear);
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