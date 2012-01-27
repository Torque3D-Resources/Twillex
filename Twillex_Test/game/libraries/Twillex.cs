//-----------------------------------------------------------------------------
// Twillex - Scene Object Tweening
// version 1.0.1
// copyright 2012: Charles Patterson
// use, modify, and distribute freely, but leave the credits
//
//
// DEFINITION:
//
// Twillex provides animation, or "tweening", between the current state of an object and a given state.
// The "state" can be represented by *any* combination of fields, such as position, blendColor,
// size, rotation, text, or your own custom fields like myField.  There are short-cuts for fields
// of the common scene object.  specifically, x and y for position, sx and sy for size, and
// r, g, b, and a for color.
//
// There are many interpolation methods to provide "easing" to your Tweens.  Easing is the animation
// technique of speeding up at the beginning and/or slowing down at the end, called easing in and out,
// respectively.  Easing options include linear, quadratic, "bounce," "elastic,"
// and other common ones.
//
//
// USE:
//
// Setup a Twillex "engine" to run tweens.  See the section SETUP below.  In our examples, we'll assume
// it is named myTweenEngine.
//
// Then create Tweens by calling your Twillex engine's to() function:
//
//    %t = myTweenEngine.to(1000, %banner, "position:100 0, rotation:-45, r:0.2");
//    %t.play();
//
// Here we built a Tween (controlled by myTweenEngine).  It will run for 1 second (1000 milliseconds),
// and it will affect the %banner object.  We see that it will vary the fields 'position' -- which
// is a two-element vector, by the way -- 'rotation', and the red component of the blendColor.
// The Tween sits dormant until you command it to play, which we did.  Suppose this is a button or
// HUD component and the time has come to show it again:
//
//    %t.rewind();
//    %t.play();
//
// Now send it back where it came from:
//
//    %t.reverse();
//    %t.play();
//
// Then when the Tween is no longer needed like at the end of the scene:
//
//    %t.delete();
//
// Here is an example of a one-off Tween:
//
//    %t = myTweenEngine.toOnce(500, %banner, "x:100, y:0, myVar:0.01", "yoyo:true");
//
// Twillex cannot be sure what your fields will be called so it accepts them all, such as "position"
// or "text" or "myVar".  You may get an error at run-time if the field didn't exist or wasn't a
// short-cut field as was described earlier.  We also see a second string which holds params that affect
// the way the Tween plays out.  Due to our "yoyo" param, this Tween plays through then plays
// backwards to the beginning and stops (all within 500 milliseconds).  Since we wanted a one-time
// Tween, we used toOnce().  This is a convenience function which calls play() for you and also
// add a parameter "delete:true" so that the Tween deletes itself on completion.  You could have used
// to() and added the parameter yourself and also called play().
//
// see $Twillex::paramValues for a complete list of parameters to affect the Tween
//
// SETUP/TEARDOWN/MAINTENANCE:
//
// To create a Twillex engine requires set up, tear down, and frame updates.
//
// Setup and tear-down work as usual in Torque.
//
// First, include the script
//
//    exec("<your_path>/Twillex.cs");
//
// Then, create a Twillex engine to run your tweens.
//
//   Twillex::create(myTweenEngine);
//
// You could even make several Twillex engines if you wanted to keep categories of Tweens separate.
// In the end, delete your Twillex objects as usual:
//
//    myTweenEngine.delete();
//
// It's not possible to get Torque to call an onUpdate() on Twillex objects each frame.
// The scene graph will have to be kind and call Twillex::onUpdate().
// For example:
//
// function mySceneGraph::onUpdateSceneTick(%this) {
//    // do all the other stuff also
//    myTweenEngine.onUpdate();
// }
//
// Note that you don't have to call onUpdate on the individual Tweens, just the Twillex engines.


// possible parameters for Tween i.e. "delete:true, yoyo:true, repeat:1"
$Twillex::paramValues =
   "delete"     TAB  // if true, when Tween completes, delete it (default is false)
   "ease"       TAB  // interpolation method. see $Twillex::easeValues below (default is linear)
   "repeat"     TAB  // number of times to run the Tween where 0 means run once (no repeats) and 3 means run 4 times.  (default is 0).  each run uses up the entire given duration
   "yoyo"       TAB  // if true, run the Tween backwards when it reaches the end (default is false).  takes twice as long.
   "flipflop"   TAB  // if true, run the Tween in reverse BUT run the ease forward.  (default is false)
   "relative"   TAB  // if true, end values mean relative to start values (defaults is false)
   "oncomplete" TAB  // if set, when playing reaches end, call() the function given (default is unset)
   "onchange"   TAB  // if set, when Tween updates values, call() the function given (default is unset)
   "around"     TAB  // if true, play Tween around start value (default is false)
   "loop"       TAB  // if true, replay forever. (default is false)
   "fit";            // if true, all repeats and yoyoing happens within the given duration (default is true)

