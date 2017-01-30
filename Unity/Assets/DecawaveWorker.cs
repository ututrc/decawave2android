/*
 * Created by Mika Taskinen on 30.1.2017
 * Copyright: University of Turku & Mika Taskinen
 * 
 * THIS IS AN EXAMPLE MONOBEHAVIOR CLASS To WHO HOW TO USE THE LIBRARY
 */

using Marin2.Decawave.Unity3d;
using Marin2.Trilinear;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DecawaveWorker : MonoBehaviour {
    
    private string dataString;
    private DecawaveManager manager;

    private Dictionary<string, Dictionary<int, int>> values = new Dictionary<string, Dictionary<int, int>>();

    private static readonly Vector3 anchor0Pos = new Vector3( 4.313f, 0.95f, 0.183f );
    private static readonly Vector3 anchor1Pos = new Vector3( .14f, .955f, .28f );
    private static readonly Vector3 anchor2Pos = new Vector3( .135f, .94f, 6.718f );

    // Use this for initialization
    void Start () {

        // Initialize decawave system and attach event on receiver discovery
        manager = DecawaveManager.Instance;
        manager.ReceiverAppeared += Manager_ReceiverAppeared;
    }

    private void Manager_ReceiverAppeared( DecawaveManager manager, Receiver beacon )
    {
        // Store new device
        values.Add( beacon.Serial, new Dictionary<int, int>() );
        // Attach event for disappearance and for new anchors
        beacon.AnchorAppeared += Receiver_AnchorAppeared;
        beacon.Disconnected += Receiver_Disconnected;
    }

    private void Receiver_Disconnected( Receiver beacon )
    {
        // Remove device
        values.Remove( beacon.Serial );
    }

    private void Receiver_AnchorAppeared( Receiver beacon, Anchor anchor )
    {
        // Add new anchor
        values[beacon.Serial].Add( anchor.Id, anchor.Distance );
        // Attach anchor related events
        anchor.Updated += Anchor_Updated;
        anchor.Disappeared += Anchor_Disappeared;
    }

    private void Anchor_Disappeared( Anchor anchor )
    {
        // Remove anchor
        values[anchor.Receiver.Serial].Remove( anchor.Id );
    }

    private void Anchor_Updated( Anchor anchor, int newDistance, int oldDistance )
    {
        // Update anchor value
        values[anchor.Receiver.Serial][anchor.Id] = newDistance;
    }

    // Update is called once per frame
    void Update ()
    {
        // Update manager to actively keep track of data (Use this as often as you want, but remember: The data will not disappear until popped by update)
        manager.Update();

        // Example string build for data
        StringBuilder builder = new StringBuilder();
        foreach ( KeyValuePair<string, Dictionary<int, int>> beacon in values )
        {
            builder.AppendLine( "BEACON: " + beacon.Key );
            foreach ( KeyValuePair<int, int> anchor in beacon.Value )
            {
                switch ( anchor.Key )
                {
                    case 0:
                        builder.AppendLine( "\tANCHOR-" + anchor.Key + ": " + anchor.Value + " " + VectorString( anchor0Pos ) );
                        break;
                    case 1:
                        builder.AppendLine( "\tANCHOR-" + anchor.Key + ": " + anchor.Value + " " + VectorString( anchor1Pos ) );
                        break;
                    case 2:
                        builder.AppendLine( "\tANCHOR-" + anchor.Key + ": " + anchor.Value + " " + VectorString( anchor2Pos ) );
                        break;
                    default:
                        builder.AppendLine( "\tANCHOR-" + anchor.Key + ": " + anchor.Value );
                        break;
                }
            }
            if ( beacon.Value.ContainsKey( 0 ) && beacon.Value.ContainsKey( 1 ) && beacon.Value.ContainsKey( 2 ) )
            {
                Vector3 result1, result2;
                if ( TrilinearCalculations.ThreePoint( anchor0Pos, anchor1Pos, anchor2Pos, beacon.Value[0] * .001f, beacon.Value[1] * .001f, beacon.Value[2] * .001f, out result1, out result2 ) )
                {
                    builder.AppendLine( "\tRESULTS:" );
                    builder.AppendLine( "\t\t" + VectorString( result1 ) );
                    builder.AppendLine( "\t\t" + VectorString( result2 ) );
                }
            }
        }

        // build to string
        dataString = builder.ToString();

    }

    void OnGUI()
    {
        // Print the data on GUI
        GUI.TextArea( new Rect( 10, 10, Screen.width - 20, Screen.height - 20 ), dataString );
    }

    /// <summary>
    /// Vectors in custom string format
    /// </summary>
    /// <param name="v">Given vector</param>
    /// <returns>String format of vector</returns>
    private string VectorString( Vector3 v )
    {
        return "[ " + v.x + ", " + v.y + ", " + v.z + " ]";
    }
}
