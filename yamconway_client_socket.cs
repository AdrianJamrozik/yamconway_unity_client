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

	// Use this for initialization
	void Start () {
		ssc = new SynchronousSocketClient ();
		ssc.Initialize ();
		Debug.Log("ssc initialized");
		//ssc.StartClient ();
		boardGOs = new GameObject[rows * columns];
		Debug.Log("board created");
		for (uint rowIndex = 0; rowIndex < rows; rowIndex++)
		{
			for (uint cellIndex = 0; cellIndex < columns; cellIndex++)
			{
				GameObject go = (GameObject)Instantiate (cellGO,
					new Vector3 (transform.position.x + cellIndex, transform.position.y + rowIndex),
					new Quaternion ());
				go.SetActive (false);
				boardGOs [(rowIndex * columns) + cellIndex] = go;
				Debug.Log("created_go");
			}
		}
	}

	private void getNextTurn(){
		Debug.Log("calling next turn");
		this.reader = new StreamReader(new System.IO.MemoryStream(ssc.NextTurn()));
		Debug.Log("returned with reader");
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
			Debug.Log("Update 1");
			this.getNextTurn();
			Debug.Log("Update 2");
			this.updateBoardFast ();
			Debug.Log("Update 3");
			this.lastUpdate = Time.time;
		}
	}
}


public class SynchronousSocketClient {  
	public byte[] bytes = new byte[1024];  
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
			Debug.Log("test1");
			sender = new Socket (this.ipAddress.AddressFamily,
				SocketType.Stream, ProtocolType.Tcp);
			Debug.Log("test2");
			sender.Connect(remoteEP);  
			Debug.Log("Socket connected to localhost");
		}
		catch (Exception e) {  
			Debug.Log( e.ToString());
		}
	}



	public byte[] NextTurn(){
		Debug.Log("NextTurn 1");
		// Encode the data string into a byte array.  
		byte[] msg = Encoding.ASCII.GetBytes("NextTurn");  
		Debug.Log("NextTurn 2");
		// Send the data through the socket.
		int bytesSent = sender.Send(msg);
		Debug.Log("NextTurn 3");
		// Receive the response from the remote device.  
		int bytesRec = sender.Receive(bytes);
		Debug.Log("NextTurn 4");
		Debug.Log("Echoed test = " + Encoding.ASCII.GetString(bytes,0,bytesRec)); 

		return bytes;
	}



	public void Destroy(){
		sender.Shutdown(SocketShutdown.Both);  
		sender.Close();  
	}

//	public void StartClient() {
//		// Connect to a remote device.  
//		try {  
//			// Connect the socket to the remote endpoint. Catch any errors.  
//			try {
//				// Release the socket.  
//				sender.Shutdown(SocketShutdown.Both);  
//				sender.Close();  
//
//			} catch (ArgumentNullException ane) {  
//				Debug.Log("ArgumentNullException : " + ane.ToString());  
//			} catch (SocketException se) {  
//				Debug.Log("SocketException :" + se.ToString());  
//			} catch (Exception e) {  
//				Debug.Log("Unexpected exception :" + e.ToString());  
//			}  
//
//		} catch (Exception e) {  
//			Debug.Log( e.ToString());  
//		}  
//	}  
		
}