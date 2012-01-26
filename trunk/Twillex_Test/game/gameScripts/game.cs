//---------------------------------------------------------------------------------------------
// Torque Game Builder
// Copyright (C) GarageGames.com, Inc.
//---------------------------------------------------------------------------------------------

//---------------------------------------------------------------------------------------------
// startGame
// All game logic should be set up here. This will be called by the level builder when you
// select "Run Game" or by the startup process of your game to load the first level.
//---------------------------------------------------------------------------------------------
function startGame(%level)
{
   Canvas.setContent(mainScreenGui);
   Canvas.setCursor(DefaultCursor);
   
   new ActionMap(moveMap);   
   moveMap.push();
   
   $enableDirectInput = true;
   activateDirectInput();
   enableJoystick();

   sceneWindow2D.loadLevel(%level);

   /* ------------------------*/
   /* Twillex test code begin */
   setupTests();
   /* Twillex test code end */
   /* ------------------------*/
}

//---------------------------------------------------------------------------------------------
// endGame
// Game cleanup should be done here.
//---------------------------------------------------------------------------------------------
function endGame()
{
   /* ------------------------*/
   /* Twillex test code begin */
   teardownTests();
   /* Twillex test code end */
   /* ------------------------*/

   sceneWindow2D.endLevel();
   moveMap.pop();
   moveMap.delete();
}
