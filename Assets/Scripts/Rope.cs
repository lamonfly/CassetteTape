using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [Header("Player")]
    public Transform character;
    private PlayerController playerController;
    private Rigidbody2D playerRB;

    [Header("Hook")]
    public float rewindSpeed = 1;
    public float throwSpeed = 0.5f;
    public float hookDistanceCheck = 1;
    public Color hookedColor;
    public Color offColor;

    // hook physic
    private Rigidbody2D rb;
    private SpringJoint2D sjoint;
    private DistanceJoint2D djoint;

    // states
    private bool onCharacter = true;
    private bool onHook = false;
    public bool canThrow = false;

    // Rope
    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();

    [Header("Line")]
    public float ropeSegLen = 0.25f;
    public int segmentLength = 35;
    public int minSegmentLength = 15;
    public float lineWidth = 0.1f;

    private void Start()
    {
        // Get initial components
        rb = GetComponent<Rigidbody2D>();
        sjoint = GetComponent<SpringJoint2D>();
        djoint = GetComponent<DistanceJoint2D>();
        lineRenderer = GetComponent<LineRenderer>();
        playerController = character.GetComponent<PlayerController>();
        playerRB = character.GetComponent<Rigidbody2D>();

        for (int i = 0; i < segmentLength; i++)
        {
            ropeSegments.Add(new RopeSegment(character.position));
        }
    }

    void Update()
    {
        // Hook is on character ready to be thrown
        if (onCharacter)
        {
            transform.position = (Vector2)character.position;
            if (Input.GetButtonDown("Fire1") && canThrow)
            {
                HookAction();
                ThrowHook();
            }
        }
        // Hook is not on character can be toggled
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                HookAction();
            }
            if (Input.GetButton("Fire2"))
            {
                Rewind();
            }
        }

        if (!canThrow && onCharacter && playerController.grounded)
            canThrow = true;

        DrawRope();
        BackToCharacter();
        PencilColor();
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hook(collision.collider);
    }

    private void HookAction()
    {
        // Invert if hook can grip
        onHook = !onHook;

        // Remove grip actions
        if (!onHook)
        {
            Unhook();
        }
        // Try and grip
        else
        {
            ContactPoint2D[] contacts = new ContactPoint2D[1];
            if (GetComponent<Collider2D>().GetContacts(contacts) > 0)
                Hook(contacts[0].collider);
        }
    }

    // Throws hook
    private void ThrowHook()
    {
        SoundManager.PlaySound(SoundManager.Sound.playerThrow, transform.position);

        // Hook can grip
        onHook = true;
        canThrow = false;

        // Hook can no longer be thrown
        onCharacter = false;

        // Send hook towards cursor
        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        rb.isKinematic = false;
        rb.velocity = playerRB.velocity;
        rb.AddForce((target - (Vector2) character.position) * throwSpeed , ForceMode2D.Impulse);
    }

    // Toggle hook on
    private void Hook(Collider2D collision)
    {
        // Check if it can grip
        if (onHook)
        {
            // Remove physics gravity
            rb.isKinematic = true;
            rb.freezeRotation = true;
            rb.velocity = Vector3.zero;

            // Set spring length
            sjoint.autoConfigureDistance = true;
            sjoint.enabled = true;
            sjoint.autoConfigureDistance = false;

            // Rotate pencil
            Vector2 target = transform.position - character.position;
            target = target.normalized;
            float rot_z = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
            rot_z += 60;
            transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);

            playerController.hooked = true;
            lineRenderer.material.color = hookedColor;

            transform.SetParent(collision.transform);

            SoundManager.PlaySound(SoundManager.Sound.playerHit, transform.position);
        }
    }

    // Toggle hook off
    private void Unhook()
    {
        // Turn on physics/gravity
        rb.isKinematic = false;
        rb.freezeRotation = false;

        // No more spring
        sjoint.enabled = false;

        playerController.hooked = false;
        lineRenderer.material.color = offColor;
        transform.SetParent(character.parent);
    }

    // Hook comes back to player
    private void Rewind()
    {
        // Currently not hooked
        // hook comes to player
        if (!onHook && !onCharacter)
        {
            rb.AddForce((character.position - transform.position).normalized * rewindSpeed * .5f, ForceMode2D.Impulse);
        }
        // Currently hook
        // player goes to hook
        else if (onHook)
        {
            if (sjoint.distance > (hookDistanceCheck / 2))
            {
                sjoint.distance -= Time.deltaTime * rewindSpeed;
            }
        }
    }

    // Check if hook is close enough to character to reset it
    private void BackToCharacter()
    {
        if (!onHook && ((Vector2)transform.position - (Vector2)character.position).magnitude < hookDistanceCheck)
        {
            rb.freezeRotation = true;
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            onCharacter = true;
            transform.rotation = Quaternion.AngleAxis(26, Vector3.forward);
        }
    }

    // Set color of pencil depending if hooked or not
    private void PencilColor()
    {
        var mat = GetComponentInChildren<SpriteRenderer>().material;

        if (!canThrow && onCharacter)
            mat.color = Color.red;
        else
            mat.color = Color.black;
    }

    // Simulate rope
    private void Simulate()
    {
        // Simulation
        Vector2 forceGravity = new Vector2(0f, -0.5f);

        for (int i = 1; i < segmentLength; i++)
        {
            RopeSegment firstSegment = ropeSegments[i];
            Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += velocity;
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
            ropeSegments[i] = firstSegment;
        }

        // Constraints
        int segDistance = (int)((character.position - transform.position).magnitude / ropeSegLen);
        segmentLength = Mathf.Max(segDistance, minSegmentLength);

        if (segmentLength >= ropeSegments.Count)
        {
            segmentLength = ropeSegments.Count - 1;
            if (onHook && rb.velocity.magnitude > 0.1f)
            {
                playerController.ThrowPlayer(rb.velocity.normalized);
                rb.velocity = Vector3.zero;
                HookAction();
            }
            djoint.enabled = true;
        }
        else
        {
            djoint.enabled = false;
        }

        for (int i = 0; i < 25; i++)
        {
            ApplyConstraint();
        }
    }

    // Apply rope physics/constraint to make move/bend
    private void ApplyConstraint()
    {
        //Constrant to First Point 
        RopeSegment firstSegment = ropeSegments[0];
        firstSegment.posNow = character.position;
        ropeSegments[0] = firstSegment;


        //Constrant to Second Point 
        RopeSegment endSegment = ropeSegments[segmentLength - 1];
        endSegment.posNow = transform.position;
        ropeSegments[segmentLength - 1] = endSegment;


        for (int i = 0; i < segmentLength - 1; i++)
        {
            RopeSegment firstSeg = ropeSegments[i];
            RopeSegment secondSeg = ropeSegments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - ropeSegLen);
            Vector2 changeDir = Vector2.zero;

            if (dist > ropeSegLen)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            }
            else if (dist < ropeSegLen)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                ropeSegments[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                ropeSegments[i + 1] = secondSeg;
            }
        }
    }

    // Draw rope using linerenderer
    private void DrawRope()
    {
        float lineWidth = this.lineWidth;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[segmentLength];
        for (int i = 0; i < segmentLength; i++)
        {
            ropePositions[i] = ropeSegments[i].posNow;
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            posNow = pos;
            posOld = pos;
        }
    }
}