$Twillex::easeValues =
   "linear"        TAB // move at a constant rate
   "sine_in"       TAB // speed up sinusoidally (quarter turn or PI/2)
   "sine_out"      TAB // slow down sinusoidally (quarter turn or PI/2)
   "sine_inout"    TAB // speed up and slow down sinusoidally (half turn or PI)
   "circ_in"       TAB // speed up circularly (90 degrees)
   "circ_out"      TAB // slow down circularly (90 degrees)
   "circ_inout"    TAB // speed up then slow down in a half circle pattern (180 degrees)
   "quad_in"       TAB // speed up quadratically (squared)
   "quad_out"      TAB // slow down quadratically (squared)
   "quad_inout"    TAB
   "quint_in"      TAB // speed up quintically (to the 5th)
   "quint_out"     TAB // slow down quintically (to the 5th)
   "quint_inout"   TAB
   "elastic_in"    TAB
   "elastic_out"   TAB // spring around the end
   "elastic_inout" TAB
   "back_in"       TAB // back up a bit at the start (get a running start)
   "back_out"      TAB // overshoot the end
   "back_inout"    TAB // both back_in and back_out
   "bounce_in"     TAB
   "bounce_out"    TAB // end like a ball hitting the floor and bouncing 4 times
   "bounce_inout";

// some constants we need

$Twillex::PI = 3.141592654;
$Twillex::PI_2 = $Twillex::PI * 2;
$Twillex::PI_HALF = $Twillex::PI / 2;

// ---------------------------------------------
// Twillex class

/* static */
function Twillex::create(%name)
{
   // no need to return it if you named it!
   if (%name !$= "")
      new ScriptObject(%name) { class = Twillex; };
   else
      return new ScriptObject() { class = Twillex; };
}

function Twillex::onAdd(%this)
{
   echo("Twillex::onAdd");
   // the engine will keep a list of Tweens playing and those that are idle.
   // this should improve performance because you could set up many latent Tweens
   // but Twillex only looks at its playing list during onUpdate()
   %this.playing = new SimSet();
   %this.idle = new SimSet();
   %this.lastTime = getSimTime();
}

function Twillex::onRemove(%this)
{
   echo("Twillex::onRemove");
 
   // take all our Tweens down with us!

   %count = %this.playing.getCount();
   for (%i = %count - 1; %i >= 0; %i--)
      %this.playing.getObject(%i).delete();
   %this.playing.delete();

   %count = %this.idle.getCount();
   for (%i = %count - 1; %i >= 0; %i--)
      %this.idle.getObject(%i).delete();
   %this.idle.delete();
}

function Twillex::to(%this, %rawDuration, %target, %fields, %params)
{
   if (! isObject(%target)) {
      error("Twillex::to: target '", %target, "' is not an object.  Tween not created.");
      return;
   }

   %tweenData = %this.initTweenData(%rawDuration, %fields, %params);
   if (%tweenData == 0) {
      error("Twillex::to: trouble creating TweenData.  Tween not created.");
      return;
   }

   // now make the main Tween instance
   // (as opposed to the read-only tween parameters 'tweenData' we created above)

   %tween = new ScriptObject() {
      class = Tween;
      target = %target;
      tweenData = %tweenData;

      // cross reference Tween with the engine running it so we can talk to the engine
      twillex = %this;
   };

   %this.engageTween(%tween, false);

   return %tween;
}

function Twillex::toOnce(%this, %rawDuration, %target, %fields, %params)
{
   // like to() except this is meant to be a one-time Tween, so set it to
   // delete itself at the end, and start it for the caller.

   %params = %params @ (%params $= "" ? "" : ", ") @ "delete:true";
   %tween = %this.to(%rawDuration, %target, %fields, %params);
   %tween.play();
   return %tween;
}

function Twillex::from(%this, %rawDuration, %target, %fields, %params)
{
   // like to() except that we *end* the Tween in its current position
   // just a convenience function, as we could have created a to() Tween,
   // reverse() it and then rewind() it before play.

   // hack here: we'll create a normal Tween,
   // and then cheat and swap the start and end data

   %tween = %this.to(%rawDuration, %target, %fields, %params);
   Twillex::convertToFrom(%tween);
   return %tween;
}

function Twillex::fromOnce(%this, %rawDuration, %target, %fields, %params)
{
   // hack here: we'll create a normal Tween, including starting it,
   // and then cheat and swap the start and end data

   %tween = %this.toOnce(%rawDuration, %target, %fields, %params);
   Twillex::convertToFrom(%tween);
   return %tween;   
}

function Twillex::initTweenData(%this, %rawDuration, %fields, %params)
{
   // tweenData will hold the read-only Tween properties (not the current state of the Tween).

   %tweenData = new ScriptObject();

   // * put in the raw duration

   if (%rawDuration <= 0){
      error("Twillex::initTweenData: duration '", %rawDuration, "' must be positive.  failed to make TweenData.");
      return 0;
   }
   %tweenData.rawDuration = %rawDuration;

   // * put in the fields
   // load up fields to be tweened, but we can't check them to a white list of acceptable fields
   // trust every "field" value is fine, because we really don't know.  Is there a "position" for this object?

   %keys = Twillex::parseStringArgs(%fields, "", %tweenData);
   if ( %keys $= "0" ) {
      error("Twillex::to: fields '", %fields, "' are not valid. failed to make TweenData.");
      %tweenData.delete();
      return 0;
   }

   // save a list of fields to be tweened
   %tweenData.fields = %keys;

   // * put in the parameters
   // load up tweening parameters after checking them to a white list
   %keys = Twillex::parseStringArgs(%params, $Twillex::paramValues, %tweenData);
   if ( %keys $= "0" ) {
      error("Twillex::initTweenData: args '", %params, "' are not valid.  failed to make TweenData.");
      %tweenData.delete();
      return 0;
   }

   if (%tweenData.flipflop $= "true" && %tweenData.yoyo $= "true") {
      warn("Twillex::initTweenData: Tween includes both 'flipflop' and 'yoyo'.  Only 'flipflop' will be active.");
      %tweenData.yoyo = "";
   }

   if (%tweenData.fit $= "")
      %tweenData.fit = "true";

   if (%tweenData.ease $= "")
      %tweenData.ease = "linear";
   else {
      if (! Twillex::validateEase(%tweenData.ease)) {
         error("Twillex::initTweenData: 'ease' arg value '", %tweenData.ease, "' is not recognized.  failed to make TweenData.");
         %tweenData.delete();
         return 0;
      }
   }
   %tweenData.interpolator = Twillex::getInterpolationFunction(%tweenData.ease);

   return %tweenData;
}

