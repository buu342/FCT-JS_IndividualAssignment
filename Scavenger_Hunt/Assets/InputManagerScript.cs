using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
public class InputManagerScript
{
    public static PlayerInput playerInput = new PlayerInput();
    public static InputAction Look = playerInput.Player.Look;
    public static InputAction Move = playerInput.Player.Move;
    

}
