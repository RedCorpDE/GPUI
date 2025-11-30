using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPUI
{
    public static class UiCursor
    {
        
        public static List<Sprite> cursorSprites => GameObject.FindAnyObjectByType<UiManager>()?.currentPalette.cursorSprites;

        public enum CursorType
        {
            Arrow,
            Hand
        }
        
        public static void ChangeCursor(CursorType cursorType)
        {

            switch (cursorType)
            {

                case CursorType.Arrow:
                    SetCursor(0);
                    break;
                case CursorType.Hand:
                    SetCursor(1);
                    break;
                
            }

        }

        static void SetCursor(int index)
        {
            
            if(index < 0 || cursorSprites == null || index >= cursorSprites.Count)
                return;
            
            Cursor.SetCursor(cursorSprites[index].texture, Vector2.zero, CursorMode.Auto);
            
        }

        public static void SetCursorState(CursorLockMode cursorLockMode)
        {

            switch (cursorLockMode)
            {
                
                case CursorLockMode.None:
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
                case CursorLockMode.Locked:
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;
                
            }
            
        }
        
        public static void SetCursorState(bool isLocked = false)
        {

            switch (isLocked)
            {
                
                case false:
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
                case true:
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;
                
            }
            
        }
        
    }
}