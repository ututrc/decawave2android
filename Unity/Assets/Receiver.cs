/*
 * Created by Mika Taskinen on 30.1.2017
 * Copyright: University of Turku & Mika Taskinen
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Marin2.Decawave.Unity3d
{
    public class Receiver
    {

        public delegate void DisconnectedEventHandler( Receiver receiver);
        /// <summary>
        /// This event lauches when receiver is disconnected
        /// </summary>
        public event DisconnectedEventHandler Disconnected;

        public delegate void AnchorAppearedEventHandler( Receiver receiver, Anchor anchor );
        /// <summary>
        /// This event lauches whenever a new anchor is detected
        /// </summary>
        public event AnchorAppearedEventHandler AnchorAppeared;

        private Dictionary<int, Anchor> anchors = new Dictionary<int, Anchor>();


        protected void OnDisconnected()
        {
            DisconnectedEventHandler handler = Disconnected;
            if ( handler != null )
                handler( this );
        }

        protected void OnAnchorAppeared( Anchor anchor )
        {
            AnchorAppearedEventHandler handler = AnchorAppeared;
            if ( handler != null )
                handler( this, anchor );
        }
        
        internal Receiver( DecawaveManager manager, string serial )
        {
            Manager = manager;
            Serial = serial;
        }

        /// <summary>
        /// Get the serial number of the receiver
        /// </summary>
        public string Serial
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the manager that handles the receiver
        /// </summary>
        public DecawaveManager Manager
        {
            get;
            private set;
        }
        /// <summary>
        /// Get the information of the receiver status
        /// </summary>
        public bool IsDisconnected
        {
            get;
            private set;
        }

        internal void Disconnect()
        {
            OnDisconnected();
            IsDisconnected = true;
        }

        internal void Update( AndroidJavaObject parser )
        {
            // This date is used for removing anchors that are not up-to-date
            DateTime now = DateTime.Now;
            // This loop is done as long as the device has packets
            while ( parser.Call<bool>( "hasPacket" ) )
            {
                // Getting the packet
                AndroidJavaObject packet = parser.Call<AndroidJavaObject>( "popPacket" );
                // Taking the information
                int id = packet.Call<int>( "getAnchorId" );
                int distance = packet.Call<int>( "getDistanceInMillimeters" );

                // Anchor creation/get
                Anchor anchor;
                if ( anchors.ContainsKey( id ) )
                {
                    anchor = anchors[id];
                }
                else
                {
                    anchor = new Anchor( this, id );
                    anchors.Add( id, anchor );
                    // On new anchor -> inform
                    OnAnchorAppeared( anchor );
                }
                // Setting data
                anchor.Set( now, distance );

            }

            // Remove all anchors that are invalid
            HashSet<int> removables = new HashSet<int>();
            foreach ( int anchorId in anchors.Keys )
            {
                if ( ( now - anchors[anchorId].Timestamp ) > Manager.DiscardInterval )
                {
                    anchors[anchorId].Remove();
                    anchors.Remove( anchorId );
                }
            }
        }
    }
}