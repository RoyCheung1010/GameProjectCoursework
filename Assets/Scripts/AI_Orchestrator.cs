using UnityEngine;

public class AI_Orchestrator : MonoBehaviour
{
    [Header("Coordinated Agents")]
    public SecurityBotController[] allAgents;

    [Header("Level 2 Puzzle")]
    public PressurePlateController[] allPressurePlates; 
    public GameObject dataCoreDoor; 
    private bool isDoorOpen = false;

    [Header("Level 3 Exit")]
    public PressurePlateController[] level3Plates; 
    public GameObject finalExitPortal;
    

    public void CheckAllPlatesStatus()
    {
        if (allPressurePlates != null && allPressurePlates.Length > 0 && dataCoreDoor != null)
        {
            CheckLevel2AndGate();
        }

        if (level3Plates != null && level3Plates.Length > 0 && finalExitPortal != null)
        {
            CheckLevel3Activation();
        }
    }

    void CheckLevel2AndGate()
    {
        int activePlates = 0;
        
        foreach (PressurePlateController plate in allPressurePlates)
        {
            if (plate == null) continue;
            if (plate.IsActive) 
            {
                activePlates++;
            }
        }

        if (activePlates == allPressurePlates.Length)
        {
            OpenDataCoreDoor();
        }
        else
        {
            if (isDoorOpen)
            {
                CloseDataCoreDoor();
            }
        }
    }

    void CheckLevel3Activation()
    {
        int activePlates = 0;
        foreach (PressurePlateController plate in level3Plates)
        {
            if (plate == null) continue;
            if (plate.IsActive) activePlates++;
        }

        if (activePlates == level3Plates.Length)
        {
            finalExitPortal.SetActive(true);
            StopAllAgents();
            Debug.Log("LEVEL 3 SOLVED: Exit Portal Activated.");
        }
        else
        {
            finalExitPortal.SetActive(false);
        }
    }

    void OpenDataCoreDoor()
    {
        isDoorOpen = true;
        Debug.Log("LEVEL 2 PUZZLE SOLVED: Data Core Door Opening!");

        if (dataCoreDoor != null)
        {
            DoorMovementController mover = dataCoreDoor.GetComponent<DoorMovementController>();
            if (mover != null) mover.SetDoorState(true); 
        }
    }

    void CloseDataCoreDoor()
    {
        isDoorOpen = false;
        Debug.Log("Data Core Door Closing.");
        
        if (dataCoreDoor != null)
        {
            DoorMovementController mover = dataCoreDoor.GetComponent<DoorMovementController>();
            if (mover != null) mover.SetDoorState(false); 
        }
    }
    
    void StopAllAgents()
    {
        foreach (var botController in allAgents)
        {
            if (botController.Agent != null)
            {
                botController.Agent.isStopped = true;
            }
        }
    }
    
    public void AlertAllBots(SecurityBotController triggeringAgent)
    {
        Debug.Log($"Orchestrator ALERT: {triggeringAgent.name} is Confused!");

        foreach (var agent in allAgents)
        {
            if (agent == null || agent == triggeringAgent) continue;
            agent.ForceConfuse();
        }
    }
}