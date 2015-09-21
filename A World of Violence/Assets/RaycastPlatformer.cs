﻿using UnityEngine;
using System.Collections;

public class RaycastPlatformer : MonoBehaviour
{
    CharacterActions Brain;

    public float RunningAcceleration;
    public float Footing;

    bool PreviousJump;
    public float JumpForce;

    public bool Grounded;
    

    public float Gravity = 20;
    float CurrentGravity;

    float JumpAscendDuration;
    public float MaxJumpAscendDuration;

    public Vector2 velocity;

    Vector2 temppos;

    public float JumpInterval;
    public float JumpIntervalTimer;

    public LayerMask GroundCollision;
    public LayerMask DodgePlatformsCollision;
    LayerMask CurrentLayer;

    float DeltaTime;

    public bool DodgingPlatforms;

    Vector2 ForceToAdd;
    // Use this for initialization
    void Start()
    {
        //Time.timeScale = 0.1f;
        CurrentGravity = Gravity;
        Brain = GetComponent<CharacterActions>();
    }

    void PreUpdate()
    {
        PreviousJump = Brain.jump;
        DeltaTime = Time.deltaTime;
        if (DeltaTime > 0.04f) DeltaTime = 0.04f;
    }

    void Update()
    {
        PreviousJump = Brain.jump;
        DeltaTime = Time.deltaTime;
        if (DeltaTime > 0.04f) DeltaTime = 0.04f;

        Brain.UpdateCommands();

        Movement();
        velocity += (ForceToAdd);
        ForceToAdd *= 0;

        velocity += -Vector2.up * CurrentGravity * DeltaTime;

        Friction();
        Positioning();

    }

    void Movement()
    {
        Walking();
        Jumping();
    }

    void Walking()
    {
        if (Brain.moveLeft)
        {
            //Body.AddForce(-RunningAcceleration * Vector2.right);
            velocity -= Vector2.right * DeltaTime * RunningAcceleration;
        }
        if (Brain.moveRight)
        {
            //Body.AddForce(RunningAcceleration * Vector2.right);
            velocity += Vector2.right * DeltaTime * RunningAcceleration;
        }
        if (Brain.moveDown)
        {
            DodgingPlatforms = true;
        }
        else DodgingPlatforms = false;
    }

    void Jumping()
    {
        if (Brain.jump && !PreviousJump)
        {
            JumpIntervalTimer = JumpInterval;
        }

        else if (!Brain.jump)
        {
            JumpIntervalTimer = 0;
        }

        if (JumpIntervalTimer > 0)
        {
            
            if (Grounded)
            {
                JumpIntervalTimer = 0;
                //Grounded = false;
                //Body.AddForce(JumpForce * Vector2.up);
                //Body.gravityScale = 0;
                velocity += Vector2.up * JumpForce;
                CurrentGravity = 0;
                JumpAscendDuration = MaxJumpAscendDuration;
            }

            JumpIntervalTimer -= DeltaTime;
        }

        if (JumpAscendDuration > 0)
        {
            JumpAscendDuration -= DeltaTime;
            if (JumpAscendDuration <= 0 || !Brain.jump)
            {
                //Body.gravityScale = PersonalGravityScale;
                CurrentGravity = Gravity;
                JumpAscendDuration = 0;
            }
        }
    }

    void Friction()
    {
        if (Grounded || Brain.moveLeft || Brain.moveRight)
        {
            float FrictionForceX = -Footing * velocity.x /** Body.mass * Body.velocity.x*/;
            if (FrictionForceX > RunningAcceleration) FrictionForceX = RunningAcceleration;
            //Body.AddForce(FrictionForceX * Vector2.right); //Friction
            velocity += FrictionForceX * DeltaTime * Vector2.right;


        }
        if (velocity.y < 0)
        {
            float FrictionForceY = -Footing * velocity.y;
            velocity += FrictionForceY * DeltaTime * Vector2.up / 3;
        }
    }

