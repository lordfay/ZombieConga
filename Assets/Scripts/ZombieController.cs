using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieController : MonoBehaviour {

	public float moveSpeed;
	public float turnSpeed;

	private Vector3 moveDirection;

	[SerializeField]
	private PolygonCollider2D[] colliders;
	private int currentColliderIndex = 0;

	private List<Transform> congaLine = new List<Transform>();

	private bool isInvincible = false;
	private float timeSpentInvincible;

	private int lives = 3;

	public AudioClip enemyContactSound;
	public AudioClip catContactSound;
	private int score = 0;
	private int level = 1;
	public Texture2D livesIconTexture1;
	public Texture2D livesIconTexture2;
	public Texture2D livesIconTexture3;




	// Use this for initialization
	void Start () {
		moveDirection = Vector3.right;

	}



	// Update is called once per frame
	void Update () {



		// 1
		Vector3 currentPosition = transform.position;
		// 2
		if( Input.GetButton("Fire1") ) {
			// 3
			Vector3 moveToward = Camera.main.ScreenToWorldPoint( Input.mousePosition );
			// 4
			moveDirection = moveToward - currentPosition;
			moveDirection.z = 0; 
			moveDirection.Normalize();
		}

		Vector3 target = moveDirection * moveSpeed + currentPosition;
		transform.position = Vector3.Lerp( currentPosition, target, Time.deltaTime );

		float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
		transform.rotation = 
			Quaternion.Slerp( transform.rotation, 
			                 Quaternion.Euler( 0, 0, targetAngle ), 
			                 turnSpeed * Time.deltaTime );

		EnforceBounds();

		//1
		if (isInvincible)
		{
			//2
			timeSpentInvincible += Time.deltaTime;
			
			//3
			if (timeSpentInvincible < 3f) {
				float remainder = timeSpentInvincible % .3f;
				GetComponent<Renderer>().enabled = remainder > .15f; 
			}
			//4
			else {
				GetComponent<Renderer>().enabled = true;
				isInvincible = false;
			}
		}
	}

	public void SetColliderForSprite( int spriteNum )
	{
		colliders[currentColliderIndex].enabled = false;
		currentColliderIndex = spriteNum;
		colliders[currentColliderIndex].enabled = true;
	}

	void OnTriggerEnter2D( Collider2D other )
	{
		if(other.CompareTag("cat")) {
			GetComponent<AudioSource>().PlayOneShot(catContactSound);
			score++;

			Transform followTarget = congaLine.Count == 0 ? transform : congaLine[congaLine.Count-1];
			other.transform.parent.GetComponent<CatController>().JoinConga( followTarget, moveSpeed, turnSpeed );

			congaLine.Add( other.transform );

			if (congaLine.Count >= 5) {
				Application.LoadLevel("CongaScene2");
			}
		}
		else if(!isInvincible && other.CompareTag("enemy")) {
			GetComponent<AudioSource>().PlayOneShot(enemyContactSound);

			isInvincible = true;
			timeSpentInvincible = 0;

			for( int i = 0; i < 2 && congaLine.Count > 0; i++ )
			{
				int lastIdx = congaLine.Count-1;
				Transform cat = congaLine[ lastIdx ];
				congaLine.RemoveAt(lastIdx);
				cat.parent.GetComponent<CatController>().ExitConga();
				score--;
			}

			if (--lives <= 0) {
				Application.LoadLevel("LoseScene");
			}
		}
	}


	void OnGUI()
	{
		DisplayScoreCount();
		DisplayLivesCount();

		
	}
	
	void DisplayScoreCount()
	{
		
		
		GUIStyle style = new GUIStyle();
	
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.black;
		
		Rect labelRects = new Rect(10 , 10, 50  , 50);

		GUI.Label(labelRects, "Score" , style);

		Rect labelRect = new Rect(60 , 10, 100 , 50);
		string s = "" + score;
		GUI.Label(labelRect, s, style);

		Rect labelRectss = new Rect(10 , 30, 50  , 80);
		
		GUI.Label(labelRectss, "Level 1" , style);

	}

	void DisplayLivesCount()
	{
		
		switch (lives) {
		case 3: 
			Rect livesIconRect31 = new Rect (10 , 50, Screen.width/20 , Screen.width/20);
			GUI.DrawTexture (livesIconRect31, livesIconTexture1);   
			Rect livesIconRect32 = new Rect (60 , 50, Screen.width/20 , Screen.width/20);
			GUI.DrawTexture (livesIconRect32, livesIconTexture1);   
			Rect livesIconRect33 = new Rect (110 , 50, Screen.width/20 , Screen.width/20);
			GUI.DrawTexture (livesIconRect33, livesIconTexture1);



			break;
		case 2:
			Rect livesIconRect21 = new Rect (10 , 50, Screen.width/20 , Screen.width/20);
			GUI.DrawTexture (livesIconRect21, livesIconTexture1);   
			Rect livesIconRect22 = new Rect (60 , 50, Screen.width/20 , Screen.width/20);
			GUI.DrawTexture (livesIconRect22, livesIconTexture1);      
			break;
		case 1:
			Rect livesIconRect1 = new Rect (10 , 50, Screen.width/20 , Screen.width/20);
			GUI.DrawTexture (livesIconRect1, livesIconTexture1); 
			break;

		}
	}

	private void EnforceBounds()
	{
		// 1
		Vector3 newPosition = transform.position; 
		Camera mainCamera = Camera.main;
		Vector3 cameraPosition = mainCamera.transform.position;
		
		// 2
		float xDist = mainCamera.aspect * mainCamera.orthographicSize; 
		float xMax = cameraPosition.x + xDist;
		float xMin = cameraPosition.x - xDist;
		
		// 3
		if ( newPosition.x < xMin || newPosition.x > xMax ) {
			newPosition.x = Mathf.Clamp( newPosition.x, xMin, xMax );
			moveDirection.x = -moveDirection.x;
		}
		// TODO vertical bounds
		float yMax = mainCamera.orthographicSize;
		
		if (newPosition.y < -yMax || newPosition.y > yMax) {
			newPosition.y = Mathf.Clamp( newPosition.y, -yMax, yMax );
			moveDirection.y = -moveDirection.y;
		}

		// 4
		transform.position = newPosition;
	}
}
