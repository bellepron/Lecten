using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, IWinObserver
{
    [SerializeField] Transform basket;
    [SerializeField] Transform ball;
    [SerializeField] float distanceBetweenBall = 8;
    float pos_Y;
    float angle_X;
    bool updating = true;

    private void Start()
    {
        pos_Y = transform.position.y;
        angle_X = transform.eulerAngles.x;

        GameManager.Instance.Add_WinObserver(this);
    }

    void LateUpdate()
    {
        if (updating == false) return;

        Vector3 ball_XZ = ball.position;
        ball_XZ.y = 0;
        Vector3 basket_XZ = basket.position;
        basket_XZ.y = 0;
        Vector3 direction = (ball_XZ - basket_XZ).normalized;
        Vector3 targetPos = ball.position + direction * distanceBetweenBall;
        targetPos = new Vector3(targetPos.x, pos_Y, targetPos.z);
        transform.position = targetPos;
        // transform.position = Vector3.MoveTowards(transform.position, targetPos, 15 * Time.deltaTime);

        transform.LookAt(basket.position);
        transform.eulerAngles = new Vector3(angle_X, transform.eulerAngles.y, 0);
    }

    public void WinScenario()
    {
        updating = false;
    }
}