/* static */
function Twillex::getInterpolationFunction(%ease)
{
   return "ease_" @ strlwr(%ease);
}

/* static */
function Twillex::validateEase(%ease)
{
   for (%i = 0; %i < getFieldCount($Twillex::easeValues); %i++) {
      if (getField( $Twillex::easeValues, %i) $= %ease)
         return true;
   }
   
   return false;
}

/* package private */
function Twillex::engageTween(%this, %tween, %play) {

   // for efficiency, we're going to let the Twillex instance know if a Tween is playing or not,
   // so it doesn't have to send onUpdate to idle tweens.
   
   // TODO: remove for efficiency
   if (%tween.twillex.getId() != %this.getId()) {
      error("Twillex::engageTween: tween '", %tween, "' does not belong to Twillex '", %this, "'");
      return;
   }

   // assume that if the Tween has no twillexList, this is its first time here
   // if this is your first time, we don't need to remove you from a list
   %experienced = isObject(%tween.twillexList);

   if (%play) {
      if (%experienced) {
         // get out early if we were already marked as playing
         if (%tween.twillexList.getId() == %this.playing.getId())
            return;

         %this.idle.remove(%tween);
      }
      %tween.twillexList = %this.playing;
      %this.playing.add(%tween);
      
      // sanity check (came up often during testing)
      if (%tween.getElapsedTime() == %tween.duration)
         warn("trying to play a Tween that is at the end of its duration already.  nothing will happen.");
   }
   else {
      if (%experienced) {
         // get out early if we were already marked as idle
         if (%tween.twillexList.getId() == %this.idle.getId())
            return;

         %this.playing.remove(%tween);
      }
      %tween.twillexList = %this.idle;
      %this.idle.add(%tween);
   }
}

/* static private */
function Twillex::parseStringArgs(%args, %validKeys, %object)
{
   // parse a string of args %args into an object %object, using keys %validKeys to verify the acceptable args
   // for instance
   //
   //    "x:1, y:2"
   //
   // would set
   //
   //    %object.x = 1 and
   //    %object.y = 2.
   //
   // The %args format is "key1:value1, key2:value2, ..."
   //   where key should contain no spaces
   //   and where value must be a scalar or a "torque vector" -- scalars with spaces between, for example
   //
   //       "a:3, repeat:1, color:1 0.2 0.2, x:-3000"
   //
   // %validKeys should be key1 TAB key2 TAB ... keyn with no spaces in each key.
   // leave this off or use "" to ignore validation
   // %objects can be any SimObject or you can leave this argument empty and the function will only validate
   // the %args, but not assign them to an object
   //
   // returns the list of keys found.  otherwise, returns 0 on error

   // see if the arg structure is correct, and collect the keys if so
   %copy = %args;
   while (%copy !$= "") {
      %copy = nextToken(%copy, pair, ",");

      // %pair should hold "key1:value1" or "key1:value1a value1b ..."
      %value = nextToken(%pair, key, ":");
      // %value should hold "value1" or "value1a value1b ..."
      // %key should hold "key1"
      if (trim(%key) $= "" || trim(%value) $= "" || strpos(%value, ":") >= 0 ) {
         error("Twillex::parseStringArgs: could not parse args '", %args, "' near '", %pair, "'");
         return "0";
      }

      %key = trim(%key);
      %keys = %keys $= "" ? %key : %keys SPC %key;
   }

   // so far so good, would we like to check if the keys are all in %validKeys?
   if (%validKeys !$= "") {
      %copy = %args;
      while (%copy !$= "") {
         %copy = nextToken(%copy, pair, ",");
         %value = nextToken(%pair, key, ":");

         %key = trim(%key);
         if (strstr(%validKeys, strlwr(%key)) < 0) {
            error("Twillex::parseStringArgs: arg '", %key, "' not in list '", %validKeys, "'");
            return "0";
         }
      }
   }

   // trying to fill an object while we're here?
   if (%object $= "")
      return %keys;

   if (!isObject(%object)) {
      error("Twillex::parseStringArgs: '", %object, "' is not an object.  bailing.");
      return "0";
   }
   
   %copy = %args;
   while (%copy !$= "") {
      %copy = nextToken(%copy, pair, ",");
      %value = nextToken(%pair, key, ":");

      %object.setFieldValue(trim(%key), trim(%value));
   }
   
   return %keys;
}

