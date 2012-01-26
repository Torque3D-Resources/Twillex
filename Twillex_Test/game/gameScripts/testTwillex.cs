
// apologies for all the global variables, but this is a test and this seems to keep
// the code as simple as possible (no behaviors, no extra classes, no extra files, etc).
// several classes are lumped together here (or actually just namespaces for individual 
// named scene objects, runningSack and pointer).

//------------------------------------
// general test setup/teardown

function setupTests()
{
   // save a copy of the scene object we are tweening
   // so we can use the clone's field values (position, rotation, etc) to reset the original
   // before each Tween.
   $pointerClone = pointer.cloneWithBehaviors();
   $pointerClone.visible = false;

   $nineOneOneClone = nineOneOne.cloneWithBehaviors();
   $nineOneOneClone.visible = false;

   // hide the "other" object that will get tweened
   nineOneOne.visible = false;

   setupTestGUI();
}

function teardownTests()
{
   $pointerClone.delete();
   $nineOneOneClone.delete();

   teardownTestGUI();
}

//------------------------------------
// scene graph's onUpdateSceneTick() required to update our Tweener

function TestSceneGraph::onUpdateSceneTick(%this)
{
   // don't call onUpdate unless we are set up right.  ahhh, so many less errors in the log!
   if (isObject(Tweener) && isMethod(Tweener, onUpdate))
      Tweener.onUpdate();
}

//-------------------------------------
// Throbbing Button functions

function throbButton::onAdd(%this)
{
   // for mouse enter/leave
   // note: we need two Tweens because we want the color change to run slower.
   %this.onThrobSpin = Tweener.to(1000, %this, "sx:3, sy:3, rotation:-120", "relative:true, ease:sine_inout");
   %this.onWhiten = Tweener.to(2000, %this, "r:1, g:1");
}

function throbButton::onRemove(%this)
{
   %this.onThrobSpin.delete();
   %this.onWhiten.delete();
}

function throbButton::onMouseEnter(%this)
{
   // make sure Tweens are running in "forward" and playing
   // if they were already playing (or in forward) this doesn't hurt. it just continues along

   %this.onThrobSpin.forward();
   %this.onThrobSpin.play();

   %this.onWhiten.forward();
   %this.onWhiten.play();
}

function throbButton::onMouseLeave(%this)
{
   // make sure Tweens are running in reverse and playing
   // if they were already playing (or in reverse) this doesn't hurt. it just continues along

   %this.onThrobSpin.reverse();
   %this.onThrobSpin.play();

   %this.onWhiten.reverse();
   %this.onWhiten.play();   
}

//-------------------------------------
// Shaking "No" Button functions

function shakingButton::onAdd(%this)
{
   // for mouse down
   %this.onShakeNo = Tweener.to(500, %this, "x:3.5", "relative:true, ease:linear, repeat:2, yoyo:true, around:true");
}

function shakingButton::onRemove(%this)
{
   %this.onShakeNo.delete();
}

function shakingButton::onMouseDown(%this)
{
   // note that quickly clicking the mouse just restarts the Tween.

   %this.onShakeNo.rewind();
   %this.onShakeNo.play();
}

//-------------------------------------
// Wiggle Button functions

function wiggleButton::onAdd(%this)
{
   // for mouse down
   %this.onShakeNo = Tweener.to(500, %this, "rotation:35", "relative:true, ease:linear, repeat:1, yoyo:true, around:true");
}

function wiggleButton::onRemove(%this)
{
   %this.onShakeNo.delete();
}

function wiggleButton::onMouseDown(%this)
{
   // note that quickly clicking the mouse just restarts the Tween.

   %this.onShakeNo.rewind();
   %this.onShakeNo.play();
}

//-------------------------------------
// Running Sack functions

function runningSack::onAdd(%this)
{
   // we're going to attempt to adjust an animations speed during play!

   // this turns out to be unsupported in every way.  you can't do
   // %animation.frame = x, nor can you do %animation.fps = 20;
   // so we can't tween the animation by changing the frame or fps values directly.

   // You can, however, adjust the frame through a *function* %animation.setAnimationFrame(x);
   // so we'll need to call that function every scene update.
   // to do this, we'll make our own "class" here and adjust our own value via Tweening,
   // called currentFPS.  Then we'll ask the Tween to call onChangeFPS() each time it adjusts the value.
   // this will give us the change to update the frame based on the new currentFPS.

   %this.currentFrame = 0;
}

