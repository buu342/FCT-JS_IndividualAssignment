/****************************************************************
                       VisualOptimizer.cs
    
This script handles the culling of drawing rooms to improve 
framerate.
****************************************************************/

#define OPTIMIZE_ROOMS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VisualOptimizer : MonoBehaviour
{
    
    public ProcGenner m_ProcGen;
    private GameObject m_Player;
    

    /*==============================
        Update
        Called every frame
    ==============================*/
    
    void Update()
    {
        #if OPTIMIZE_ROOMS
            // Ideally this should be done with triggers but this will do for now
            if (this.m_Player != null)
            {
                List<ProcGenner.RoomDef> rooms = this.m_ProcGen.GetRoomDefs();
                for (int i=0; i<rooms.Count; i++)
                {
                    ProcGenner.RoomDef room = rooms[i];
                    bool visible = PlayerCanSeeRoom(room);
                    if (visible != room.visible)
                    {
                        foreach (GameObject obj in room.objects)
                        {
                            if (obj.GetComponent<SkinnedMeshRenderer>() != null)
                                obj.GetComponent<SkinnedMeshRenderer>().enabled = visible;
                            if (obj.GetComponent<MeshRenderer>() != null)
                                obj.GetComponent<MeshRenderer>().enabled = visible;
                        }
                        this.m_ProcGen.SetRoomVisible(i, visible);
                    }
                }
            }
        #endif
    }


    /*==============================
        SetPlayer
        Sets the player object
        @param The player object
    ==============================*/
    
    public void SetPlayer(GameObject ply)
    {
        this.m_Player = ply;
    }


    /*==============================
        PlayerCanSeeRoom
        Checks if the player can see inside the room
        @param Whether the inside of the room is visible
    ==============================*/
    
    bool PlayerCanSeeRoom(ProcGenner.RoomDef room)
    {
        // Check if the player is in bounds of the room
        Vector3 realroomstart = room.midpoint;
        Vector3 realroomsize = ((Vector3)room.size)*ProcGenner.GridScale/2;
        Vector3 playerpos = this.m_Player.transform.position;
        if (playerpos.x >= realroomstart.x-realroomsize.x && playerpos.x <= realroomstart.x+realroomsize.x &&
            playerpos.y >= realroomstart.y-realroomsize.y && playerpos.y <= realroomstart.y+realroomsize.y &&
            playerpos.z >= realroomstart.z-realroomsize.z && playerpos.z <= realroomstart.z+realroomsize.z
        )
            return true;
            
        // Check if a door is open
        foreach (GameObject door in room.doors)
            if (door.GetComponent<DoorLogic>() != null && door.GetComponent<DoorLogic>().IsDoorOpen())
                return true;
            
        // No reason to draw the room then
        return false;
    }
}