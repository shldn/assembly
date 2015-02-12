#pragma strict

/* a variable to control the speed */
 
public  var  speedFactor : float = 2.0f;
 
/* the transform you want to rotate */
 
public  var  someThing   : Transform;
 
function LateUpdate ()
{
    processTouchInput();
}
 
function processTouchInput ()
{
    if ( ! someThing ) {
        Debug.LogError("Thing is null");
        return;
    }
 
    /* only rotate for one touch */
 
    if ( Input.touchCount != 1 ) {
        return;
    }
 
    var firstFinger : Touch = Input.GetTouch(0);
 
    /* only deal with input if the finger has moved */
 
    if ( firstFinger.phase != TouchPhase.Moved ) {
        return;
    }
 
    /* get the current rotation of someThing's transform as a vector */
 
    var theRotation : Vector3 = someThing.transform.rotation.eulerAngles;
 
    /* calculate a change in rotation based on touch movement */
 
    var movement : Vector2 = firstFinger.deltaPosition;
    var deltaY   : float   = movement.y * speedFactor * Time.deltaTime;
 	var deltaX	: float = movement.x * speedFactor * Time.deltaTime;
    /* adjust the y-coordinate of the rotation vector */
 
    theRotation.y += deltaY;
 	theRotation.x += deltaX;
    /* set the rotation of someThing to the adjusted value */
 
    someThing.transform.rotation  =  Quaternion.Euler(theRotation);
}