/* private */
function Twillex::deleteAllForSet(%this, %target, %set)
{
   for( %i == %set.getCount() - 1; %i >= 0; %i++)
   {         
      %tween = %set.getObject(%i);

      if(%tween.target.getID() == %target.getID())
         %tween.delete();
   }
}

function Twillex::deleteAllFor(%this, %target)
{
   %this.deleteAllForSet(%target, %this.playing);
   %this.deleteAllForSet(%target, %this.idle);
}

// note: Torque won't call Twillex::onUpdate "naturally", so the programmer
// will have to call this from t2dSceneGraph::onUpdateSceneTick
function Twillex::onUpdate(%this)
{
   %curTime = getSimTime();
   %timeElapsed = %curTime - %this.lastTime;
   %this.lastTime = %curTime;

   for( %i = 0; %i < %this.playing.getCount(); %i++ )
   {
      %tween = %this.playing.getObject(%i);

      // check if the object we are animating exists (still).
      // take this opportunity to throw out Tweens for targets that have been deleted.
      // note that SimSets normally remove reference to objects that have been deleted, but
      // we are worried about the targets of the tweens, not the Tween itself.
      // This list wouldn't notice the targets being deleted, but then the Tween would be useless.
      if(!isObject(%tween.target))
      {
         echo("Twillex::onUpdate: found an Tween with a bad target.  Deleting it.");
         %tween.delete();
         %i--;
         continue;
      }         

      %tween.onUpdate( %timeElapsed );
   }

   // TODO: when is a good chance to prune the idle list?  here but every 5 seconds or a minute?
}

// ---------------------------------------------
// Tween class

function Tween::onAdd(%this)
{
   %this.reset();
}

function Tween::onRemove(%this)
{
   %this.tweenData.delete();
}

// play Tween where it left off.
// when it reached the end it will stop.
// it is safe to call it multiple times.  it will just keep playing.
// calling again does not pause or stop or reset the Tween to the beginning.
// if the Tween was already at its end when you call this, it will stop (and call onComplete)
// The "end" of the tween may be the start if we are in reverse.  this should work out naturally.
function Tween::play(%this)
{
   %this.twillex.engageTween(%this, true);
}

// stop Tween where it is.
// it is safe to call it multiple times.
// this does not cause the call of onComplete. only play can cause that
function Tween::stop(%this)
{
   %this.twillex.engageTween(%this, false);
}

// play Tween in reverse
// calling this over and over will not switch between forward and reverse
// it is safe to call it multiple times
function Tween::reverse(%this)
{
   %this.reverse = true;
}

// play Tween in "normal", or forward, direction
// it is safe to call it multiple times
function Tween::forward(%this)
{
   %this.reverse = false;
}

// sets the Tween back to its beginning.
// (or if we're in reverse, set it to the ending, but this should work out naturally)
// if it was playing it will continue to play.
// if it was stopped it will stay stopped.
function Tween::rewind(%this)
{
   %this.setElapsedTime( 0 );
   %this.interpolateAllFields();
}

// sets the Tween to its ending.
// (or if we're in reverse, set it to the beginning, but this should work out naturally)
// if it was playing it will continue to play (but now we're at the end so it will stop before any more Tweening)
// if it was stopped it will stay stopped.
function Tween::fforward(%this)
{
   %this.setElapsedTime( %this.duration );
   %this.interpolateAllFields();
}

// reset us to reset our Tween to start where we are now (not where we were
// when the Tween was first created) using all the data already in the Tween --
// duration, fields, and parameters.
// note that if this Tween was build with relative or absolute values, those still hold
// for instance, if we were moving 10 units in x, we will move another 10 in x from here
// if we were moving to position 0 in x, we will again move to position 0 in x.
// this will probably work best for position/rotation Tweens, but we'll see
function Tween::reset(%this)
{
   %this.initInterpolationRanges();
   %this.setElapsedTime( 0 );
}

function Tween::isPlaying(%this)
{
   return (%this.twillexList.getId() == %this.twillex.playing.getId());
}

function Tween::vectorAdd(%v1, %v2)
{
   // a generic element-wise addition of two n-length vectors

   // TODO: remove for performance
   if (getWordCount(%v1) != getWordCount(%v2))
      error("Tween::vectorAdd: vectors v1 \"", %v1, "\" and v2 \"", %v2, "\" are different lengths.  soldiering on.");

   for (%i = 0; %i < getWordCount(%v1); %i++) {
      %e = getWord(%v1, %i) + getWord(%v2, %i);
      %temp = %i == 0 ? %e : %temp SPC %e;
   }

   return %temp;
}

function Tween::vectorSub(%v1, %v2)
{
   // a generic element-wise subtraction of two n-length vectors

   // TODO: remove for performance
   if (getWordCount(%v1) != getWordCount(%v2))
      error("Tween::vectorSub: vectors v1 \"", %v1, "\" and v2 \"", %v2, "\" are different lengths.  soldiering on.");

   for (%i = 0; %i < getWordCount(%v1); %i++) {
      %e = getWord(%v1, %i) - getWord(%v2, %i);
      %temp = %i == 0 ? %e : %temp SPC %e;
   }

   return %temp;
}

function Tween::vectorNegate(%v)
{
   // a generic element-wise negation of an n-length vector

   for (%i = 0; %i < getWordCount(%v); %i++) {
      %e = -getWord(%v, %i);
      %temp = %i == 0 ? %e : %temp SPC %e;
   }

   return %temp;
}

