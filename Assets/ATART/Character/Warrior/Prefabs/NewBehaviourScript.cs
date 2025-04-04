using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    
    public float energia;
    public float velRecuperacionEn;
    public Slider Energ;
    public float walkSpeed;
    public float walkSpeedFixed;
    public float runSpeed;
    public float runSpeedFixed;

    bool walk;
    bool run;
    float lastPosX;
    float lastPosY;

    public float force;
    public bool jumping;
    public bool attacking;
    public bool running;

    public float tocarSuelo;
    public float caer;
    public float acabarAtacar;
    float attackStartTime;
    float timeAtJump;

    public GameObject camera;
    GameObject currentEnemy;
    public Vector3 move;

    public float gravity;
    public int combos;
    public enum ComboAtaques { agua, fuego , special1,special2,Special3, Slide, Realista1};

    public int currentScroll;
    [System.Serializable]
    public struct Ataques
    {
        public float ataque;
        public float delay;
        public bool attacking;
        public GameObject effects;
        public GameObject[] cuts;
        public float effectDelay;
        public string name;
        public float animationSpeed;
        public float force;
        public float delayForce;
        public CollisionesArma col;
        public int Pesado;
        public float transition;
        public AnimationCurve curvaDeVelocidad;
        public float consumoEnergia;
        public Vector3 effectTransformPos;
        public Quaternion effectTransformRot;

    }
    [System.Serializable]
    public struct ListaAtaques
    {
        public ComboAtaques combo;
        public Ataques[] attacks;

    }
    [SerializeField]
    public ListaAtaques[] attack;
    float lastMoveValue;
    ListaAtaques currentAttack;

	public ComboAtaques currentCombo;
	public ComboAtaques currentComboDebil;
	public ComboAtaques currentComboFuerte;

	int currentComboAttack;
    public bool enemyFix;
    public GameObject[] enemies;
    Vector3 fixedEnemyY;
    Vector3 fixedEnemyX;

    int enemyFixed;
    public Animator anim;
    public bool enAire;
    float timeAttack;
    Gamepad controller;
    public bool dash;
    public float tiempoDash;
    public float DashForce;
    public float delayDash;
    float dashStartTime;
    bool noMando;

    int movementDirection;

    bool canChangeFixed;

    int lastDir;

    bool walkVelAnimation;
    public float CameraRotatSpeed;
    bool timeAtAir;
    float StartTimeAtAir;
    public GameObject direccionAtaques;
    bool doDash;
    bool StartRunning;
    float BButtonStartTimePressed;

    public GameObject GuardarEffectos;
    public GameObject LanzarEffectos;
    bool correr;
    float alturaAlEmpezarSaltar;
    bool block;
    public float endBlock;

    public float delayLastAttack;
    // Start is called before the first frame update
    void Start()
    {
        block = false;
        BButtonStartTimePressed = 0;
        StartTimeAtAir = 0;
        timeAtAir = false;
        lastMoveValue = 0;
        walkVelAnimation = true;
        fixedEnemyY = new Vector3();
        fixedEnemyX = new Vector3();
        enAire = false;

        lastDir = 4;
        canChangeFixed  = true;
        movementDirection = 1;
        dashStartTime = 0;
        running = false;
        for (int i=0; i < Gamepad.all.Count;i++)
        {
            Debug.Log(Gamepad.all[i].name);
            controller = Gamepad.all[i];
        }
        noMando = true;
        anim.CrossFade("Idle", 0);

        enemyFixed = 0;
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        currentScroll = 0;
        enemyFix = false;
        currentComboAttack = 0;
        currentCombo = ComboAtaques.agua;
        move = new Vector3();
        lastPosY = Input.mousePosition.x;
        lastPosX = Input.mousePosition.y;

        jumping = false;
        attacking = false;
        for (int i = 0; i < attack.Length; i++)
        {
            if (attack[i].combo == currentCombo)
            {
                currentAttack = attack[i];
                break;
            }
        }

        for(int i = 1; i < attack.Length; i++)
        {
            for (int e = 1; e < attack[i].attacks.Length; e++)
            {
                attack[i].attacks[e].effectTransformPos = attack[i].attacks[e].effects.transform.localPosition;
                attack[i].attacks[e].effectTransformRot = attack[i].attacks[e].effects.transform.localRotation;

            }
        }
    }
    IEnumerator rotate(float y,float x)
    {

        yield return new WaitForSeconds(.1f);
        camera.transform.Rotate(new Vector3(0, y, 0) * Time.deltaTime * CameraRotatSpeed);
        float a = camera.transform.GetChild(0).GetChild(0).localEulerAngles.x;

        if((a < 80 && x > 0) || (a > 5 && x < 0))
        camera.transform.GetChild(0).GetChild(0).Rotate(new Vector3(x, 0, 0) * Time.deltaTime * CameraRotatSpeed);
    }
    void QuitarEfecto()
    {
        currentAttack.attacks[currentComboAttack].effects.transform.SetParent(LanzarEffectos.transform);

    }
    IEnumerator EfectoAtaque(GameObject effect, float delay,GameObject[] plane,CollisionesArma col)
    {
        if(effect != null)
        {
        Invoke("QuitarEfecto", currentAttack.attacks[currentComboAttack].ataque-0.01f);

        yield return new WaitForSeconds(delay);

        
        effect.transform.SetParent(GuardarEffectos.transform);
        effect.transform.localPosition = currentAttack.attacks[currentComboAttack].effectTransformPos;
        effect.transform.localRotation = currentAttack.attacks[currentComboAttack].effectTransformRot;

        effect.GetComponent<ParticleSystem>().Play();
        energia -= currentAttack.attacks[currentComboAttack].consumoEnergia;
        }

        yield return new WaitForSeconds(0.15f);

        for(int i = 0; i < plane.Length;i++)
        {

            plane[i].GetComponent<DynamicMeshCutter.PlaneBehaviour>().Cut(col.GetObjects());
            yield return new WaitForSeconds(0.1f);

        }
    }
    void WalkAllDir()
    {
        switch (movementDirection)
        {
            case 0:
                anim.CrossFadeInFixedTime("WalkUpLeft", 0.2f);

                break;
            case 1:
                anim.CrossFadeInFixedTime("WalkUp", 0.2f);

                break;
            case 2:
                anim.CrossFadeInFixedTime("WalkUpRight", 0.2f);

                break;
            case 3:
                anim.CrossFadeInFixedTime("WalkLeft", 0.2f);

                break;
            case 5:
                anim.CrossFadeInFixedTime("WalkRight", 0.2f);

                break;
            case 6:
                anim.CrossFadeInFixedTime("WalkDownLeft", 0.2f);

                break;
            case 7:
                anim.CrossFadeInFixedTime("WalkDown", 0.2f);

                break;
            case 8:
                anim.CrossFadeInFixedTime("WalkDownRight", 0.2f);

                break;
        }
    }
    void RunAllDir()
    {
        switch (movementDirection)
        {
            case 0:
                anim.CrossFadeInFixedTime("RunUpLeft", 0.2f);

                break;
            case 1:
                anim.CrossFadeInFixedTime("RunUp", 0.2f);

                break;
            case 2:
                anim.CrossFadeInFixedTime("RunUpRight", 0.2f);

                break;
            case 3:
                anim.CrossFadeInFixedTime("RunLeft", 0.2f);

                break;
            case 5:
                anim.CrossFadeInFixedTime("RunRight", 0.2f);

                break;
            case 6:
                anim.CrossFadeInFixedTime("RunDownLeft", 0.2f);

                break;
            case 7:
                anim.CrossFadeInFixedTime("RunDown", 0.2f);

                break;
            case 8:
                anim.CrossFadeInFixedTime("RunDownRight", 0.2f);

                break;
        }
    }
    
    void Movement()
    {
        if ((controller.leftStick.ReadValue().magnitude > 0.25f) && !attacking && !dash && !jumping && !block)
        {
            if(!running)
            {        
                if (!enemyFix)
                {

                    if((controller.leftStick.ReadValue().magnitude > 0.5f))
                    {
                        Vector3 dir;
                        this.transform.GetChild(0).GetChild(0).transform.localPosition += new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                        dir = this.transform.GetChild(0).GetChild(0).transform.position - this.transform.position;
                        this.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
                        move = dir * walkSpeed * Time.deltaTime;
                        changeWalk();

                    }
                    else
                    {
                        Vector3 dir;
                        this.transform.GetChild(0).GetChild(0).transform.localPosition += new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                        dir = this.transform.GetChild(0).GetChild(0).transform.position - this.transform.position;
                        this.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
                        move = dir * walkSpeed * Time.deltaTime*0.35f;
                        changeWalk();

                    }

                    camera.transform.GetChild(1).localPosition += new Vector3(controller.leftStick.ReadValue().normalized.x, 0, controller.leftStick.ReadValue().normalized.y);
                    this.transform.LookAt(camera.transform.GetChild(1).position);
                    camera.transform.GetChild(1).localPosition = new Vector3();


                }
                else
                {
                    if (movementDirection > 5)
                    {
                        Vector3 dir;
                        this.transform.GetChild(0).GetChild(0).transform.localPosition += new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                        dir = this.transform.GetChild(0).GetChild(0).transform.position - this.transform.position;
                        this.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
                        move = dir * walkSpeedFixed * Time.deltaTime*0.75f;
                    }
                    else
                    {
                        Vector3 dir;
                        this.transform.GetChild(0).GetChild(0).transform.localPosition += new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                        dir = this.transform.GetChild(0).GetChild(0).transform.position - this.transform.position;
                        this.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
                        move = dir * walkSpeedFixed * Time.deltaTime;
                    }
                    //this.transform.LookAt(fixedEnemyY);
                    //camera.transform.LookAt(fixedEnemyY);
                    //direccionAtaques.transform.LookAt(fixedEnemyX);
                    //camera.transform.GetChild(0).GetChild(0).LookAt(fixedEnemyX);

                    anim.speed = 1f;

                    if (lastDir != movementDirection && !jumping)
                    {
                        lastDir = movementDirection;
                        WalkAllDir();
                    }
                }

            }



            if (running && !attacking)
            {
                energia -= 5 * Time.deltaTime;
                if (!enemyFix)
                {
                    Vector3 dir;
                    this.transform.GetChild(0).GetChild(0).transform.localPosition += new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                    dir = this.transform.GetChild(0).GetChild(0).transform.position - this.transform.position;
                    this.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
                    move = dir * runSpeed * Time.deltaTime;
                    camera.transform.GetChild(1).localPosition += new Vector3(controller.leftStick.ReadValue().normalized.x, 0, controller.leftStick.ReadValue().normalized.y);
                    this.transform.LookAt(camera.transform.GetChild(1).position);
                    camera.transform.GetChild(1).localPosition = new Vector3();
                }
                else
                {

                        Vector3 dir;
                        this.transform.GetChild(0).GetChild(0).transform.localPosition += new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                        dir = this.transform.GetChild(0).GetChild(0).transform.position - this.transform.position;
                        this.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
                        move = dir * runSpeedFixed * Time.deltaTime;
                    
                    //this.transform.LookAt(fixedEnemyY);
                    //camera.transform.LookAt(fixedEnemyY);
                    //camera.transform.GetChild(0).GetChild(0).LookAt(fixedEnemyX);
                    //direccionAtaques.transform.LookAt(fixedEnemyX);

                    //if (movementDirection >2)
                    //anim.speed = 1.25f;
                    //else
                        anim.speed = 0.85f;

                    //if (lastDir != movementDirection && !jumping)
                    //{
                    //    lastDir = movementDirection;
                    //    RunAllDir();
                    //}


                }

            }
            //this.transform.GetChild(0).GetChild(0).GetComponent<Rigidbody>().AddForce(move,ForceMode.Force);

            


        }
    }
    void Correr()
    {
        if (StartRunning && energia > 0)
        {
            if (walk && !running)
            {
                StartRunning = false;
                running = true;
                lastDir = -1;
                if (!attacking && !jumping && !block)
                    anim.CrossFadeInFixedTime("Run", 0.2f);

            }
        }
    }
    void DashAllDir()
    {
        Vector3 dir = new Vector3();
        switch (movementDirection)
        {
            case 1:
                anim.CrossFadeInFixedTime("DashUp", 0.2f);

                camera.transform.GetChild(1).localPosition -= new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                dir = this.transform.position - (camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3(0, 0, 0); 
                break;
            case 3:
                anim.CrossFadeInFixedTime("DashLeft", 0.2f);
                camera.transform.GetChild(1).localPosition -= new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                dir = this.transform.position - (camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3(0, 0, 0);
                break;
            case 0:
                anim.CrossFadeInFixedTime("DashLeft", 0.2f);
                camera.transform.GetChild(1).localPosition -= new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                dir = this.transform.position - (camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3(0, 0, 0);
                break;
            case 6:
                anim.CrossFadeInFixedTime("DashLeft", 0.2f);
                camera.transform.GetChild(1).localPosition -= new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                dir = this.transform.position - (camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3(0, 0, 0);
                break;
            case 2:
                anim.CrossFadeInFixedTime("DashRight", 0.2f);
                camera.transform.GetChild(1).localPosition -= new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                dir = this.transform.position - (camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3(0, 0, 0);
                break;
            case 5:
                anim.CrossFadeInFixedTime("DashRight", 0.2f);
                camera.transform.GetChild(1).localPosition -= new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                dir = this.transform.position - (camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3(0, 0, 0);
                break;
            case 8:
                anim.CrossFadeInFixedTime("DashRight", 0.2f);
                camera.transform.GetChild(1).localPosition -= new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                dir = this.transform.position - (camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3(0, 0, 0);
                break;
            case 7:
                anim.CrossFadeInFixedTime("DashDown", 0.2f);
                camera.transform.GetChild(1).localPosition -= new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
                dir = this.transform.position - (camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3(0, 0, 0);
                break;
        }
        this.transform.GetComponent<Rigidbody>().AddForce(dir.normalized * Time.fixedDeltaTime * DashForce, ForceMode.Impulse);

    }
    void Dash()
    {
        if (walk && !jumping && !attacking && doDash && !dash && (Time.time - dashStartTime) > delayDash && energia > 0 && !block)
        {
			this.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;

			doDash = false;
            energia -= 10;
            dashStartTime = Time.time;
            dash = true;
            Vector3 dir;
            this.transform.GetChild(0).GetChild(0).transform.localPosition += new Vector3(controller.leftStick.ReadValue().x, 0, controller.leftStick.ReadValue().y).normalized;
            dir = this.transform.GetChild(0).GetChild(0).transform.position - this.transform.position;
            this.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(0, 0, 0);
            move = dir * walkSpeed * 0.005f;
            if (!enemyFix)
            {
                anim.CrossFadeInFixedTime("Dash", 0.1f);
                this.transform.GetComponent<Rigidbody>().AddForce(this.transform.forward * Time.fixedDeltaTime * DashForce, ForceMode.Impulse);
                Invoke("SalirDash", tiempoDash);

            }
            else
            {
                DashAllDir();
                Invoke("SalirDash", 0.4f);

            }

        }
    }

    void MoveDir()
    {
        if ((controller.leftStick.ReadValue().magnitude > 0))
        {
            switch (controller.leftStick.ReadValue().x)
            {
                case > 0.25f:
                    switch (controller.leftStick.ReadValue().y)
                    {
                        case < -0.25f:
                            movementDirection = 8;

                            break;
                        case > 0.25f:
                            movementDirection = 2;

                            break;
                        default:
                            movementDirection = 5;

                            break;
                    }
                    break;
                case < -0.25f:
                    switch (controller.leftStick.ReadValue().y)
                    {
                        case < -0.25f:
                            movementDirection = 6;

                            break;
                        case > 0.25f:
                            movementDirection = 0;

                            break;
                        default:
                            movementDirection = 3;

                            break;
                    }
                    break;
                default:
                    switch (controller.leftStick.ReadValue().y)
                    {
                        case < -0.25f:
                            movementDirection = 7;

                            break;
                        case > 0.25f:
                            movementDirection = 1;

                            break;
                        default:
                            movementDirection = -1;

                            break;
                    }
                    break;
            }
        }
    }

    void changeWalk()
    {

        if(controller.leftStick.ReadValue().magnitude < 0.5f && (lastMoveValue < 0.1f || lastMoveValue >= 0.5f))
        {
            walkVelAnimation = true;

        }
        else if (controller.leftStick.ReadValue().magnitude >= 0.5f && ((lastMoveValue < 0.5f)||running))
        {
            walkVelAnimation = false;

        }

        if (walkVelAnimation && controller.leftStick.ReadValue().magnitude > 0.25f && controller.leftStick.ReadValue().magnitude < 0.5f)
        {
            walkVelAnimation = false;
            anim.CrossFadeInFixedTime("WalkLento", 0.2f);

        }
        else if(!walkVelAnimation && controller.leftStick.ReadValue().magnitude >= 0.5f)
        {
            walkVelAnimation = true;
            anim.CrossFadeInFixedTime("Walk", 0.2f);

        }

    }
    void CloseShield()
    {
        if(!block)
        {

            shieldPos = false;
        }


    }
    bool shieldPos = false;

    void SalirBlock()
    {
        block = false;
        returnNormal();

    }
    // Update is called once per frame


    public float smooth;
    void Update()
    {
        if (controller == null)
        {
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                Debug.Log(Gamepad.all[i].name);
                controller = Gamepad.all[i];
            }
            if (noMando)
            {
                anim.CrossFadeInFixedTime("Idle", 0.1f);
                noMando = false;
            }
            return;

        }

        if(controller.rightTrigger.wasPressedThisFrame && !attacking)
        {
            currentCombo = currentComboDebil;
			for (int i = 0; i < attack.Length; i++)
			{
				if (attack[i].combo == currentCombo)
				{
					currentAttack = attack[i];
					break;
				}
			}
		}
        else if(controller.rightShoulder.wasPressedThisFrame && !attacking)
		{
			currentCombo = currentComboFuerte;
			for (int i = 0; i < attack.Length; i++)
			{
				if (attack[i].combo == currentCombo)
				{
					currentAttack = attack[i];
					break;
				}
			}
		}




		if (controller.leftShoulder.wasPressedThisFrame && !block && !jumping && !attacking && !dash)
        {            
            block = true;
            move = new Vector3();
            this.GetComponent<Rigidbody>().velocity = Vector3.zero;
            anim.CrossFadeInFixedTime("Block", 0.2f);
            Invoke("SalirBlock", endBlock);
        }

        if (controller.bButton.wasPressedThisFrame && walk)
        {
            correr = true;
            BButtonStartTimePressed = Time.time;
        }

        if(controller.bButton.wasReleasedThisFrame)
        {
            if((Time.time - BButtonStartTimePressed)<0.5f && !jumping && !attacking && !dash && energia > 0)
            {
                doDash = true;
                this.GetComponent<Rigidbody>().drag = 10;
            }
            else
            {
                if (running && walk)
                {

                    if (!jumping && !attacking && !dash)
                    {
                        if (!enemyFix)
                        {
                            changeWalk();
                        }
                        else
                        {
                            WalkAllDir();
                        }
                    }


                    running = false;

                }

            }
        }

        if(controller.bButton.isPressed && correr)
        {
            if ((Time.time - BButtonStartTimePressed) > 0.5f && !running && energia > 0)
            {
                StartRunning = true;
                correr = false;
            }
        }

        if(enAire && !timeAtAir)
        {
            StartTimeAtAir = Time.time;
            timeAtAir = true;
        }
        else if(enAire && timeAtAir)
        {
            if(Time.time-StartTimeAtAir > 5)
            {
                Debug.Log("muerto");
                running = false;
                enAire = false;
            }
        }
        if(!enAire && timeAtAir)
        {
            timeAtAir = false;
        }
        if(jumping)
        {
            RaycastHit hit2;

            if (Physics.Raycast(this.transform.position+new Vector3(0,0.5f,0), move.normalized, out hit2,2,3))
            {


                if (hit2.distance < 1f)
                {
                    move = new Vector3();
                    this.transform.GetComponent<Rigidbody>().AddForce(-transform.up * gravity * Time.fixedDeltaTime, ForceMode.Force);

                }
            }

        }
        if (controller.leftStick.ReadValue().magnitude < 0.5f && (lastMoveValue == 0 || lastMoveValue >= 0.5f))
        {
            walkVelAnimation = true;

        }
        else if (controller.leftStick.ReadValue().magnitude >= 0.5f && (lastMoveValue < 0.5f))
        {
            walkVelAnimation = false;

        }
        Energ.value = energia;

        if(!running && !attacking && !jumping && !dash)
        {
            if (energia < 100)
                energia += velRecuperacionEn * Time.deltaTime;
            else
                energia = 100;
            
        }

        //Esto tiene que estar arriba
        if (controller.aButton.wasPressedThisFrame && !jumping && !attacking && !dash && energia > 0 && !block)
        {
            move *= 0.2f;
            energia -= 10;
            enAire = true;
            timeAtJump = Time.time;
            jumping = true;
            alturaAlEmpezarSaltar = this.transform.position.y;
            this.GetComponent<Rigidbody>().drag = 5;

            anim.CrossFadeInFixedTime("Jump", 0.1f);
            this.GetComponent<Rigidbody>().AddForce(Vector3.up * force * Time.fixedDeltaTime, ForceMode.Impulse);
        }

        for (int i = 0; i < enemies.Length;i++)
        {
            if(enemies[i] == null)
            {
                enemies = GameObject.FindGameObjectsWithTag("Enemy");

                return;
            }
        }
        if(currentEnemy == null && enemyFix)
        {
            direccionAtaques.transform.rotation = new Quaternion();

            enemyFix = false;
            if(!attacking)
            {
                anim.speed = 1;
                returnNormal();
            }

        }

        if (enemyFix)
        {
            fixedEnemyY = currentEnemy.transform.position;
            fixedEnemyX = currentEnemy.transform.position;


            fixedEnemyY = new Vector3(fixedEnemyY.x, this.transform.position.y, fixedEnemyY.z);

            if(running)
            {

                camera.transform.GetChild(1).localPosition += new Vector3(controller.leftStick.ReadValue().normalized.x, 0, controller.leftStick.ReadValue().normalized.y);
                this.transform.LookAt(camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3();

				if (fixedEnemyY != null)
				{
					Vector3 targetDirectionCamera = fixedEnemyY - camera.transform.position;
					Quaternion targetRotationCamera = Quaternion.LookRotation(targetDirectionCamera);
					camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, targetRotationCamera, Time.deltaTime * smooth); // smoothTime debe estar definido
				}

				// LookAt suave para camera.transform.GetChild(0).GetChild(0)
				if (fixedEnemyX != null && camera.transform.childCount > 0 && camera.transform.GetChild(0).childCount > 0)
				{
					Transform childTransform = camera.transform.GetChild(0).GetChild(0);
					Vector3 targetDirectionChild = fixedEnemyX - childTransform.position;
					Quaternion targetRotationChild = Quaternion.LookRotation(targetDirectionChild);
					childTransform.rotation = Quaternion.Slerp(childTransform.rotation, targetRotationChild, Time.deltaTime * smooth); // smoothTime debe estar definido
				}

				// LookAt suave para direccionAtaques.transform
				if (direccionAtaques != null && fixedEnemyX != null)
				{
					Vector3 targetDirectionAtaques = fixedEnemyX - direccionAtaques.transform.position;
					Quaternion targetRotationAtaques = Quaternion.LookRotation(targetDirectionAtaques);
					direccionAtaques.transform.rotation = Quaternion.Slerp(direccionAtaques.transform.rotation, targetRotationAtaques, Time.deltaTime * smooth); // smoothTime debe estar definido
				}
			}
            else
            {
                this.transform.LookAt(fixedEnemyY);
				if (fixedEnemyY != null)
				{
					Vector3 targetDirectionCamera = fixedEnemyY - camera.transform.position;
					Quaternion targetRotationCamera = Quaternion.LookRotation(targetDirectionCamera);
					camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, targetRotationCamera, Time.deltaTime * smooth); // smoothTime debe estar definido
				}

				// LookAt suave para camera.transform.GetChild(0).GetChild(0)
				if (fixedEnemyX != null && camera.transform.childCount > 0 && camera.transform.GetChild(0).childCount > 0)
				{
					Transform childTransform = camera.transform.GetChild(0).GetChild(0);
					Vector3 targetDirectionChild = fixedEnemyX - childTransform.position;
					Quaternion targetRotationChild = Quaternion.LookRotation(targetDirectionChild);
					childTransform.rotation = Quaternion.Slerp(childTransform.rotation, targetRotationChild, Time.deltaTime * smooth); // smoothTime debe estar definido
				}

				// LookAt suave para direccionAtaques.transform
				if (direccionAtaques != null && fixedEnemyX != null)
				{
					Vector3 targetDirectionAtaques = fixedEnemyX - direccionAtaques.transform.position;
					Quaternion targetRotationAtaques = Quaternion.LookRotation(targetDirectionAtaques);
					direccionAtaques.transform.rotation = Quaternion.Slerp(direccionAtaques.transform.rotation, targetRotationAtaques, Time.deltaTime * smooth); // smoothTime debe estar definido
				}
			}


        }
        if (dash&&enemyFix)
        {
            anim.speed = 1;

            if (running)
            {

                camera.transform.GetChild(1).localPosition += new Vector3(controller.leftStick.ReadValue().normalized.x, 0, controller.leftStick.ReadValue().normalized.y);
                this.transform.LookAt(camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3();
                camera.transform.LookAt(fixedEnemyY);
                camera.transform.GetChild(0).GetChild(0).LookAt(fixedEnemyX);
                direccionAtaques.transform.LookAt(fixedEnemyX);


            }
            else
            {
                this.transform.LookAt(fixedEnemyY);
                camera.transform.LookAt(fixedEnemyY);
                camera.transform.GetChild(0).GetChild(0).LookAt(fixedEnemyX);
                direccionAtaques.transform.LookAt(fixedEnemyX);
            }

        }

        this.transform.GetChild(1).localPosition = new Vector3(0, 0, 0);
        MoveDir();
        Movement();


        this.transform.GetChild(0).rotation = camera.transform.rotation;

        Correr();

        Dash();


        if(controller.rightStickButton.wasPressedThisFrame && !enemyFix&& enemies.Length > 0 && !block && !dash)
        {

            float value = 1;
            for(int i = 0; i < enemies.Length;i++)
            {
                if(Vector3.Dot(camera.transform.forward, camera.transform.position - enemies[i].transform.position) < value)
                {
                    enemyFixed = i;
                    fixedEnemyY = enemies[i].transform.position;
                    fixedEnemyX = enemies[i].transform.position;
                    currentEnemy = enemies[i];
                    enemyFix = true;
                    fixedEnemyY = new Vector3(fixedEnemyY.x,this.transform.position.y, fixedEnemyY.z);
                    value = Vector3.Dot(camera.transform.forward, camera.transform.position - enemies[i].transform.position);
                }
            }
            if (running)
            {

                camera.transform.GetChild(1).localPosition += new Vector3(controller.leftStick.ReadValue().normalized.x, 0, controller.leftStick.ReadValue().normalized.y);
                this.transform.LookAt(camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3();
                camera.transform.LookAt(fixedEnemyY);
                camera.transform.GetChild(0).GetChild(0).LookAt(fixedEnemyX);
                direccionAtaques.transform.LookAt(fixedEnemyX);


            }
            else
            {
                this.transform.LookAt(fixedEnemyY);
                camera.transform.LookAt(fixedEnemyY);
                camera.transform.GetChild(0).GetChild(0).LookAt(fixedEnemyX);
                direccionAtaques.transform.LookAt(fixedEnemyX);
            }

            if (running)
            {
                RunAllDir();
            }
            else if(walk)
            {
                WalkAllDir();
            }

        }
        else if (controller.rightStickButton.wasPressedThisFrame && enemyFix && !block && !dash)
        {
            direccionAtaques.transform.rotation = new Quaternion();

            enemyFix = false;
            anim.speed = 1;
            returnNormal();
        }

        if (enemyFix&& Mathf.Abs(controller.rightStick.ReadValue().x) > 0.3f && canChangeFixed)
        {
                canChangeFixed = false;
                enemyFixed = ((int)enemyFixed+1) %enemies.Length;
                fixedEnemyY = enemies[enemyFixed].transform.position;
            fixedEnemyX = enemies[enemyFixed].transform.position;
            currentEnemy = enemies[enemyFixed];

            fixedEnemyY = new Vector3(fixedEnemyY.x, this.transform.position.y, fixedEnemyY.z);
            if (running)
            {

                camera.transform.GetChild(1).localPosition += new Vector3(controller.leftStick.ReadValue().normalized.x, 0, controller.leftStick.ReadValue().normalized.y);
                this.transform.LookAt(camera.transform.GetChild(1).position);
                camera.transform.GetChild(1).localPosition = new Vector3();
                camera.transform.LookAt(fixedEnemyY);
                camera.transform.GetChild(0).GetChild(0).LookAt(fixedEnemyX);
                direccionAtaques.transform.LookAt(fixedEnemyX);


            }
            else
            {
                this.transform.LookAt(fixedEnemyY);
                camera.transform.LookAt(fixedEnemyY);
                camera.transform.GetChild(0).GetChild(0).LookAt(fixedEnemyX);
                direccionAtaques.transform.LookAt(fixedEnemyX);
            }

        }
        else if(enemyFix && Mathf.Abs(controller.rightStick.ReadValue().x) == 0)
        {
            canChangeFixed = true;

        }

        //if(!jumping)
        camera.transform.position = this.transform.position;
        //else
        //{
        //    camera.transform.position = new Vector3(this.transform.position.x, alturaAlEmpezarSaltar, this.transform.position.z);

        //}
        float rotationY = controller.rightStick.ReadValue().x;
        float rotationX = controller.rightStick.ReadValue().y;


        if(!enemyFix&& controller.rightStick.ReadValue().magnitude > 0.2f)
        StartCoroutine(rotate(rotationY, -rotationX));

        if(!walk && !jumping && !dash)
        {
            move = new Vector3();
        }
        if(running && energia <=0 && !attacking &&!jumping && !dash && !block)
        {
            if (!enemyFix)
                changeWalk();
            else
                WalkAllDir();

            running = false;
            anim.speed = 1;
        }

        if (controller.leftStick.ReadValue().magnitude < 0.25f)
        {
            if (walk)
            {
                walkVelAnimation = true;

                walk = false;
                lastDir = -1;

                if (!jumping && !attacking && !dash && !block)
                {

                    anim.CrossFadeInFixedTime("Idle", 0.1f);

                }

                running = false;
                anim.speed = 1;
            }


        }

        if (!walk && (controller.leftStick.ReadValue().magnitude > 0.25f) )
        {
            if (!jumping && !attacking && !dash && !enemyFix && !block)
                changeWalk();

                walk = true;
        }


      

        if (jumping)
        {
            anim.speed = 1;

            if (enemyFix)
            {
                if (running)
                {

                    camera.transform.GetChild(1).localPosition += new Vector3(controller.leftStick.ReadValue().normalized.x, 0, controller.leftStick.ReadValue().normalized.y);
                    this.transform.LookAt(camera.transform.GetChild(1).position);
                    camera.transform.GetChild(1).localPosition = new Vector3();
                    camera.transform.LookAt(fixedEnemyY);
                    camera.transform.GetChild(0).GetChild(0).LookAt(fixedEnemyX);
                    direccionAtaques.transform.LookAt(fixedEnemyX);


                }
                else
                {
                    this.transform.LookAt(fixedEnemyY);
                    camera.transform.LookAt(fixedEnemyY);
                    camera.transform.GetChild(0).GetChild(0).LookAt(fixedEnemyX);
                    direccionAtaques.transform.LookAt(fixedEnemyX);
                }

            }

            RaycastHit hit;

            if (Physics.Raycast(this.transform.GetChild(1).position, -Vector3.up, out hit) && Time.time-timeAtJump>0.2f)
            {

                if (hit.distance < tocarSuelo && enAire)
                {
                    enAire = false;

                    anim.CrossFadeInFixedTime("Fall",0.1f);


                    Invoke("SalirCaer",caer);
                }
                if (hit.distance < tocarSuelo && jumping)
                {
                    anim.speed = 0.75f;

                }

            }
        }
        if (((currentAttack.attacks[currentComboAttack].Pesado == 1 && controller.rightTrigger.IsPressed()) || (currentAttack.attacks[currentComboAttack].Pesado == 0 && controller.rightShoulder.IsPressed())) && !jumping && !attacking && !dash && energia > 0 && !block && (Time.time - delayLastAttack) > 0.5f)
        {
            this.GetComponent<Rigidbody>().drag = 10;

            timeAttack = Time.time;

            attacking = true;
            currentComboAttack = 0;

            for (int i = 0; i < attack.Length;i++)
            {
                if(attack[i].combo == currentCombo)
                {
                    currentAttack = attack[i];
                    break;
                }
            }


            currentAttack.attacks[currentComboAttack].attacking = true;
            Invoke("addAttackForce", currentAttack.attacks[currentComboAttack].delayForce);
            StartCoroutine(EfectoAtaque(currentAttack.attacks[currentComboAttack].effects, currentAttack.attacks[currentComboAttack].effectDelay, currentAttack.attacks[currentComboAttack].cuts, currentAttack.attacks[currentComboAttack].col));
            
            anim.speed = currentAttack.attacks[currentComboAttack].animationSpeed;
            anim.CrossFadeInFixedTime(currentAttack.attacks[currentComboAttack].name, 0.2f);

            attackStartTime = Time.time;
        }

        if(attacking)
        {
            move = new Vector3();
            if (currentAttack.attacks[currentComboAttack].attacking)
            {
                if(currentAttack.attacks[currentComboAttack].curvaDeVelocidad.length != 0)
                {
                    float a = Time.time - timeAttack;
                    anim.speed = currentAttack.attacks[currentComboAttack].curvaDeVelocidad.Evaluate(a);
                }



                if ((Time.time - attackStartTime) > currentAttack.attacks[currentComboAttack].ataque)
                {
                    if ((Time.time - attackStartTime) < currentAttack.attacks[currentComboAttack].delay + currentAttack.attacks[currentComboAttack].ataque && currentAttack.attacks.Length != currentComboAttack +1)
                    {
                        if (((currentAttack.attacks[currentComboAttack+1].Pesado == 1 && controller.rightTrigger.IsPressed()) || (currentAttack.attacks[currentComboAttack+1].Pesado == 0 && controller.rightShoulder.IsPressed())) && energia > 0)
                        {

                            currentComboAttack++;
                            Invoke("addAttackForce", currentAttack.attacks[currentComboAttack].delayForce);

                            //effects[0].transform.localRotation = new Quaternion();
                            //effects[0].transform.Rotate(new Vector3(0, 0, rotation2));
                            StartCoroutine(EfectoAtaque(currentAttack.attacks[currentComboAttack].effects, currentAttack.attacks[currentComboAttack].effectDelay, currentAttack.attacks[currentComboAttack].cuts, currentAttack.attacks[currentComboAttack].col));
                            anim.speed = currentAttack.attacks[currentComboAttack].animationSpeed;
                            anim.CrossFadeInFixedTime(currentAttack.attacks[currentComboAttack].name, currentAttack.attacks[currentComboAttack-1].transition);


                            attackStartTime = Time.time;
                            currentAttack.attacks[currentComboAttack-1].attacking = false;
                            currentAttack.attacks[currentComboAttack].attacking = true;

                        }
                    }
                    else
                    {
                        delayLastAttack = Time.time;

						currentAttack.attacks[currentComboAttack].attacking = false;
                        attacking = false;
                        anim.speed = 1;
                        returnNormal();
                        this.GetComponent<Rigidbody>().drag = 20;

                        currentComboAttack = 0;

                    }
                }
            }
        }
        RaycastHit hit1;

        if (Physics.Raycast(this.transform.GetChild(1).position, -Vector3.up, out hit1))
        {

            if (!jumping)
            {
                if (hit1.distance > tocarSuelo && !enAire)
                {
                    this.GetComponent<Rigidbody>().drag = 5;
                    move *= 0.2f;

                    enAire = true;
                    jumping = true;
                    anim.CrossFadeInFixedTime("Jump", 0.2f);

                }
            }


            if (hit1.distance > 0.1f)
            {
                this.transform.GetComponent<Rigidbody>().AddForce(-transform.up * gravity * Time.fixedDeltaTime, ForceMode.Force);
            }
        }


        this.transform.GetComponent<Rigidbody>().AddForce(move,ForceMode.Force);
        
        //this.transform.position = this.transform.GetChild(0).GetChild(0).position;
        //this.transform.GetChild(0).GetChild(0).localPosition = new Vector3(0, 0, 0);
        lastMoveValue = controller.leftStick.ReadValue().magnitude;

    }


    void returnNormal()
    {
        if (controller.leftStick.ReadValue().magnitude < 0.25f)
        {
            anim.CrossFadeInFixedTime("Idle", 0.2f);       
            walk = false;
            run = false;
        }
        else if (controller.leftStick.ReadValue().magnitude > 0.25f && !running)
        {
            if (!enemyFix)
            {
                if (controller.leftStick.ReadValue().magnitude < 0.5f)
                {
                    walkVelAnimation = true;

                }
                else if (controller.leftStick.ReadValue().magnitude >= 0.5f)
                {
                    walkVelAnimation = false;

                }
                changeWalk();
            }            
            else
                WalkAllDir();
            run = false;
            walk = true;
        }
        else if (controller.leftStick.ReadValue().magnitude > 0.25f && running)
        {
                anim.CrossFadeInFixedTime("Run", 0.2f);

            run = true;
            walk = true;
        }
    }
    void addAttackForce()
    {
        this.GetComponent<Rigidbody>().AddForce(this.transform.forward * Time.fixedDeltaTime * currentAttack.attacks[currentComboAttack].force, ForceMode.Impulse);

    }
    void SalirDash()
    {
        this.GetComponent<Rigidbody>().drag = 20;

        returnNormal();

        dash = false;

        move = new Vector3();
    }
    void SalirCaer()
    {
        this.GetComponent<Rigidbody>().drag = 20;

        anim.speed = 1;
        returnNormal();
        jumping = false;

        move = new Vector3();

    }
    void SalirAttack()
    {
        attacking = false;
        this.GetComponent<Rigidbody>().drag = 20;

        returnNormal();


    }
}
