using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using UnityEngine.XR;
using static UnityEditor.ShaderData;

public class player : MonoBehaviour
{
    private bool bWasOnFloor = false;
    public Vector2 vectorVelocity;
    private Vector2 vector_gravity;
    public float last_horizontal_direction = 1; // seems to only be -1, 0, or 1
    private bool active = true;
    private string anim = "";
    private Vector2 vSpriteOffset = new Vector2(0,.5f); // changed from (0, 8) to use unity units instead of pixels
    private enum PlayerState
    { 
        State_normal, 
        State_dive 
    }
    private PlayerState pState = PlayerState.State_normal;

    public bool flag_constant_spritetrail = false;
    public UnityEngine.Color color_spritetrail;
    private bool flag_can_create_spritetrail = true;

    private const float lerp_constant = 0.5f;
    [SerializeField] private Vector2 vector_gravity_up = new Vector2(0, -10); // should be const
    [SerializeField] private Vector2 vector_gravity_down = new Vector2(0, -12.5f); // should be const

    [SerializeField] private float maximum_speed = 95;
    [SerializeField] private float jump_force = 175;

    private float airTime = 0;
    private const float maxAirTime = 5;
    private float jumpBuffer = 0;
    private const float maxJumpBuffer = 2.5f;
    private float diveBuffer = 0;
    private const float maxDiveBuffer = 0.51f; // orig .51
    private float target_rotation = 0;
    private const float twn_duration = 0.25f;
    public AnimationCurve tween_interpolation_curve;
    //const spritetrail= preload('res://Scenes/spritetrail/sprite_trail.tscn')
    //const spherizeShader= preload("res://Scenes/spherizeShader.tscn")
    [SerializeField] private GameObject drop;

    [SerializeField] private SpriteRenderer nSprite; // sprite type instead?
    [SerializeField] private GameObject eyes;
    [SerializeField] Animator nAnimationPlayer;
    [SerializeField] DustAnimationHandler jumpDustAnimator;
    [SerializeField] DustAnimationHandler landDustAnimator;
    [SerializeField] private GameObject dive_aim;
    [SerializeField] private Collider2D dive_aim_collider;
    [SerializeField] SpriteRenderer nVignette;

    [Header("Sound Effects")]
    [SerializeField] AudioSource sfxPlayer;
    [SerializeField] AudioClip jumpSfx;
    [SerializeField] AudioClip landSfx;
    [SerializeField] AudioClip diveSfx;
    [SerializeField] AudioClip dive1Sfx;
    [SerializeField] AudioClip diveawaySfx;
    [SerializeField] AudioClip[] footstepSfx;


    // Start is called before the first frame update
    void Start()
    {
        //nSprEyes.visible = false
        nVignette.color = new Color(0, 0, 0, 0); // set vignette to be transparent
        nAnimationPlayer.Play("player_idle", 0);

        // initialize dive aimer rotation
        Vector3 newRot = dive_aim.transform.eulerAngles;
        newRot.z = last_horizontal_direction == 1 ? 0 : -180;
        dive_aim.transform.eulerAngles = newRot;
    }

