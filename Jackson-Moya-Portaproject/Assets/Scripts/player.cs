using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using UnityEngine.XR;

public class player : MonoBehaviour
{

    private bool bWasOnFloor = false;
    private Vector2 vectorVelocity;
    private Vector2 vector_gravity;
    public float last_horizontal_direction; // seems to only be -1, 0, or 1
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
    public Color color_spritetrail;
    private bool flag_can_create_spritetrail = true;

    private const float lerp_constant = 0.5f;
    private Vector2 vector_gravity_up = new Vector2(0, 10); // should be const
    private Vector2 vector_gravity_down = new Vector2(0, 12.5f); // should be const

    private Vector2 vector_normal = new Vector2(0, -1); // should be const
    private const float maximum_speed = 95;
    private const float jump_force = 175;

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

    private SpriteRenderer nSprite; // sprite type instead?
    //onready var nSprEyes:Sprite = $eyes
    //onready var nAnimationPlayer:AnimationPlayer = $animation_player
    //onready var nDiveAim:Node2D =$dive_aim/dive_aim
    //onready var dive_aim:Node2D =$dive_aim/dive_aim
    //onready var nTwnDive:Tween = $twn_dive
    //onready var nVignette:Sprite = $layerVignette/sprVignette


    // Start is called before the first frame update
    void Start()
    {
        //add_to_group('Player')   // like giving object tag player
        //nSprEyes.visible = false
        //global.player = self  // unnecessary i believe
        //nVignette.modulate.a = 0
        //nDiveAim.rotation = 0.0 if self.last_horizontal_direction == 1 else -PI
    }

    // Update is called once per frame
    void Update()
    {
        nSprite.flipY = last_horizontal_direction != 1;


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
                anim = vectorVelocity.y < 0 ? "going_up" : "going_down";
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
        Vector2 vector_direction_input = new Vector2(Input.GetAxis("horizontal"), Input.GetAxis("vertical") * -1); // vertical input seems to be inverted for some reason
        last_horizontal_direction = vector_direction_input.x != 0 ? vector_direction_input.x : last_horizontal_direction;
	
	    if (active)
        {
		    if (pState == PlayerState.State_normal) 
            {
                _state_normal(delta, vector_direction_input);
            }
            /*
             * Diving
		    else if (pState == PlayerState.State_dive) 
            {
                _state_dive(delta, vector_direction_input);
            }
            */
        }
		
        /*
         * Dive aimer
	    if (vector_direction_input!=Vector2.zero)
        { 
		    $dive_aim.rotation = lerp_angle($dive_aim.rotation, vector_direction_input.angle(), 0.5)
        }
        */
    }


    private bool is_on_floor()
    {
        // check if the bottom of the collision box contacts the ground
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - GetComponent<BoxCollider2D>().size.y / 2), Vector2.down, .01f);

        // may need to update to tell difference between colliders, will come back when tags for ground objects and whatnot have been decided upon
        return hit.transform != null;
    }

}