function runningSack::onMouseDown(%this)
{
   // begin running

   // need to keep up with elapsed time, current frame of animation and frame rate
   %this.lastTime = getSimTime();
   %this.currentFrame = 0;
   %this.currentFPS = 0;

   // have to "play" the animation in order to be allowed to update the frame. kinda weird.
   %this.playAnimation("FlourSackAnimation");
   
   // update currentFPS from 0 to 30 with a sinusoidal pattern.  we'll handle the rest in onChangeFPS
   // note that we don't need to save this Tween.  it's a play once and delete.
   // this isn't the best choice for an onMouseDown since you can click quickly and make lots of
   // (competing) Tweens, but it will do for this test.
   Tweener.toOnce(5000, %this, "currentFPS:90", "onChange:onChangeFPS, yoyo:true, ease:sine_inout");
}

function runningSack::onChangeFPS(%this, %tween)
{
   // we have 23 frames [0, 22].

   // need elapsed time (in seconds) since last call
   %curTime = getSimTime();
   %elapsedTime = (%curTime - %this.lastTime) / 1000;
   %this.lastTime = %curTime;

   // how many frames have passed?
   %elapsedFrames = %elapsedTime * %this.currentFPS;

   // note: we'll keep current frame in floating point, so it has sub-frame accuracy
   %this.currentFrame += %elapsedFrames;
   
   // wrap around to stay within [0, 22] (with floating point)
   %this.currentFrame -= mFloor(%this.currentFrame / 23) * 23;

   %this.setAnimationFrame(mFloor(%this.currentFrame));
}

//-------------------------------------
// Hiding GUI functions

function setupTestGUI() {

   tweenPanel.modal = false;
   controlPanel.model = false;

   setDefaultGUIState();

   // this field is just to show the command used to start the Tween.  don't allow edit.
   exampleText.setActive(false);

   // oh hell yeah!  we're going to Tween the GUI itself!
   
   // want to show/hide the guis with a button and some Tweens.
   // my GUIs start in the "shown" position because that is where they are created in the GUI Builder.
   // we want to move it to the hide position before starting.
   // Tweens that will show and hide it have to take the loaded position into consideration,
   // hence we try to save some math by using Tweener.from().  Frankly, I'm not sure it was easier to figure out!
   
   // I'm going to use two Tweens so I can use different eases for show and hide
   %offset = -135 SPC 0;
   // hiding goes from where it is now "to" a mostly off-screen position
   $tweenPanelHide = Tweener.to(500, tweenPanel, "position:" @ %offset, "relative:true, ease:back_out");
   // showing comes "from" mostly off screen to where it is now. note same settings for position field
   $tweenPanelShow = Tweener.from(1000, tweenPanel, "position:" @ %offset, "relative:true, ease:bounce_out");
   // ok now hide it
   $tweenPanelHide.fforward();

   %offset = 0 SPC 50;
   $controlPanelHide = Tweener.to(500, controlPanel, "position:" @ %offset, "relative:true, ease:back_in");
   $controlPanelShow = Tweener.from(1000, controlPanel, "position:" @ %offset, "relative:true, ease:quint_out");
   $controlPanelHide.fforward();
}

function setDefaultGUIState()
{
   // some nice defaults
   positionButton.setvalue(true);
   sizeButton.setvalue(false);
   reddenButton.setvalue(false);
   rotateButton.setvalue(false);
   fadeButton.setvalue(false);

   noBackButton.performClick();
   quintButton.performClick();
   inButton.performClick();
   fitButton.setvalue(true);
   durationText.setValue(1000);
}

function teardownTestGUI()
{
   $tweenPanelHide.delete();
   $tweenPanelShow.delete();
   $controlPanelHide.delete();
   $controlPanelShow.delete();
   
   if (isObject($mainTween))
      $mainTween.delete();
}

function appendParam(%string, %param)
{
   // create a string of params separated by commas.
   // but if this is the first param in the string, do not put a comma in front of it.

   return %string @ (%string $= "" ? "" : ", ") @ %param;
}