    // Update is called once per frame
    void Update()
    {
        nSprite.flipX = last_horizontal_direction != 1;
        nVignette.transform.position = Vector3.zero; // make sure the vignette stays centered as the player moves

        // determine which animation to play
        if (pState == PlayerState.State_dive)
        {
            anim = "player_idle";
        }
        else
        {
            if (is_on_floor())
            {
                anim = Mathf.Abs(vectorVelocity.x) <= .01 ? "player_idle" : "player_walk";
            }
            else
            {
                anim = vectorVelocity.y < 0 ? "player_fall" : "player_up1";
            }
        }


        // color dive aimer
        List<Collider2D> overlaps = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        dive_aim_collider.OverlapCollider(filter.NoFilter(), overlaps);
        SpriteRenderer dive_aim_sprite = dive_aim.transform.Find("Main Sprite").transform.gameObject.GetComponent<SpriteRenderer>();
        Color32 r = new Color32(192, 70, 31, 255); // red is "c0461f"
        Color32 gr = new Color32(175, 255, 0, 255); // green is "afff00")
        bool phaseable = true;
        foreach (Collider2D body in overlaps)
        {
            if (!body.gameObject.CompareTag("Phaseable"))
            {
                phaseable = false;
            }
        }
        if (phaseable) 
        { 

            if (pState == PlayerState.State_normal) 
            {
                dive_aim_sprite.color = overlaps.Count > 0 ? gr : r;
            }

            else 
            {
                dive_aim_sprite.color = overlaps.Count > 0 ? r : gr;
            }
        }
        else 
        {
            dive_aim_sprite.color = r;
        }



        // play the selected animation
        if (!nAnimationPlayer.GetCurrentAnimatorStateInfo(0).IsName(anim))
        {
            nAnimationPlayer.Play(anim, 0);
        }


        /*
         * Animation
        if flag_constant_spritetrail:
		    _create_spritetrail()
        */


        //Vector2 vector_direction_input = new Vector2(1 if Input.is_action_pressed('ui_right') else -1 if Input.is_action_pressed('ui_left') else 0, 1 if Input.is_action_pressed('ui_down') else -1 if Input.is_action_pressed('ui_up') else 0);
        //Vector2 vector_direction_input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 vector_direction_input = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            vector_direction_input.x = -1;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            vector_direction_input.x = 1;
        }
        else
        {
            vector_direction_input.x = 0;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            vector_direction_input.y = -1;
        }
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            vector_direction_input.y = 1;
        }
        else
        {
            vector_direction_input.y = 0;
        }

        last_horizontal_direction = vector_direction_input.x != 0 ? vector_direction_input.x : last_horizontal_direction;
	
	    if (active)
        {
		    if (pState == PlayerState.State_normal) 
            {
                _state_normal(Time.deltaTime, vector_direction_input);
            }
		    else if (pState == PlayerState.State_dive) 
            {
                _state_dive(Time.deltaTime, vector_direction_input);
            }
        }
		
	    if (vector_direction_input!=Vector2.zero)
        {
            // smoothly rotate dive_aim based on input
            Vector3 newRot = dive_aim.transform.eulerAngles;
            newRot.z = Mathf.LerpAngle(dive_aim.transform.eulerAngles.z, Mathf.Atan2(vector_direction_input.y, vector_direction_input.x) * Mathf.Rad2Deg, 0.5f);
            dive_aim.transform.eulerAngles = newRot;
        }
        
    }

    private void _state_normal(float _delta, Vector2 vector_direction_input)
    {
        nVignette.color = new Color(0, 0, 0, Mathf.Lerp(nVignette.color.a, 0, 0.1f * (Time.deltaTime / (1f / 60f)))); // fade out vignette

        //set_collision_layer_bit(0, true)

        //set_collision_mask_bit(0, true)

        if (vectorVelocity.y > 0 && Input.GetKey(KeyCode.Z)) 
        {
            vector_gravity = vector_gravity_up;
        } 
        else if (is_on_floor())
        {
            vector_gravity = Vector2.zero;
        }
        else
        {
            vector_gravity = vector_gravity_down;
        }

        // sets buffers for jump and dive inputs
        if (Input.GetKeyDown(KeyCode.Z))
        {
		    jumpBuffer = maxJumpBuffer;
        }

        if (Input.GetKeyDown(KeyCode.X)) 
        {
            diveBuffer = maxDiveBuffer;
        }


        if (is_on_floor()) 
        {
            airTime = maxAirTime;
        }
        else 
        {
            airTime -= 0.5f;
        }

        jumpBuffer -= 0.5f;
        diveBuffer -= 0.5f;


        if (jumpBuffer > 0 && airTime > 0)
        {
            airTime = 0;
            jumpBuffer = 0;

            // animation
            StartCoroutine(JumpSquishTween());

            // jump sound effect
            sfxPlayer.clip = jumpSfx;
            sfxPlayer.Play();
            //$sounds / snd_jump.play()

            vectorVelocity.y = jump_force;

            // for particle effects
            PlayDustAnimation(jumpDustAnimator);
        }


        List<Collider2D> overlaps = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();

        if (diveBuffer > 0 && dive_aim_collider.OverlapCollider(filter, overlaps) > 0)
        {

            bool phaseable = true;
		    // Check if the tileset is unphaseable
		    foreach (Collider2D body in overlaps)
            {

                if (!body.gameObject.CompareTag("Phaseable"))
                {
                    phaseable = false;
                }
            }


            if (phaseable) 
            {
                // music change
                //global.changeToLowPassMusic()
                GameManager.Instance.ChangeToLowpassMusic();

                // sound effects
                if (UnityEngine.Random.value <= 0.5) 
                {
                    sfxPlayer.clip = diveSfx;
                    //$sounds / snd_dive.play()
                }
                else 
                {
                    sfxPlayer.clip = dive1Sfx;
				    //$sounds / snd_dive1.play()
                }
                sfxPlayer.Play();
                

                pState = PlayerState.State_dive;
                Vector2 cursor_position = dive_aim.transform.Find("Phase Point").transform.position;

                Vector3 starting_position = transform.position;
                Vector2 vector_target_position = new Vector2(Mathf.Floor(cursor_position.x) + .5f, Mathf.Floor(cursor_position.y) + .5f);


                StartCoroutine(DiveTween(starting_position, vector_target_position, twn_duration));


                CreateSplash(20, 30, -((Vector2)transform.position - vector_target_position), ((Vector2)transform.position - vector_target_position) / 2);
                /*
                 * Animation stuff
                createSpherize(vector_target_position)
			    $camera2D.minorShake()
                */

                return;
            }
        }


        //Debug.Log(vector_direction_input);
        vectorVelocity.x = Mathf.Lerp(vectorVelocity.x, maximum_speed * vector_direction_input.x, lerp_constant);

        if (vectorVelocity.y <= 0 && is_on_floor())
        {
            vectorVelocity.y = 0;
        } 
        vectorVelocity.y += vector_gravity.y * Time.deltaTime;

        GetComponent<Rigidbody2D>().velocity = vectorVelocity;


        if(!bWasOnFloor && is_on_floor())
        {
            sfxPlayer.clip = landSfx;
            sfxPlayer.Play();
            //$sounds / snd_land.play()

            PlayDustAnimation(landDustAnimator);

            //StopCoroutine(JumpSquishTween());
            StartCoroutine(LandSquishTween());
        }

        bWasOnFloor = is_on_floor();
    }



    private void _state_dive(float _delta, Vector2 vector_direction_input)
    {
        // set velocity to zero
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        // animation
        nVignette.color = new Color(0, 0, 0, Mathf.Lerp(nVignette.color.a, 1, 0.1f * (Time.deltaTime/(1f/60f)))); // fade in vignette

        if (Input.GetKeyDown(KeyCode.X)) 
        {
            diveBuffer = maxDiveBuffer;
        }
        diveBuffer -= 0.5f;

        List<Collider2D> overlaps = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        if (diveBuffer>0 && dive_aim_collider.OverlapCollider(filter, overlaps) == 0) 
        {
            /*
             * music and sfx stuff
		    global.changeFromLowPassMusic()
            */
            GameManager.Instance.ChangeToRegularMusic();
            sfxPlayer.clip = diveawaySfx;
            sfxPlayer.Play();
            //$sounds/snd_dive_away.play()
            pState = PlayerState.State_normal;

            Vector2 cursor_position = dive_aim.transform.Find("Phase Point").transform.position;

            Vector3 starting_position = transform.position;
            Vector2 vector_target_position = new Vector2(Mathf.Floor(cursor_position.x) + .5f, Mathf.Floor(cursor_position.y) + .5f);


            //var _v = nTwnDive.interpolate_property(self, 'global_position', self.global_position, vector_target_position, twn_duration, Tween.TRANS_QUART, Tween.EASE_OUT)
            StartCoroutine(DiveTween(starting_position, vector_target_position, twn_duration * .8f));


            CreateSplash(10, 15, -((Vector2)transform.position - vector_target_position), ((Vector2)transform.position - vector_target_position) / 2);
            /*
             * animation stuff
		    createSpherize()
		    $camera2D.minorShake()
            */

            vectorVelocity = Vector2.zero; //(self.global_position-vector_target_position).normalized()*maximum_speed #Add conservation of momentum maybe
        }
    }



    // takes starting position, target position, and the duration
    private IEnumerator DiveTween(Vector3 starting_position, Vector3 target_position, float duration)
    {

        // run when tween starts
        //self.active = false

        nSprite.gameObject.SetActive(!nSprite.gameObject.activeSelf);
        eyes.SetActive(!eyes.activeSelf);

        flag_constant_spritetrail = true;

        // prevent player from interacting with other objects
        //gameObject.GetComponent<Collider2D>().enabled = false;
        gameObject.GetComponent<CapsuleCollider2D>().enabled = false;

        Vector3 currentPosition = starting_position;
        float passedTime = 0;
        // interpolate position to target
        while(passedTime <= duration)
        {
            passedTime += Time.deltaTime;
            float alpha = passedTime / duration;

            currentPosition = Vector3.Lerp(starting_position, target_position, tween_interpolation_curve.Evaluate(alpha));
            transform.position = currentPosition;

            yield return null;
        }

        if (currentPosition != target_position)
        {
            transform.position = target_position;
        }


        // run when tween finishes
        //self.active = true

        flag_constant_spritetrail = false;

        if (pState == PlayerState.State_normal)
        {
            //gameObject.GetComponent<Collider2D>().enabled = true;
            gameObject.GetComponent<CapsuleCollider2D>().enabled = true;
        }
    }


    private IEnumerator JumpSquishTween()
    {
        Debug.Log("Jump Tween");

        StopCoroutine(LandSquishTween());
        yield return new WaitForEndOfFrame();

        //$twn_squishy.interpolate_property($sprite, 'scale', $sprite.scale * scale_vector, Vector2(1, 1), 0.5, Tween.TRANS_QUINT, Tween.EASE_OUT)
        float duration = .5f;
        Vector3 currentScale = new Vector3(.5f, 1.5f, 1);
        Vector3 targetScale = new Vector3(1, 1, 1);
        float passedTime = 0;
        // interpolate position to target
        while (passedTime <= duration)
        {
            passedTime += Time.deltaTime;
            float alpha = passedTime / duration;

            currentScale = Vector3.Lerp(currentScale, targetScale, alpha);
            nSprite.transform.localScale = currentScale;

            yield return null;
        }
        nSprite.transform.localScale = targetScale;
    }
    
    private IEnumerator LandSquishTween()
    {
        Debug.Log("Land Tween");
        //$twn_squishy.interpolate_property($sprite, 'scale', $sprite.scale * scale_vector, Vector2(1,1), 0.5, Tween.TRANS_QUINT, Tween.EASE_OUT)
        float duration = .5f;
        Vector3 currentScale = new Vector3(1.5f, .5f, 1);
        Vector3 targetScale = new Vector3(1, 1, 1);
        float passedTime = 0;
        Vector3 currentPos = nSprite.transform.localPosition;
        // interpolate position to target
        while (passedTime <= duration)
        {
            passedTime += Time.deltaTime;
            float alpha = passedTime / duration;

            currentScale = Vector3.Lerp(currentScale, targetScale, alpha);
            nSprite.transform.localScale = currentScale;

            // adjust position so it stays on ground
            currentPos = nSprite.transform.localPosition;
            currentPos.y = -1 * ((1-currentScale.y) / 2);
            nSprite.transform.localPosition = currentPos;

            yield return null;
        }
        nSprite.transform.localScale = targetScale;

        currentPos.y = Mathf.Floor(transform.position.y) + (1 * .536f);
        nSprite.transform.localPosition = Vector3.zero;
    }



    private void CreateSplash(int minimum, int maximum, Vector2 direction, Vector2 offset)
    {
        int numSplashes = UnityEngine.Random.Range(minimum, maximum);

        for (int i=0; i<numSplashes; i++)
        {
            GameObject inst = Instantiate(drop);
            inst.transform.position = (Vector2)transform.position + (offset * new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f)));

            inst.GetComponent<Drop>().direction = direction.normalized;
        }


    }



    private bool is_on_floor()
    {
        // check if the bottom of the collision box contacts the ground
        //RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(transform.position.x, transform.position.y), Vector2.down, (GetComponent<BoxCollider2D>().size.y / 2) + .05f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(transform.position.x, transform.position.y), Vector2.down, (GetComponent<CapsuleCollider2D>().size.y / 2) + .05f);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Phaseable") || hit.collider.gameObject.CompareTag("Unphaseable"))
            {
                return true;
            }
        }

        return false;
    }


    
    private void PlayDustAnimation(DustAnimationHandler dustPlayer)
    {
        dustPlayer.gameObject.SetActive(true);
        dustPlayer.PlayDustAnimation();
    }



    public void PlayFootstepSfx()
    {
        int randi = UnityEngine.Random.Range(0, footstepSfx.Length);
        sfxPlayer.clip = footstepSfx[randi];
        sfxPlayer.Play();
    }
}
