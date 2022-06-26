using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

	public GameManager gameManager;
	public AudioClip audioJump;
	public AudioClip audioAttack;
	public AudioClip audioDamaged;
	public AudioClip audioItem;
	public AudioClip audioDie;
	public AudioClip audioFinish;

	public float maxSpeed;
	public float jumpPower;
	Rigidbody2D rigid;
	SpriteRenderer spriteRenderer;
	Animator animator;
	CapsuleCollider2D capsuleCollider;
	AudioSource audioSource;


	void Awake () 
	{
		rigid = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		animator = GetComponent<Animator>();
		capsuleCollider = GetComponent<CapsuleCollider2D>();
		audioSource = GetComponent<AudioSource>();
	}
	
	void Update () //단발적인 키업데이트는 update에서 하는게 좋다. 물리라고해도
				   //단발적인업데이트를 fixedupdate에서 하면 가끔씩 키씹힘이 생긴다.
	{ 
		//Jump
		//if(Input.GetButtonDown("Jump") && animator.GetBool("isJumping") == false) 둘다같은상황
		if (Input.GetButtonDown("Jump") && !animator.GetBool("isJumping"))
			                               //isJumping이 bool값이므로 true false만 있음
										   //false란 소리는 점프안하고있다는말
		{
			rigid.AddForce(Vector3.up * jumpPower, ForceMode2D.Impulse);
			animator.SetBool("isJumping", true);
			PlaySound("JUMP");
		}


		//Stop Speed
		if(Input.GetButtonUp("Horizontal"))
		{
			rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
			//normalized : 벡터 크기를 1로 만든 상태(단위벡터) -1, 0 1 
			//소수점을 곱할때에는 f(float)를 꼭 붙여야한다.
		}

		//Direction Sprite
		if(Input.GetButton("Horizontal")) //GetButtonDown을 해도되지만 가끔씹 씹히거나 오류생김
			spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
		//flipX는 bool값이고 초기값은 false이다.

		//Animation
		if (Mathf.Abs(rigid.velocity.x) < 0.3)
			//Mathf는 수학 관련 함수를 제공하는 클래스 abs : 절댓값
			animator.SetBool("isWalking", false); //Parameters에 넣은게 bool이므로 setbool을 사용
		else
			animator.SetBool("isWalking", true);
	}
	
	void FixedUpdate () //지속적인 키업데이트는 fixedupdate
	{

		//Move Speed
		float h = Input.GetAxisRaw("Horizontal");

		rigid.AddForce(Vector2.right * h * 2, ForceMode2D.Impulse);
		//right로 한 이유는 Horizontal기본설정이 positive buttoon이 오른쪽이고
		//negative button이 왼쪽이기 때문이다.
		// * 2를 한 이유는 오르막길을 올라가는 힘이 부족해서 그냥 그 힘을 늘려준건다.

		//Max Speed
		if(rigid.velocity.x > maxSpeed) //Right Max Speed
		{
			rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
		}
		else if(rigid.velocity.x < maxSpeed * (-1)) //Left Max Speed
		{
			rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
		}

		//Landing Platform
		if(rigid.velocity.y < 0) //y축의 속도를 보고 내려가는 속도일때만 Ray를 쏜다.
		{
			Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));

			RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1,
													LayerMask.GetMask("Platform"));
			//RayCastHit는 물리기반이다.
			if (rayHit.collider != null)
			{
				if (rayHit.distance < 0.5f)
					//Debug.Log(rayHit.collider.name);
					animator.SetBool("isJumping", false);
				//Ray를 쏴서 collider이랑 만나면서 그 거리가 0.5보다 작으면 점프애니메이션을 끈다.
			}
		}
		
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.gameObject.tag == "Enemy")
		{
			//Attack  몬스터보다 위에 있고, 아래로 낙하중일때
			if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
			{
				OnAttack(collision.transform);
			}
			//Damaged
			else
			{
				OnDamaged(collision.transform.position);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if(collision.gameObject.tag == "Item")
		{
			//Point
			bool isBroze = collision.gameObject.name.Contains("Broze");
			//Contains(비교문) : 대상 문자열에 비교문이 있으면 true
			bool isSilver = collision.gameObject.name.Contains("Silver");
			bool isGold = collision.gameObject.name.Contains("Gold");

			if (isBroze)
				gameManager.stagePoint = gameManager.stagePoint + 50;
			else if (isSilver)
				gameManager.stagePoint = gameManager.stagePoint + 100;
			else if (isGold)
				gameManager.stagePoint = gameManager.stagePoint + 300;
			//Deactive Item
			collision.gameObject.SetActive(false);

			//Sound
			PlaySound("ITEM");
		}
		else if(collision.gameObject.tag == "Finish")
		{
			//Next Stage
			gameManager.NextStage();
			//Sound
			PlaySound("FINISH");
		}
	}

	void OnAttack(Transform enemy)
	{
		//Point
		gameManager.stagePoint = gameManager.stagePoint + 100;
		//Reaction Force
		rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
		//Enemy Die
		EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
		enemyMove.OnDamaged(); 
		PlaySound("ATTACK");
	}

	void OnDamaged(Vector2 targetPosition) //무적효과함수 생성
	{
		//Health Down
		gameManager.HealthDown();
		//Change Layer 충돌시 레이어변경
		gameObject.layer = 11;//숫자는 layer의 숫자이다.

		//View Alpha 충돌시 시각적으로 보이게 설정
		spriteRenderer.color = new Color(1, 1, 1, 0.4f);

		//Reaction Force
		int dirc = transform.position.x - targetPosition.x > 0 ? 1 : -1;
		rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);

		//Animation
		animator.SetTrigger("isDamaged");

		Invoke("OffDamaged", 3);

		PlaySound("DAMAGED");
	}

	void OffDamaged() //무적해제함수 생성
	{
		gameObject.layer = 10;
		spriteRenderer.color = new Color(1, 1, 1, 1);
	}


	public void OnDie()
	{
		//Sprite Alpha
		spriteRenderer.color = new Color(1, 1, 1, 0.4f);
		//Sprite Flip Y
		spriteRenderer.flipY = true;
		//Collider Disable
		capsuleCollider.enabled = false;
		//Die Effect Jump
		rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
		PlaySound("DIE");
	}

	public void VelocityZero()
	{
		rigid.velocity = Vector2.zero;
	}
	void PlaySound(string action)
	{
		switch (action)
		{
			case "JUMP":
				audioSource.clip = audioJump;
				break;
			case "ATTACK":
				audioSource.clip = audioAttack;
				break;
			case "DAMAMGED":
				audioSource.clip = audioDamaged;
				break;
			case "ITEM":
				audioSource.clip = audioItem;
				break;
			case "DIE":
				audioSource.clip = audioDie;
				break;
			case "FINISH":
				audioSource.clip = audioFinish;
				break;
		}
		audioSource.Play();
	}
}