function Tween::initInterpolationRanges(%this)
{
   %relative = (%this.tweenData.relative $= "true");

   %this.duration = %this.tweenData.rawDuration;
   if (%this.tweenData.fit !$= "true") {
      %this.duration *= (%this.tweenData.repeat + 1);
      if (%this.tweenData.yoyo $= "true" || %this.tweenData.flipflop $= "true")
         %this.duration *= 2;
   }

   // this will be a little hard on the eyes because it needs to work with arbitrary fields.
   // therefore is uses getFieldValue.  That is, where you may normally see
   //
   //    echo(%object.a)
   //    %object.a = 3;
   //
   // this has to say
   //
   //    %field = "a";
   //    echo(%object.getFieldValue(%field))
   //    %object.setFieldValue(%field, 3);
   //
   // because it doesn't know the field name up front.
   // this type of code happens any time we work with the fields being tweened

   // init each field being Tweened
   for (%i = 0; %i < getWordCount(%this.tweenData.fields); %i++) {
      %field = getWord(%this.tweenData.fields, %i);
      %value = %this.tweenData.getFieldValue(%field);

      // handle "syntactic sugar" cases for scene objects like "x" (position x) or "r" (blendColor red)
      if (%this.initInterpolationRangeSpecialField(%field, %value, %relative))
         continue;

      // wasn't a special case?  then do the normal field setup
      // note that vectorAdd() and vectorSub() can work on scalars too
      %start = %this.target.getFieldValue(%field);
      %end = %relative ? Tween::vectorAdd(%start, %value) : %value;
      %diff = Tween::vectorSub(%end, %start);

      %this.setFieldValue(startValue @ %field, %start);
      %this.setFieldValue(endValue @ %field, %end);
      %this.setFieldValue(diffValue @ %field, %diff);
   }
}

function Tween::initInterpolationRangeSpecialField(%this, %field, %value, %relative)
{
   switch$ (%field) {

      case "x":
         %this.startValueX = getWord(%this.target.getPosition(), 0);

         if(%relative)
            %this.endValueX = %this.startValueX + %value;
         else
            %this.endValueX = %value;

         %this.diffValueX = %this.endValueX - %this.startValueX;
         return true;

      case "y":
         %this.startValueY = getWord(%this.target.getPosition(), 1);

         if(%relative)
            %this.endValueY = %this.startValueY + %value;
         else
            %this.endValueY = %value;

         %this.diffValueY = %this.endValueY - %this.startValueY;
         return true;

      case "sx":
         %this.startValueSX = getWord(%this.target.getSize(), 0);

         if(%relative)
            %this.endValueSX = %this.startValueSX + %value;
         else
            %this.endValueSX = %value;

         %this.diffValueSX = %this.endValueSX - %this.startValueSX;
         return true;

      case "sy":
         %this.startValueSY = getWord(%this.target.getSize(), 1);

         if(%relative)
            %this.endValueSY = %this.startValueSY + %value;
         else
            %this.endValueSY = %value;

         %this.diffValueSY = %this.endValueSY - %this.startValueSY;
         return true;

      case "r":
         %this.startValueR = getWord(%this.target.getBlendColor(), 0);

         if(%relative)
            %this.endValueR = %this.startValueR + %value;               
         else
            %this.endValueR = %value;   

         %this.diffValueR = %this.endValueR - %this.startValueR;
         return true;

      case "g":
         %this.startValueG = getWord(%this.target.getBlendColor(), 1);

         if(%relative)
            %this.endValueG = %this.startValueG + %value;               
         else
            %this.endValueG = %value;   

         %this.diffValueG = %this.endValueG - %this.startValueG;
         return true;

      case "b":
         %this.startValueB = getWord(%this.target.getBlendColor(), 2);

         if(%relative)
            %this.endValueB = %this.startValueB + %value;               
         else
            %this.endValueB = %value;   

         %this.diffValueB = %this.endValueB - %this.startValueB;
         return true;

      case "a":
         %this.startValueA = getWord(%this.target.getBlendColor(), 3);

         if(%relative)
            %this.endValueA = %this.startValueA + %value;
         else
            %this.endValueA = %value;

         %this.diffValueA = %this.endValueA - %this.startValueA;
         return true;

      default:
         return false;
   }
}

/* static protected */
function Twillex::convertToFrom(%tween)
{
   // hack to make writing from(), fromOnce(), and possiby other convenience functions easier
   // set Tween with to() normally, then reverse some fields

   // for instance, for field x, swap startValueX and endValueX.
   // then negate diffValueX

   // swap start/end for each field being Tweened
   for (%i = 0; %i < getWordCount(%tween.tweenData.fields); %i++) {
      %field = getWord(%tween.tweenData.fields, %i);

      %start = %tween.getFieldValue(startValue @ %field);
      %end = %tween.getFieldValue(endValue @ %field);
      %diff = %tween.getFieldValue(diffValue @ %field);

      %diff = Tween::vectorNegate(%diff);

      %tween.setFieldValue(startValue @ %field, %end);
      %tween.setFieldValue(endValue @ %field, %start);
      %tween.setFieldValue(diffValue @ %field, %diff);
   }
}

