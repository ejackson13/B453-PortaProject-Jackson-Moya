using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using UnityEngine.XR;

public class player : MonoBehaviour
{

    private bool bWasOnFloor = false;
    private Vector2 vectorVelocity;
    private Vector2 vector_gravity;
    public float last_horizontal_direction = 1; // seems to only be -1, 0, or 1
    private bool active = true;
    private string anim = "";
    private Vector2 vSpriteOffset = new Vector2(0,8);

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
    private const float maxDiveBuffer = 0.51f;
    private float target_rotation = 0;
    private const float twn_duration = 0.25f;
    //const spritetrail= preload('res://Scenes/spritetrail/sprite_trail.tscn')
    //const spherizeShader= preload("res://Scenes/spherizeShader.tscn")
    //const drop= preload("res://Scenes/drop.tscn")
    //const fxPlayerJumpDust = preload("res://Scenes/fxPlayerJumpDust.tscn")
    //const fxPlayerLandDust = preload("res://Scenes/fxPlayerLandDust.tscn")

    [SerializeField] private SpriteRenderer nSprite; // sprite type instead?
    //onready var nSprEyes:Sprite = $eyes
    //onready var nAnimationPlayer:AnimationPlayer = $animation_player
    //onready var nDiveAim:Node2D =$dive_aim/dive_aim
    [SerializeField] private GameObject dive_aim;
    //onready var nTwnDive:Tween = $twn_dive
    //onready var nVignette:Sprite = $layerVignette/sprVignette


    // Start is called before the first frame update
    void Start()
    {
        //nSprEyes.visible = false
        //nVignette.modulate.a = 0

        // initialize dive aimer rotation
        Vector3 newRot = dive_aim.transform.eulerAngles;
        newRot.z = last_horizontal_direction == 1 ? 0 : -180;
        dive_aim.transform.eulerAngles = newRot;
    }

    // Update is called once per frame
    void Update()
    {
        nSprite.flipX = last_horizontal_direction != 1;


        if (pState == PlayerState.State_dive)
        {
            anim = "idle";
        }
        else
        {
            if (is_on_floor())
            {
                anim = Mathf.Abs(vectorVelocity.x) <= 10 ? "idle" : "walk";
            }
            else
            {
                anim = vectorVelocity.y < 0 ? "going_down" : "going_up";
            }
        }

        /*
         * Animations
        if (nAnimationPlayer.current_animation != anim):
		    nAnimationPlayer.play(anim)


        if flag_constant_spritetrail:
		    _create_spritetrail()


         * Resetting scene
        if Input.is_action_just_pressed('ui_reset'):
		    var _v = get_tree().reload_current_scene()
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
            /*
             * Diving
		    else if (pState == PlayerState.State_dive) 
            {
                _state_dive(delta, vector_direction_input);
            }
            */
        }
		
	    if (vector_direction_input!=Vector2.zero)
        {
            // smoothly rotate dive_aim based on input
            Vector3 newRot = dive_aim.transform.eulerAngles;
            newRot.z = Mathf.LerpAngle(dive_aim.transform.eulerAngles.z, Mathf.Atan2(vector_direction_input.y, vector_direction_input.x), 0.5f);
            dive_aim.transform.eulerAngles = newRot;
        }
        
    }

    private void _state_normal(float _delta, Vector2 vector_direction_input)
    {

        // I believe this is for the visual effect that appears when the player enters a block
        //nVignette.modulate.a = lerp(nVignette.modulate.a, 0, 0.1);

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

        /*
         * Dive mechanic
        if (Input.GetKeyDown(KeyCode.X)) 
        {
            diveBuffer = maxDiveBuffer;
        }
        */


        if (is_on_floor()) 
        {
            airTime = maxAirTime;
        }
        else 
        {
            airTime -= 0.5f;
        }

        jumpBuffer -= 0.5f; ;
        //diveBuffer -= 0.5f;


        if (jumpBuffer > 0 && airTime > 0)
        {
            airTime = 0;
            jumpBuffer = 0;

            // animation
            //_twn_squishy()

            // jump sound effect
            //$sounds / snd_jump.play()

            vectorVelocity.y = jump_force;

            // for particle effects
            //var i = fxPlayerJumpDust.instance();
            //i.global_position = self.global_position - vSpriteOffset
            //get_parent().add_child(i)
        }


        /*
         * For diving into blocks
        if (diveBuffer > 0 && dive_aim.get_overlapping_bodies().size() > 0)
        { 

            var phaseable = true
		    #Check if the tileset is unphaseable
		    for body in dive_aim.get_overlapping_bodies():

                if body.is_in_group('Unphaseable'):

                    phaseable = false


            if phaseable:
			    global.changeToLowPassMusic()


                if randf() <= 0.5:
				    $sounds / snd_dive.play()

                else:
				    $sounds / snd_dive1.play()

                self.state = State_dive
                var cursor_position = dive_aim.get_node('position_2d').global_position#-Vector2(0,1)
			    var vector_target_position = Vector2(
                    floor(cursor_position.x / global.tile_size.x) * global.tile_size.x,\

                    floor(cursor_position.y / global.tile_size.y) * global.tile_size.y \
				    ) + global.tile_size / 2 + vSpriteOffset


                var _v = nTwnDive.interpolate_property(self, 'global_position', self.global_position, vector_target_position, twn_duration, Tween.TRANS_QUART, Tween.EASE_OUT)

                _v = nTwnDive.start()

                nAnimationPlayer.play("idle")

                create_splash(20, 30, -(self.global_position - vector_target_position), (self.global_position - vector_target_position) / 2)

                createSpherize(vector_target_position)
			    $camera2D.minorShake()

                return;
        }
        */

        var initialVelocity = vectorVelocity;

        //Debug.Log(vector_direction_input);
        vectorVelocity.x = Mathf.Lerp(vectorVelocity.x, maximum_speed * vector_direction_input.x, lerp_constant);

        if (vectorVelocity.y <= 0 && is_on_floor())
        {
            vectorVelocity.y = 0;
        } 
        vectorVelocity.y += vector_gravity.y * Time.deltaTime;

        GetComponent<Rigidbody2D>().velocity = vectorVelocity;

        /*
         * landing sound effect
        if (initialVelocity.y != vectorVelocity.y and self.is_on_floor())
        { 
		    $sounds / snd_land.play()
        }
        */

        /*
         * Particle effects and animation
        if !self.bWasOnFloor and self.is_on_floor():
		    var i = fxPlayerLandDust.instance()

            i.global_position = self.global_position - vSpriteOffset
            get_parent().add_child(i)

            tweenLandSquish()
        */

        bWasOnFloor = is_on_floor();
    }

    private bool is_on_floor()
    {
        // check if the bottom of the collision box contacts the ground
        RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(transform.position.x, transform.position.y), Vector2.down, (GetComponent<BoxCollider2D>().size.y / 2) + .05f);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Ground"))
            {
                return true;
            }
        }

        return false;
    }

}