function tweenPressed(%object, %onComplete, %position, %size, %rotation, %color, %alpha)
{
   if (!isObject(%object))
      %object = pointer;

   // set object back to starting position, color, size, etc.
   if (%object.getId() == pointer.getId()) {
      pointer.copy($pointerClone, true);
      pointer.visible = true;
   }
   else if (%object.getId() == nineOneOne.getId())
   {
      nineOneOne.copy($nineOneOneClone, true);
      nineOneOne.visible = true;
   }

   // get rid of the last Tween.  we are going to make a new global one.
   if (isObject($mainTween))
      $mainTween.delete();

   // create the new Tween

   %fields = GUItoFields(%position, %size, %rotation, %color, %alpha);
   %params = GUIToParams(%onComplete);

   // get control for Tween

   %duration = 1000;
   if (durationText.getText() > 0)
      %duration = durationText.getText();

   if (fromButton.getValue()) %from = true;
   
   // show the user the command that has been built
   %example = %from ? "Tweener.from(" : "Tweener.to(";
   %example = %example @ %duration @ ", %object, \"" @ %fields @ "\", \"" @ %params @ "\")";
   exampleText.setText(%example);

   if (%from)
      $mainTween = Tweener.from(%duration, %object, %fields, %params);
   else
      $mainTween = Tweener.to(%duration, %object, %fields, %params);

   // start it
   $mainTween.play();
}

function GUIToParams(%onComplete)
{
   // set params for Tween

   %params = "relative:true";
   if (yoyoButton.getValue())     %params = appendParam( %params, "yoyo:true" );
   if (flipflopButton.getValue()) %params = appendParam( %params, "flipflop:true" );
   if (repeatText.getText() > 0)  %params = appendParam( %params, "repeat:" @ repeatText.getText() );
   if (!fitButton.getValue())     %params = appendParam( %params, "fit:false" );
   if (loopButton.getValue())     %params = appendParam( %params, "loop:true" );
   if (aroundButton.getValue())   %params = appendParam( %params, "around:true" );

   // set ease for Tween

   if (LinearButton.getValue())   %ease = "linear";
   if (quintButton.getValue())    %ease = "quint";
   if (quadButton.getValue())     %ease = "quad";
   if (ElasticButton.getValue())  %ease = "elastic";
   if (BounceButton.getValue())   %ease = "bounce";
   if (BackButton.getValue())     %ease = "back";
   if (CircularButton.getValue()) %ease = "circ";
   if (SineButton.getValue())     %ease = "sine";

   // set ease direction for Tween

   if (inButton.getValue())    %easeDir = "in";
   if (outButton.getValue())   %easeDir = "out";
   if (inOutButton.getValue()) %easeDir = "inout";

   if (%ease !$= "linear" ) %ease = %ease @ "_" @ %easeDir;
   %params = appendParam( %params, "ease:" @ %ease );
   
   if (%onComplete !$= "")
      %params = appendParam( %params, "onComplete:" @ %onComplete );

   return %params;
}

function GUItoFields(%position, %size, %rotation, %color, %alpha)
{
   // if we didn't receive values for the distance to move, etc. create some now
   // note that all values are for relative movements.  (so a red change of 0, means don't change it)
   if (%position $= "") %position = "-20 5";
   if (%size $= "") %size = "4 4";
   if (%rotation $= "") %rotation = -120;
   if (%color $= "") %color = "0 -0.8 -0.8";
   if (%alpha $= "") %alpha = -0.7;

   // we're going to set short-cut fields -- r, g, b, instead of color -- to test it
   %sx = getWord(%size, 0);
   %sy = getWord(%size, 1);
   %r = getWord(%color, 0);
   %g = getWord(%color, 1);
   %b = getWord(%color, 2);

   // set fields to Tween

   %fields = "";
   if (positionButton.getValue()) %fields = appendParam( %fields, "position:" @ %position );
   if (sizeButton.getValue())     %fields = appendParam( %fields, "sx:" @ %sx @ ", sy:" @ %sy );
   if (rotateButton.getValue())   %fields = appendParam( %fields, "rotation:" @ %rotation );
   if (fadeButton.getValue())     %fields = appendParam( %fields, "a:" @ %alpha );
   if (ReddenButton.getValue())   %fields = appendParam( %fields, "r:" @ %r @ ", g:" @ %g @ ", b:" @ %b );
   
   return %fields;
}

function call911Now()
{
   // always do the same show...
   pointer.visible = false;
   nineOneOne.visible = true;
   setRandomSeed(356423);
   $call911 = 40;
   nineOneOne.call911Again();
}