function Tween::onUpdate(%this, %elapsedTime)
{
   // some magic below:
   // setElapsedTime/getElapsedTime/fforward will make the clock always look like it's going forward.
   // these functions take into account Tween "reverse" and adjust so we don't have to!

   %this.setElapsedTime( %this.getElapsedTime() + %elapsedTime );

   // reached end of Tween?
   if( %this.getElapsedTime() < %this.duration ) {
      %this.interpolateAllFields();
      return;
   }

   // at (or past) end of Tween

   // loop forever?
   if (%this.tweenData.loop $= "true") {
      // TODO: if we were past duration then we should be a little past 0 now
      %this.rewind();
      return;
   }
   
   // stop, end on exactly last frame, call onComplete, maybe delete.
   %this.stop();
   %this.fforward();

   if (%this.tweenData.onComplete !$= "")
      %this.target.call(%this.tweenData.onComplete, %this);

   if (%this.tweenData.delete $= "true")
      %this.delete();

   return;
}

function Tween::setElapsedTime(%this, %value)
{
   // note: setElapsedTime and getElapsedTime will hide the 'reverse' state by
   // creating the illusion that time is moving forward only.  This will simplify some code.
   // For instance, you can stop playing when elapsed time == duration, without worrying about
   // "or if in reverse, stop when elapsed time == 0."
   // A function can avoid these functions and instead acquire elapsedTime directly.

   %this.elapsedTime = %this.reverse ? (%this.duration - %value) : %value;
}

function Tween::getElapsedTime(%this)
{
   // see comments in setElapsedTime()

   return %this.reverse ? (%this.duration - %this.elapsedTime) : %this.elapsedTime;
}

function Tween::interpolateAllFields(%this)
{
   // this is where all the real tricks happen.
   // this code takes one long timeline 0 -> duration and makes one gigantic function over time
   // that takes into account repeating, yoyo'ing, and flipflop'ing.
   // it does this by twittling the time sent to the interpolation function, and also
   // twittling the result of the function.

   // normalize the time for interpolation
   %t = %this.elapsedTime / %this.duration;

   // if we are repeating, divide the time [0, 1] into multiple runs.
   // do this with [%t * (repeat + 1)] mod (repeat + 1);
   if (%this.tweenData.repeat > 0) {
      %t = %t * (%this.tweenData.repeat + 1);
      if (%t != 0 && %t == mFloor(%t)) // at end of each run, careful with floor'ing!
         %t = 1;
      else
         %t -= mFloor(%t);
   }

   // if we are in "around" mode, while yoyo'ing or flipflop'ing, let's do a nice
   // trick where we begin and end on the start.  if we were going to pass through the
   // start and make it half-way to the end, instead let's go from start to before start,
   // then cross through start to half-way to the end, then return to start.  Same thing,
   // but "out of phase" by a quarter of the yoyo/flipflop.
   // offset t by 0.25 and wrap it to [0, 1]
   if (%this.tweenData.around $= "true") {
      if (%this.tweenData.yoyo $= "true" || %this.tweenData.flipflop $= "true" ) {
         if (%t >= 0.25)
            %t -= 0.25;
         else
            %t += 0.75;
      }
   }

   // note: don't want to flipflop and yoyo, so the calling code should have forced one or the other by here
   // if we are yoyo'ing, this is an easy way to do so, now that we are normalized (per repeat)
   if (%this.tweenData.yoyo $= "true") {
      %t = %t * 2;
      if (%t > 1.0)
         %t = 2 - %t;
   }

   // note: all of our fields are independent.  and all of the elements in a vector, such as position x and y,
   // are independent.  this means we can get f(t) for the current time and reuse it for all fields and all
   // elements of this Tween.  (some types of interpolation would not allow this, but I haven't needed any.)

   if (%this.tweenData.flipflop !$= "true") {
      %ft = %this.call(%this.tweenData.interpolator, %t);
   }
   else {
      if (%t <= 0.5)
         %ft = %this.call(%this.tweenData.interpolator, %t * 2);
      else
         %ft = 1 - %this.call(%this.tweenData.interpolator, %t * 2 - 1);
   }

   // if we are in "around" mode, offset f(t) to go from half the diff before our start point to half the diff after it.
   // that is, instead of [0, 1], use [-0.5, 0.5].  put differently, we don't want to tween from start to end.
   // we want to tween from half the distance to the end from half the distance before the start
   if (%this.tweenData.around $= "true") {
      %ft -= 0.5;
   }

   for (%i = 0; %i < getWordCount(%this.tweenData.fields); %i++) {
      %field = getWord(%this.tweenData.fields, %i);
      %this.interpolate(%field, %ft);
   }

   if (%this.tweenData.onChange !$= "")
      %this.target.call(%this.tweenData.onChange, %this);
}

function Tween::interpolate(%this, %field, %ft)
{
   // handle "syntactic sugar" cases for scene objects fields like "x" (position x) or "r" (blendColor red)
   if (%this.interpolateSpecialField(%field, %ft))
      return;

   // wasn't a special case?  OK, grab generic fields and interpolate
   %start = %this.getFieldValue(startValue @ %field);
   %diff = %this.getFieldValue(diffValue @ %field);
   %value = "";

   // interpolate each element of a vector independently
   // (or if this was a scalar, that will work too)
   for (%i = 0; %i < getWordCount(%start); %i++) {
      %elemStart = getWord(%start, %i);
      %elemDiff = getWord(%diff, %i);
      %elemValue = %elemStart + %ft * %elemDiff;
      %value = %i == 0 ? %elemValue : %value SPC %elemValue;
   }
   %this.target.setFieldValue(%field, %value);
}

