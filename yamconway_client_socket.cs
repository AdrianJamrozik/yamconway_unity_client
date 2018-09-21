using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;

public class yamconway_client_socket : MonoBehaviour {
	public string server_address = "localhost";
	public uint server_port = 5002;
	public float secondsBetweenUpdates = 1.0f;
	public GameObject cellGO = null;
	public uint rows = 20;
	public uint columns = 20;
	private float lastUpdate = 0.0f;
	private GameObject[] boardGOs = null;
	private SynchronousSocketClient ssc = null;
	private StreamReader reader = null;

	private void InitFlatBoard ()
	{
		boardGOs = new GameObject[rows * columns];
		Debug.Log ("board created");
		for (uint rowIndex = 0; rowIndex < rows; rowIndex++) {
			for (uint cellIndex = 0; cellIndex < columns; cellIndex++) {
				GameObject go = (GameObject)Instantiate (cellGO, new Vector3 (transform.position.x + cellIndex, transform.position.y + rowIndex), new Quaternion ());
				go.SetActive (false);
				boardGOs [(rowIndex * columns) + cellIndex] = go;
			}
		}
	}

	// Use this for initialization
	void Start () {
		ssc = new SynchronousSocketClient ();
		ssc.Initialize ();
		ssc.InitSimulation (rows, columns);
		InitFlatBoard ();
	}

	private void getNextTurn(){
		this.reader = new StreamReader(new System.IO.MemoryStream(ssc.NextTurn()));
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
			this.getNextTurn();
			this.updateBoardFast ();
			this.lastUpdate = Time.time;
		}
	}
}

public class SynchronousSocketClient {  
	public byte[] bytes = new byte[262144];  
	// Establish the remote endpoint for the socket.  
	// This example uses port 5002 on the local computer.  
	IPHostEntry ipHostInfo = null;
	IPAddress ipAddress = null;
	IPEndPoint remoteEP = null;

	Socket sender = null;

	public void Initialize(){
		ipHostInfo = Dns.GetHostEntry("localhost");
		foreach (IPAddress ip in ipHostInfo.AddressList)
		{
			Debug.Log("---"+ip);
		}
		ipAddress = ipHostInfo.AddressList[1];
		remoteEP = new IPEndPoint(ipAddress,5002);
		try{
			sender = new Socket (this.ipAddress.AddressFamily,
				SocketType.Stream, ProtocolType.Tcp);
			sender.Connect(remoteEP);  
			Debug.Log("Socket connected to localhost");
		}
		catch (Exception e) {  
			Debug.Log( e.ToString());
		}
	}

	public void InitSimulation(uint rows, uint columns){
		byte[] msg = Encoding.ASCII.GetBytes ("Init"+rows.ToString()+"x"+columns.ToString());
		int bytesSent = sender.Send(msg);
	}
		
	public byte[] NextTurn(){
		// Encode the data string into a byte array.  
		byte[] msg = Encoding.ASCII.GetBytes("NextTurn");  
		// Send the data through the socket.
		int bytesSent = sender.Send(msg);
		// Receive the response from the remote device.  
		int bytesRec = sender.Receive(bytes);
		return bytes;
	}
		
	public void Destroy(){
		sender.Shutdown(SocketShutdown.Both);  
		sender.Close();  
	}
		
}