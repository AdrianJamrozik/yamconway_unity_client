using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.IO;

public class yamconway_client : MonoBehaviour {
	public float secondsBetweenUpdates = 1.0f;
	public GameObject cellGO = null;
	public uint rows = 20;
	public uint columns = 20;
	private float lastUpdate = 0.0f;
	private HttpWebRequest request = null;
	private HttpWebResponse response = null;
	private StreamReader reader = null;
	private GameObject[] boardGOs = null;

	// Use this for initialization
	void Start () {
		boardGOs = new GameObject[rows * columns];
		for (uint rowIndex = 0; rowIndex < rows; rowIndex++)
		{
			for (uint cellIndex = 0; cellIndex < columns; cellIndex++)
			{
				GameObject go = (GameObject)Instantiate (cellGO,
					new Vector3 (transform.position.x + cellIndex, transform.position.y + rowIndex),
					new Quaternion ());
				go.SetActive (false);
				boardGOs [(rowIndex * columns) + cellIndex] = go;
			}
		}
	}


	private void getNextTurn(){
		this.request = (HttpWebRequest)WebRequest.Create("http://localhost:5002/nextturn");
		this.response = (HttpWebResponse)request.GetResponse();
		this.reader = new StreamReader (response.GetResponseStream ());
		//this.reader = new StreamReader(new System.IO.MemoryStream(new byte[]{21,22,23}));
	}
		

	private void updateBoardFast(){
		this.reader.Read ();
		foreach (GameObject go in this.boardGOs) {
			if (reader.Read () == '1') {
				go.SetActive (true);
			} else {
				go.SetActive (false);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (Time.time - this.lastUpdate > secondsBetweenUpdates) {
			//this.getBoardFromServer ();
			this.getNextTurn();
			this.updateBoardFast ();
			this.lastUpdate = Time.time;
		}
	}

}