function Tween::interpolateSpecialField(%this, %field, %ft)
{
   switch$(%field) {

      case "x":
         %value = %this.startValueX + %ft * %this.diffValueX;
         %this.target.setPositionX(%value);
         return true;

      case "y":
         %value = %this.startValueY + %ft * %this.diffValueY;
         %this.target.setPositionY(%value);
         return true;

      case "sx":
         %value = %this.startValueSX + %ft * %this.diffValueSX;
         %this.target.setSizeX(%value);
         return true;

      case "sy":
         %value = %this.startValueSY + %ft * %this.diffValueSY;
         %this.target.setSizeY(%value);
         return true;

      case "a":
         %value = %this.startValueA + %ft * %this.diffValueA;
         %this.target.setBlendAlpha(%value);
         return true;

      case "r":
         %value = %this.startValueR + %ft * %this.diffValueR;
         %this.target.setBlendColor(setWord(%this.target.getBlendColor(), 0, %value));
         return true;

      case "g":
         %value = %this.startValueG + %ft * %this.diffValueG;
         %this.target.setBlendColor(setWord(%this.target.getBlendColor(), 1, %value));
         return true;

      case "b":
         %value = %this.startValueB  + %ft * %this.diffValueB;
         %this.target.setBlendColor(setWord(%this.target.getBlendColor(), 2, %value));
         return true;

      default:
         return false;
   }
}

// ---------------------------------------------
// interpolators

// each interpolator should return a normalized value [0, 1] over time.
// the calling function will take our normalized output and scale it to the
// desired range (start + t(end - start)) whatever that range is.
//
// the input time will be between [0, 1] always, because the calling function will have
// normalized [0..duration] to [0..1] for our convenience.
//
// our interpolation functions can be *really* simple due to us getting t between 0 and 1
// and sending back a value between 0 and 1.

// note that we can sometimes output values outside of 0 to 1 for special effect,
// such as overshooting the target.

// note: I would like these to be static functions (no %this), however then I can't
// call them as %this.call().  And making then regular (global) functions seems messy.
// perhaps I should change these to be their own class and otherwise route the calls.

// -----------------------
// interpolation helper functions

function Tween::flipFunction(%this, %func, %t)
{
   // flip %func vertically and horizontally,
   // which is usually the difference between ease in and ease out

   // flip(f(t)) => [1 - f(1-t)]

   return 1 - %this.call(%func, 1 - %t);
}

function Tween::halfFunction(%this, %func, %t)
{
   // send in %t between [0, 0.5] to get back values between [0, 0.5]
   // this is specifically useful for inOutFunction()

   return %this.call(%func, 2 * %t) / 2;
}

function Tween::inOutFunction(%this, %funcIn, %funcOut, %t)
{
   // use %funcIn for first half of interpolation with results half sized.
   // use %funcOut for the second half with results also half sized, but also
   // shifted from [0, 0.5] up to [0.5, 1]
   
   // note: the reason we want to use normal interpolators but squished, is
   // because we can ensure that if t = 0.5, then f(t) = 0.5.  That is, both interpolators
   // should touch in the middle.

   if (%t < 0.5)
      return %this.call(halfFunction, %funcIn, %t);
   return %this.call(halfFunction, %funcOut, %t - 0.5) + 0.5;
}

// -----------------------
// interpolations

//--------------
// linear

function Tween::ease_linear(%this, %t)
{
   return %t;
}

//--------------
// quadratic (quad)

function Tween::ease_quad_in(%this, %t)
{
   return %t * %t;
}

function Tween::ease_quad_out(%this, %t)
{
   return %this.flipfunction(ease_quad_in, %t);
}

function Tween::ease_quad_inout(%this, %t)
{
   return %this.inOutFunction(ease_quad_in, ease_quad_out, %t);
}

//--------------
// quintic (quint)

function Tween::ease_quint_in(%this, %t)
{
   return %t * %t * %t * %t * %t;
}

function Tween::ease_quint_out(%this, %t)
{
   return %this.flipfunction(ease_quint_in, %t);
}

function Tween::ease_quint_inout(%this, %t)
{
   return %this.inOutFunction(ease_quint_in, ease_quint_out, %t);
}

//--------------
// sinusoidal (sine)

function Tween::ease_sine_in(%this, %t)
{
   return mSin(%t * $Twillex::PI_HALF - $Twillex::PI_HALF) + 1;
}

function Tween::ease_sine_out(%this, %t)
{
   // hard-coded this instead of using flipFunction
   return mSin(%t * $Twillex::PI_HALF);
}

function Tween::ease_sine_inout(%this, %t)
{
   // hard-coded this instead of using inOutFunction
   return 0.5 * mSin(%t * $Twillex::PI - $Twillex::PI_HALF) + 0.5;
}

/*
// Robert Penner's
// ActionScript 3 sine equations
public static function easeIn (t:Number, b:Number, c:Number, d:Number):Number {
   return -c * Math.cos(t/d * _HALF_PI) + c + b;
}
public static function easeOut (t:Number, b:Number, c:Number, d:Number):Number {
   return c * Math.sin(t/d * _HALF_PI) + b;
}
public static function easeInOut (t:Number, b:Number, c:Number, d:Number):Number {
   return -c*0.5 * (Math.cos(Math.PI*t/d) - 1) + b;
}
*/