    public void Positioning()
    {
        if (DodgingPlatforms)
        {
            CurrentLayer = DodgePlatformsCollision;
        }
        else CurrentLayer = GroundCollision;

        if (velocity.x != 0 || velocity.y != 0)
        {
            bool offsetedup = false;
            bool offseteddown = false;
            bool offsetedx = false;
            Grounded = false;


            temppos = new Vector2(transform.position.x, transform.position.y + 0.5f);
            RaycastHit2D miduprayinfo = Physics2D.Raycast(temppos, velocity.normalized, (velocity * DeltaTime).magnitude, DodgePlatformsCollision);

            temppos = new Vector2(transform.position.x, transform.position.y - 0.5f);
            RaycastHit2D middownrayinfo = Physics2D.Raycast(temppos, velocity.normalized, (velocity * DeltaTime).magnitude, CurrentLayer);

            temppos = new Vector2(transform.position.x - 0.5f, transform.position.y);
            RaycastHit2D midleftrayinfo = Physics2D.Raycast(temppos, velocity.normalized, (velocity * DeltaTime).magnitude, DodgePlatformsCollision);

            temppos = new Vector2(transform.position.x + 0.5f, transform.position.y);
            RaycastHit2D midrightrayinfo = Physics2D.Raycast(temppos, velocity.normalized, (velocity * DeltaTime).magnitude, DodgePlatformsCollision);

            if(miduprayinfo.collider != null && velocity.y >0)
            {
                transform.position += new Vector3(0, miduprayinfo.distance * velocity.normalized.y, 0);
                offseteddown = true;
                JumpAscendDuration = 0;
                CurrentGravity = Gravity;
                velocity = new Vector2(velocity.x, 0);
            }

            if (middownrayinfo.collider != null && velocity.y < 0)
            {
                transform.position += new Vector3(0, middownrayinfo.distance * velocity.normalized.y, 0);
                //OFFSET UP
                offsetedup = true;
                Grounded = true;
                velocity = new Vector2(velocity.x, 0);
            }

            if (midrightrayinfo.collider != null && velocity.x > 0)
            {
                transform.position += new Vector3(midrightrayinfo.distance * velocity.normalized.x, 0, 0);
                velocity = new Vector2(0, velocity.y);
                offsetedx = true;
            }

            if (midleftrayinfo.collider != null && velocity.x < 0)
            {
                transform.position += new Vector3(midleftrayinfo.distance * velocity.normalized.x, 0, 0);
                velocity = new Vector2(0, velocity.y);
                offsetedx = true;
            }

            temppos = new Vector2(transform.position.x - 0.5f, transform.position.y + 0.5f);
            RaycastHit2D upleftrayinfo = Physics2D.Raycast(temppos, velocity.normalized, (velocity * DeltaTime).magnitude, DodgePlatformsCollision);

            temppos = new Vector2(transform.position.x + 0.5f, transform.position.y + 0.5f);
            RaycastHit2D uprightrayinfo = Physics2D.Raycast(temppos, velocity.normalized, (velocity * DeltaTime).magnitude, DodgePlatformsCollision);

            temppos = new Vector2(transform.position.x - 0.5f, transform.position.y - 0.5f);
            RaycastHit2D downleftrayinfo = Physics2D.Raycast(temppos, velocity.normalized, (velocity * DeltaTime).magnitude, CurrentLayer);

            temppos = new Vector2(transform.position.x + 0.5f, transform.position.y - 0.5f);
            RaycastHit2D downrightrayinfo = Physics2D.Raycast(temppos, velocity.normalized, (velocity * DeltaTime).magnitude, CurrentLayer);

            if (upleftrayinfo.collider != null && miduprayinfo.collider == null && midleftrayinfo.collider == null)
            {
                if (velocity.x < 0 || velocity.y > 0)
                {
                    float tempX;
                    float tempY;

                    tempX = upleftrayinfo.point.x - upleftrayinfo.collider.transform.position.x;
                    tempY = upleftrayinfo.point.y - upleftrayinfo.collider.transform.position.y;

                    if (Mathf.Abs(tempX) > Mathf.Abs(tempY))
                    {
                        if (tempX > 0 && velocity.x < 0 && !offsetedx)
                        {
                            transform.position += new Vector3(upleftrayinfo.distance * velocity.normalized.x, 0, 0);
                            //OFFSET RIGHT
                            velocity = new Vector2(0, velocity.y);
                            offsetedx = true;
                        }
                    }
                    else
                    {
                        if (tempY < 0 && !offseteddown)
                        {
                            
                            transform.position += new Vector3(0, upleftrayinfo.distance * velocity.normalized.y, 0);
                            //OFFSET DOWN
                            offseteddown = true;
                            JumpAscendDuration = 0;
                            CurrentGravity = Gravity;
                            velocity = new Vector2(velocity.x, 0);
                        }
                    }

                }
            }

            if (uprightrayinfo.collider != null && miduprayinfo.collider == null && midrightrayinfo.collider == null)
            {
                if (velocity.x > 0 || velocity.y > 0)
                {
                    float tempX;
                    float tempY;

                    tempX = uprightrayinfo.point.x - uprightrayinfo.collider.transform.position.x;
                    tempY = uprightrayinfo.point.y - uprightrayinfo.collider.transform.position.y;

                    if (Mathf.Abs(tempX) > Mathf.Abs(tempY))
                    {
                        if (tempX < 0 && !offsetedx && velocity.x > 0)
                        {
                            //OFFSET LEFT
                            transform.position += new Vector3(uprightrayinfo.distance * velocity.normalized.x, 0, 0);
                            velocity = new Vector2(0, velocity.y);
                            offsetedx = true;
                        }
                    }
                    else
                    {
                        if (tempY < 0 && !offseteddown)
                        {
                            transform.position += new Vector3(0, uprightrayinfo.distance * velocity.normalized.y, 0);
                            //OFFSET DOWN
                            offseteddown = true;
                            JumpAscendDuration = 0;
                            CurrentGravity = Gravity;
                            velocity = new Vector2(velocity.x, 0);
                        }
                    }
                }
            }

            if (downleftrayinfo.collider != null && middownrayinfo.collider == null && midleftrayinfo.collider == null)
            {
                if (velocity.x < 0 || velocity.y < 0)
                {
                    float tempX;
                    float tempY;

                    tempX = downleftrayinfo.point.x - downleftrayinfo.collider.transform.position.x;
                    tempY = downleftrayinfo.point.y - downleftrayinfo.collider.transform.position.y;

                    if (Mathf.Abs(tempX) > Mathf.Abs(tempY))
                    {
                        if (tempX > 0 && !offsetedx && velocity.x < 0)
                        {
                            //OFFSET RIGHT
                            if (downleftrayinfo.collider.gameObject.layer != 1 << 9)
                            {
                                transform.position += new Vector3(downleftrayinfo.distance * velocity.normalized.x, 0, 0);
                            }
                            velocity = new Vector2(0, velocity.y);
                            offsetedx = true;
                        }
                    }
                    else
                    {
                        if (tempY > 0 && velocity.y < 0 && !offsetedup)
                        {
                            transform.position += new Vector3(0, downleftrayinfo.distance * velocity.normalized.y, 0);
                            //OFFSET UP
                            offsetedup = true;
                            Grounded = true;
                            velocity = new Vector2(velocity.x, 0);

                        }
                    }
                }
            }

            if (downrightrayinfo.collider != null && middownrayinfo.collider == null && midrightrayinfo.collider == null)
            {
                if (velocity.x > 0 || velocity.y < 0)
                {
                    float tempX;
                    float tempY;

                    tempX = downrightrayinfo.point.x - downrightrayinfo.collider.transform.position.x;
                    tempY = downrightrayinfo.point.y - downrightrayinfo.collider.transform.position.y;

                    if (Mathf.Abs(tempX) > Mathf.Abs(tempY))
                    {
                        if (tempX < 0 && !offsetedx && velocity.x > 0)
                        {
                            //OFFSET LEFT
                            if (downrightrayinfo.collider.gameObject.layer != 1 << 9)
                            {
                                transform.position += new Vector3(downrightrayinfo.distance * velocity.normalized.x, 0, 0);
                            }
                            velocity = new Vector2(0, velocity.y);
                            offsetedx = true;

                        }
                    }
                    else
                    {
                        if (tempY > 0 && !offsetedup && velocity.y < 0)
                        {
                            transform.position += new Vector3(0, downrightrayinfo.distance * velocity.normalized.y, 0);
                            //OFFSET UP
                            Grounded = true;
                            offsetedup = true;
                            velocity = new Vector2(velocity.x, 0);

                        }
                    }
                }
            }


            //float offset;
            //if (offsetX < offsetY)
            //{
            //    offset = offsetX;
            //}
            //else offset = offsetY;

            //else
            {
                transform.position += new Vector3(velocity.x, velocity.y, 0) * DeltaTime;

            }

            if (!offsetedup)
            {
                Grounded = false;
            }
        }

    }

    void AddForce(Vector2 force)
    {
        ForceToAdd += force;
    }
}
