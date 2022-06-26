using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour {

	Rigidbody2D rigid;
	public int nextMove;  //행동지표를 결정할 변수 하나를 생성
	Animator animator;
	SpriteRenderer spriteRenderer;
	CapsuleCollider2D capsuleCollider;  //변수명으로 collider은 비추천

	void Awake () 
	{
		rigid = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		capsuleCollider = GetComponent<CapsuleCollider2D>();

		Invoke("Think", 5);
		//Invoke() : 주어진 시간이 지난 뒤, 지정된 함수를 실행하는 함수
	}
	
	
	void FixedUpdate () 
	{
		//Move
		rigid.velocity = new Vector2(nextMove, rigid.velocity.y); //y값은 0이아닌 현재속도의 y값을 넣어야한다.
																  //Think();  이렇게하면 fixedupdate특성상 1초에 50번정도를 불러오므로 과부화가걸린다.

		//Platform Check
		Vector2 frontVec = new Vector2(rigid.position.x + nextMove*0.2f, rigid.position.y);
		//몹의 앞쪽을 체크해야함으로 frontVec을 따로 두고, 가능방향의 일정거리앞이므로 nextMove사용
		Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));

		RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1,
												LayerMask.GetMask("Platform"));
		if (rayHit.collider == null)
		{
			Turn();
		}
	}

	void Think()
	{
		//Set Next Active
		nextMove = Random.Range(-1, 2);
		//Random : 랜덤 수를 생성하는 로직 관련 클래스
		//Range() : 최소~최대범위의 랜덤 수 생성 (최대제외)

		//Sprite Animation
		animator.SetInteger("WalkSpeed", nextMove); //Integer = int

		//Flip Sprite
		if(nextMove != 0)
			spriteRenderer.flipX = nextMove == 1;

		//Recursive(재귀함수) 보통 재귀함수는 맨아래에 위치한다.
		float nextThinkTime = Random.Range(2f, 5f);
		Invoke("Think", nextThinkTime);
		//Think(); 재귀함수 : 자신을 스스로 호출하는 함수  +  딜레이 없이 재귀함수를 사용하는것은 위험!
	}

	void Turn()
	{
		nextMove = nextMove * -1;
		spriteRenderer.flipX = nextMove == 1;

		CancelInvoke(); //CancelInvoke() : 현재 작동중인 모든 Invoke함수를 멈추는 함수
		Invoke("Think", 2);
	}

	public void OnDamaged()
	{
		//Sprite Alpha
		spriteRenderer.color = new Color(1, 1, 1, 0.4f);
		//Sprite Flip Y
		spriteRenderer.flipY = true;
		//Collider Disable
		capsuleCollider.enabled = false;
		//Die Effect Jump
		rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
		//Destory
		Invoke("DeActive", 5);
	}

	void DeActive()
	{
		gameObject.SetActive(false);
	}
}
