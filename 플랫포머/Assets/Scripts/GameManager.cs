using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour 
{   //매니저는 점수와 스테이지를 관리

	public int totalPoint;
	public int stagePoint;
	public int stageIndex = 0;
	public int health;
	public PlayerMove player;
	public GameObject[] Stages;

	public Image[] UIhealth;
	public Text UIPoint;
	public Text UIStage;
	public GameObject UIRestartBtn;
	
	void Update()
	{
		UIPoint.text = (totalPoint + stagePoint).ToString();
	}
	public void NextStage () 
	{
		//Change Stage
		if (stageIndex < Stages.Length - 1)
		{
			Stages[stageIndex].SetActive(false);
			stageIndex++;
			Stages[stageIndex].SetActive(true);
			PlayerReposition();

			UIStage.text = "STAGE " + (stageIndex + 1);
		}
		else
		{//Game Clear
		 //Player Contol Lock
			Time.timeScale = 0; //timeScale = 0으로 시간을 멈춰둠.

			//Result UI
			Debug.Log("게임 클리어!");
			//Restart Button UI
			Text btnText = UIRestartBtn.GetComponentInChildren<Text>();
			//버튼 텍스트는 자식오브젝트이므로 InChildren을 더 붙여야한다.
			btnText.text = "Clear!";
			UIRestartBtn.SetActive(true);
		}

		//Calculate Point
		totalPoint = totalPoint + stagePoint;

		stagePoint = 0;
	}

	public void HealthDown()
	{
		if (health > 1)
		{ 
			health--;
			UIhealth[health].color = new Color(1, 0, 0, 0.4f);
		}
		else
		{
			//All Health UI Off
			UIhealth[0].color = new Color(1, 0, 0, 0.4f);

			//Player Die Effect
			player.OnDie();
			//Result UI
			Debug.Log("쥬금");
			//Retry Buttion UI
			UIRestartBtn.SetActive(true);
		}
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			//Player Reposition
			if (health > 1)
			{
				PlayerReposition();
			}

			//Health Down
			HealthDown();
		}
	}
	void PlayerReposition()
	{
		player.transform.position = new Vector3(-6, 4, 0);
		player.VelocityZero();

	}
	public void Restart()
	{
		Time.timeScale = 1;
		SceneManager.LoadScene(0);
	}
}
