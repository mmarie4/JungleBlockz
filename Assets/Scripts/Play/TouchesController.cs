using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TouchesController : MonoBehaviour
{

    private GameObject ninjaObject;
    private Ninja ninjaScript;
    private BoxCollider2D leftButton;
    private bool leftPressed = false;
    private BoxCollider2D rightButton;
    private bool rightPressed = false;
    private BoxCollider2D jumpButton;
    private bool jumpPressed = false;
    private BoxCollider2D shootButton;
    private bool shootPressed = false;
    private float timeSinceLastShot = 0.0f;
    private float shootingDelay;
    private int move = 0;
    private PlayerStats playerStats;
    private BoxCollider2D menuButton;

    // attack button
    public Button atkBtn;

    private void Start()
    {
        playerStats = SaveHandler.Load();
        shootingDelay = 1 / GameSettings.GetAtkPerSecond(playerStats, playerStats.GetWeapon());
        ninjaObject = GameObject.Find("Ninja");
        ninjaScript = ninjaObject.GetComponent<Ninja>();
        leftButton = GameObject.Find("LeftButton").GetComponent<BoxCollider2D>();
        rightButton = GameObject.Find("RightButton").GetComponent<BoxCollider2D>();
        jumpButton = GameObject.Find("JumpButton").GetComponent<BoxCollider2D>();
        shootButton = GameObject.Find("ShootButton").GetComponent<BoxCollider2D>();

        // Set attack button img
        atkBtn.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/button_" + playerStats.GetWeapon());
    }
    void Update()
    {
        // Check which buttons are touched
        leftPressed = false;
        rightPressed = false;
        jumpPressed = false;
        shootPressed = false;
        for (int i = 0; i < Input.touchCount; ++i)
        {
            
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
            Vector2 touchPos = new Vector2(wp.x, wp.y);
            if (Get2DBounds(leftButton.bounds).Contains(touchPos)) leftPressed = true;
            if (Get2DBounds(rightButton.bounds).Contains(touchPos)) rightPressed = true;
            if (Get2DBounds(jumpButton.bounds).Contains(touchPos) && Input.GetTouch(i).phase == TouchPhase.Began) jumpPressed = true;
            if (Get2DBounds(shootButton.bounds).Contains(touchPos)) shootPressed = true;
        }

        // Set movement
        move = leftPressed && rightPressed ? 0 : !leftPressed && !rightPressed ? 0 : leftPressed ? -1 : 1;
        ninjaScript.SetMoveHorizontal(move);

        // Jump
        if (jumpPressed) ninjaScript.Jump();

        timeSinceLastShot += Time.deltaTime;
        // Shoot
        if (shootPressed && timeSinceLastShot > shootingDelay)
        {
            timeSinceLastShot = 0.0f;
            ninjaScript.Shoot();
        }

    }

    private static Bounds Get2DBounds(Bounds aBounds)
    {
        var ext = aBounds.extents;
        ext.z = float.PositiveInfinity;
        aBounds.extents = ext;
        return aBounds;
    }
}