//--------------
// circular (circ)

function Tween::ease_circ_in (%this, %t)
{
   return -mSqrt(1 - %t * %t) + 1;
}

function Tween::ease_circ_out (%this, %t)
{
   // hard-coded this instead of using flipFunction
   %t -= 1;
   return mSqrt(1 - %t * %t);
}

function Tween::ease_circ_inout (%this, %t)
{
   // hard-coded this instead of using inOutFunction
   if (%t < 0.5) return 0.5 * %this.ease_circ_in(%t * 2);
   return 0.5 * %this.ease_circ_out(%t * 2 - 1) + 0.5;
}

/*
// Robert Penner's
// ActionScript 3 circular functions
public static function easeIn (t:Number, b:Number, c:Number, d:Number):Number {
   return -c * (Math.sqrt(1 - (t/=d)*t) - 1) + b;
}
public static function easeOut (t:Number, b:Number, c:Number, d:Number):Number {
   return c * Math.sqrt(1 - (t=t/d-1)*t) + b;
}
public static function easeInOut (t:Number, b:Number, c:Number, d:Number):Number {
   if ((t/=d*0.5) < 1) return -c*0.5 * (Math.sqrt(1 - t*t) - 1) + b;
   return c*0.5 * (Math.sqrt(1 - (t-=2)*t) + 1) + b;
}
*/

//--------------
// elastic

function Tween::ease_elastic_out (%this, %t)
{
   // TODO: cannot take a "diffValue" of 0
   %a = 0;
   %p = 0;

   if ( %t == 0 ) return 0;
   if ( %t == 1 ) return 1;

   if (%p == 0) %p = 0.3;
   if (%a == 0 || %a < 1) {
      %a = 1;
      %s = %p / 4;
   }
   else
      %s = %p/$Twillex::PI_2 * mAsin(1/%a);

   return (%a * mPow(2,-10*%t) * mSin((%t - %s) * $Twillex::PI_2/%p ) + 1);
}

function Tween::ease_elastic_in (%this, %t)
{
   %this.flipfunction(ease_elastic_out, %t);
}

function Tween::ease_elastic_inout (%this, %t)
{
   %this.inOutFunction(ease_elastic_in, ease_elastic_out, %t);
}

/*
// Robert Penner's
// ActionScript 3 elastic.easeOut equation
public static function easeOut (t:Number, b:Number, c:Number, d:Number, a:Number = 0, p:Number = 0):Number {
   var s:Number;
   if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3;
   if (!a || (c > 0 && a < c) || (c < 0 && a < -c)) { a=c; s = p/4; }
   else s = p/_2PI * Math.asin (c/a);
   return (a*Math.pow(2,-10*t) * Math.sin( (t*d-s)*_2PI/p ) + c + b);
}
*/

//--------------
// back

function Tween::ease_back_in (%this, %t)
{
   %s = 1.70158;
   return %t * %t * ((%s + 1) * %t - %s);
}

function Tween::ease_back_out (%this, %t)
{
   return %this.flipFunction(ease_back_in, %t);
}

function Tween::ease_back_inout (%this, %t)
{
   return %this.inOutFunction(ease_back_in, ease_back_out, %t);
}

/*
// Robert Penner's
// ActionScript 3 back.inOut equation
public static function easeIn (t:Number, b:Number, c:Number, d:Number, s:Number = 1.70158):Number {
   return c*(t/=d)*t*((s+1)*t - s) + b;
}
public static function easeInOut (t:Number, b:Number, c:Number, d:Number, s:Number = 1.70158):Number {
   if ((t/=d*0.5) < 1) return c*0.5*(t*t*(((s*=(1.525))+1)*t - s)) + b;
   return c/2*((t-=2)*t*(((s*=(1.525))+1)*t + s) + 2) + b;
}
*/

//--------------
// bounce

function Tween::ease_bounce_out (%this, %t)
{
   if (%t < (1/2.75)) {
      return 7.5625 * %t * %t;
   } else if (%t < (2/2.75)) {
      %t -= 1.5/2.75;
      return 7.5625 * %t * %t + 0.75;
   } else if (%t < (2.5/2.75)) {
      %t -= 2.25/2.75;
      return 7.5625 * %t * %t + 0.9375;
   } else {
      %t -= 2.625/2.75;
      return 7.5625 * %t * %t + 0.984375;
   }
}

function Tween::ease_bounce_in(%this, %t)
{
   return %this.flipFunction(ease_bounce_out, %t);
}

function Tween::ease_bounce_inout(%this, %t)
{
   return %this.inOutFunction(ease_bounce_in, ease_bounce_out, %t);
}

/*
// Robert Penner's
// ActionScript 3 bounce.out equation
   public static function easeOut (t:Number, b:Number, c:Number, d:Number):Number {
      if ((t/=d) < (1/2.75)) {
         return c*(7.5625*t*t) + b;
      } else if (t < (2/2.75)) {
         return c*(7.5625*(t-=(1.5/2.75))*t + .75) + b;
      } else if (t < (2.5/2.75)) {
         return c*(7.5625*(t-=(2.25/2.75))*t + .9375) + b;
      } else {
         return c*(7.5625*(t-=(2.625/2.75))*t + .984375) + b;
      }
   }
*/

// ---------------------------------------------