// this must be a method of the flying text, so that the Tween can call it as an onComplete() method
function nineOneOne::call911Again()
{
   if ($call911 == 0) {
      pointer.visible = true;
      nineOneOne.visible = false;
      return;
   }
   $call911--;

   // randomize tween "distances"

   %position = getRandom(-20, 10) SPC getRandom(-3, 10);
   %size = getRandom(2, 6) SPC getRandom(2, 6);
   %rotation = getRandom(1, 3) * -40;
   %relativeColor = Tween::vectorSub(getRandomVividColor(), "1 1 1 1");
   %color = %relativeColor;
   %alpha = getRandom(-1, 0) * 0.8;   

echo($call911, ": ", %position, "/", %size, "/", %rotation, "/", %color, "/", %alpha);

   // randomize fields to Tween

   positionButton.setValue( getRandom() < 0.5 );
   sizeButton.setValue( getRandom() < 0.7 );
   rotateButton.setValue( getRandom() < 0.7 );
   fadeButton.setValue( getRandom() < 0.1 );
   ReddenButton.setValue( getRandom() < 0.5 );

   // randomize params for Tween

   %params = "relative:true";
   switch (getRandom(2)) {
      case 0: yoyoButton.performClick();
      case 1: flipflopButton.performClick();
      case 2: noBackButton.performClick();
   }

   // randomize ease for Tween

   switch (getRandom(7)) {
      case 0: LinearButton.performClick();
      case 1: quintButton.performClick();
      case 2: quadButton.performClick();
      case 3: ElasticButton.performClick();
      case 4: BounceButton.performClick();
      case 5: BackButton.performClick();
      case 6: CircularButton.performClick();
      case 7: SineButton.performClick();
   }

   // randomize ease direction for Tween

   switch (getRandom(2)) {
      case 0: inButton.performClick();
      case 1: outButton.performClick();
      case 2: inOutButton.performClick();
   }

   // randomize controls for Tween
   // these kind of work together or else it could get long and boring

   if (getRandom() < 0.3) {
      fitButton.setValue(false);
      repeatText.setValue( getRandom(0, 2) );
      aroundButton.setValue( getRandom() < 0.2 );
      durationText.setText( getRandom(1,3) * 200 );
   }
   else {
      fitButton.setValue(true);
      repeatText.setValue( getRandom(0, 5) );
      aroundButton.setValue( getRandom() < 0.7 );
      durationText.setText( getRandom(1,5) * 400 );
   }

   tweenPressed(nineOneOne, call911Again, %position, %size, %rotation, %color, %alpha);
}

function getRandomVividColor()
{
   // hmm. found a bug in TorqueScript?!
   // if I do a "switch(getRandom(0,5))", I sometimes fall through all cases, which should be impossible.
   // so instead I calculate getRandom(0,5) up front and then call switch with the result.
   // this works as expected.

   %rand = getRandom(0, 5);
   switch (%rand) {
      case 0: return "0 0.9 0.9 1"; // Cyan
      case 1: return "0.9 1 0 1";   // Lemon Yellow
      case 2: return "1 0 0 1";     // Red
      case 3: return "0 1 0.1 1";   // Lime Green
      case 4: return "1 0.1 0 1";   // Burnt Orange
      case 5: return "1 0 1 1";     // Purple
      default: echo("oops!"); return "0 0 1 1"; // to catch that bug.  it hasn't happened since I made the change mentioned.
   }
}

function showHideTweenPanel()
{
   // because I wanted a different ease function for hiding and showing,
   // I can't just reverse a tween here

   if (tweenHideButton.getText() $= "<") {
      tweenHideButton.setText(">");
      $tweenPanelHide.rewind();
      $tweenPanelHide.play();
      return;
   }

   tweenHideButton.setText("<");
   $tweenPanelShow.rewind();
   $tweenPanelShow.play();
}

function showHideControlPanel()
{
   // because I wanted a different ease function for hiding and showing,
   // I can't just reverse a tween here

   if (controlHideButton.getText() $= "^") {
      controlHideButton.setText("v");
      $controlPanelShow.rewind();
      $controlPanelShow.play();
      return;
   }

   controlHideButton.setText("^");
   $controlPanelHide.rewind();
   $controlPanelHide.play();
}

function playTween() { $mainTween.play(); }
function stopTween() { $mainTween.stop(); }
function rewindTween() { $mainTween.rewind(); }
function fforwardTween() { $mainTween.fforward(); }
function reverseTween() { $mainTween.reverse(); }
function forwardTween() { $mainTween.forward(); }
